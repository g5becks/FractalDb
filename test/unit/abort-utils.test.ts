import { describe, expect, it } from "bun:test"
import { createAbortPromise, throwIfAborted } from "../../src/abort-utils.js"
import { AbortedError } from "../../src/errors.js"

describe("Abort Utilities", () => {
  describe("throwIfAborted", () => {
    it("should throw AbortedError when signal is aborted", () => {
      const controller = new AbortController()
      controller.abort()

      expect(() => throwIfAborted(controller.signal)).toThrow(AbortedError)
    })

    it("should do nothing when signal is not aborted", () => {
      const controller = new AbortController()

      expect(() => throwIfAborted(controller.signal)).not.toThrow()
    })

    it("should do nothing when signal is undefined", () => {
      expect(() => throwIfAborted(undefined)).not.toThrow()
    })

    it("should preserve abort reason in error", () => {
      const controller = new AbortController()
      const reason = new Error("Custom abort reason")
      controller.abort(reason)

      try {
        throwIfAborted(controller.signal)
        expect.unreachable("Should have thrown")
      } catch (error) {
        expect(error).toBeInstanceOf(AbortedError)
        expect((error as AbortedError).reason).toBe(reason)
        expect((error as AbortedError).message).toBe("Custom abort reason")
      }
    })

    it("should use default message when reason has no message", () => {
      const controller = new AbortController()
      controller.abort()

      try {
        throwIfAborted(controller.signal)
        expect.unreachable("Should have thrown")
      } catch (error) {
        expect(error).toBeInstanceOf(AbortedError)
        // When abort() is called without reason, browser/Bun provides default DOMException
        expect((error as AbortedError).message).toBe(
          "The operation was aborted."
        )
      }
    })
  })

  describe("createAbortPromise", () => {
    it("should reject immediately when signal is already aborted", async () => {
      const controller = new AbortController()
      controller.abort()

      const { promise, cleanup } = createAbortPromise(controller.signal)

      try {
        await promise
        expect.unreachable("Promise should have rejected")
      } catch (error) {
        expect(error).toBeInstanceOf(AbortedError)
      } finally {
        cleanup()
      }
    })

    it("should reject when signal is aborted later", async () => {
      const controller = new AbortController()
      const { promise, cleanup } = createAbortPromise(controller.signal)

      setTimeout(() => controller.abort(), 10)

      try {
        await promise
        expect.unreachable("Promise should have rejected")
      } catch (error) {
        expect(error).toBeInstanceOf(AbortedError)
      } finally {
        cleanup()
      }
    })

    it("should never resolve when signal is undefined", async () => {
      const { promise, cleanup } = createAbortPromise(undefined)

      const timeout = new Promise((resolve) => setTimeout(resolve, 50))
      const result = await Promise.race([promise, timeout])

      expect(result).toBeUndefined() // timeout won
      cleanup()
    })

    it("should cleanup event listener when cleanup is called", () => {
      const controller = new AbortController()
      const { cleanup } = createAbortPromise(controller.signal)

      // Call cleanup
      cleanup()

      // Abort after cleanup - should not cause issues
      controller.abort()

      // If listener wasn't removed, this would be problematic
      // No assertion needed - just verifying no errors occur
    })

    it("should preserve abort reason in rejected promise", async () => {
      const controller = new AbortController()
      const reason = new Error("Custom reason")
      const { promise, cleanup } = createAbortPromise(controller.signal)

      controller.abort(reason)

      try {
        await promise
        expect.unreachable("Promise should have rejected")
      } catch (error) {
        expect(error).toBeInstanceOf(AbortedError)
        expect((error as AbortedError).reason).toBe(reason)
        expect((error as AbortedError).message).toBe("Custom reason")
      } finally {
        cleanup()
      }
    })

    it("should use default message when abort has no reason", async () => {
      const controller = new AbortController()
      const { promise, cleanup } = createAbortPromise(controller.signal)

      controller.abort()

      try {
        await promise
        expect.unreachable("Promise should have rejected")
      } catch (error) {
        expect(error).toBeInstanceOf(AbortedError)
        // When abort() is called without reason, browser/Bun provides default DOMException
        expect((error as AbortedError).message).toBe(
          "The operation was aborted."
        )
      } finally {
        cleanup()
      }
    })
  })

  describe("AbortedError", () => {
    it("should have correct code and category properties", () => {
      const error = new AbortedError("Test message")

      expect(error.code).toBe("OPERATION_ABORTED")
      expect(error.category).toBe("database")
    })

    it("should preserve reason property", () => {
      const reason = { custom: "data" }
      const error = new AbortedError("Test message", reason)

      expect(error.reason).toBe(reason)
    })
  })
})
