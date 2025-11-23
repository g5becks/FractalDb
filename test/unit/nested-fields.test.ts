/**
 * Tests for nested field support with dot notation
 */

import { describe, expect, it } from "bun:test"
import { createSchema, type Document } from "../../src/index.js"

describe("Nested Field Support", () => {
  describe("Field Definition with Dot Notation", () => {
    it("should support single-level nested fields", () => {
      type User = Document<{
        name: string
        profile?: {
          bio: string
          avatar: string
        }
      }>

      const schema = createSchema<
        Omit<User, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("profile.bio", { type: "TEXT", indexed: true })
        .field("profile.avatar", { type: "TEXT", indexed: false })
        .build()

      expect(schema.fields).toHaveLength(3)

      // Check nested field paths
      const bioField = schema.fields.find(
        (f) => String(f.name) === "profile.bio"
      )
      expect(bioField).toBeDefined()
      expect(bioField?.path).toBe("$.profile.bio")
      expect(bioField?.indexed).toBe(true)

      const avatarField = schema.fields.find(
        (f) => String(f.name) === "profile.avatar"
      )
      expect(avatarField).toBeDefined()
      expect(avatarField?.path).toBe("$.profile.avatar")
      expect(avatarField?.indexed).toBe(false)
    })

    it("should support multi-level nested fields", () => {
      type Config = Document<{
        name: string
        settings?: {
          display?: {
            theme: string
            fontSize: number
          }
        }
      }>

      const schema = createSchema<
        Omit<Config, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("settings.display.theme", { type: "TEXT", indexed: true })
        .field("settings.display.fontSize", { type: "INTEGER", indexed: false })
        .build()

      expect(schema.fields).toHaveLength(3)

      const themeField = schema.fields.find(
        (f) => String(f.name) === "settings.display.theme"
      )
      expect(themeField).toBeDefined()
      expect(themeField?.path).toBe("$.settings.display.theme")
    })

    it("should support multiple nested fields from same parent", () => {
      type Alert = Document<{
        name: string
        watchlistItem?: {
          ticker: string
          symbol: string
          exchange: string
        }
      }>

      const schema = createSchema<
        Omit<Alert, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("watchlistItem.ticker", { type: "TEXT", indexed: true })
        .field("watchlistItem.symbol", { type: "TEXT", indexed: true })
        .field("watchlistItem.exchange", { type: "TEXT", indexed: true })
        .build()

      expect(schema.fields).toHaveLength(4)

      // All nested fields should have correct paths
      const tickerField = schema.fields.find(
        (f) => String(f.name) === "watchlistItem.ticker"
      )
      const symbolField = schema.fields.find(
        (f) => String(f.name) === "watchlistItem.symbol"
      )
      const exchangeField = schema.fields.find(
        (f) => String(f.name) === "watchlistItem.exchange"
      )

      expect(tickerField?.path).toBe("$.watchlistItem.ticker")
      expect(symbolField?.path).toBe("$.watchlistItem.symbol")
      expect(exchangeField?.path).toBe("$.watchlistItem.exchange")
    })

    it("should handle top-level fields normally", () => {
      type User = Document<{
        name: string
        email: string
        age: number
      }>

      const schema = createSchema<
        Omit<User, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("email", { type: "TEXT", indexed: true, unique: true })
        .field("age", { type: "INTEGER", indexed: true })
        .build()

      expect(schema.fields).toHaveLength(3)

      // Top-level fields should have simple paths
      const nameField = schema.fields.find((f) => String(f.name) === "name")
      const emailField = schema.fields.find((f) => String(f.name) === "email")
      const ageField = schema.fields.find((f) => String(f.name) === "age")

      expect(nameField?.path).toBe("$.name")
      expect(emailField?.path).toBe("$.email")
      expect(ageField?.path).toBe("$.age")
    })

    it("should respect explicit path parameter when provided", () => {
      type User = Document<{
        name: string
        metadata?: Record<string, unknown>
      }>

      const schema = createSchema<
        Omit<User, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("custom_field", {
          path: "$.metadata.customField" as any,
          type: "TEXT",
          indexed: true,
        })
        .build()

      const customField = schema.fields.find(
        (f) => String(f.name) === "custom_field"
      )
      expect(customField?.path).toBe("$.metadata.customField")
    })

    it("should support top-level field name with nested path", () => {
      type Product = Document<{
        name: string
        metadata?: {
          warehouse?: {
            location: string
            quantity: number
          }
        }
      }>

      const schema = createSchema<
        Omit<Product, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        // Use simple name but explicit nested path
        .field("location", {
          path: "$.metadata.warehouse.location" as any,
          type: "TEXT",
          indexed: true,
        })
        .field("quantity", {
          path: "$.metadata.warehouse.quantity" as any,
          type: "INTEGER",
          indexed: true,
        })
        .build()

      expect(schema.fields).toHaveLength(3)

      const locationField = schema.fields.find(
        (f) => String(f.name) === "location"
      )
      expect(locationField).toBeDefined()
      expect(locationField?.path).toBe("$.metadata.warehouse.location")
      expect(locationField?.indexed).toBe(true)

      const quantityField = schema.fields.find(
        (f) => String(f.name) === "quantity"
      )
      expect(quantityField).toBeDefined()
      expect(quantityField?.path).toBe("$.metadata.warehouse.quantity")
      expect(quantityField?.indexed).toBe(true)
    })

    it("should support explicit path overriding dot notation", () => {
      type Config = Document<{
        name: string
        data?: Record<string, unknown>
      }>

      const schema = createSchema<
        Omit<Config, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        // Field name has dots, but explicit path overrides
        .field("settings.theme", {
          path: "$.data.user_settings.display_theme" as any,
          type: "TEXT",
          indexed: true,
        })
        .build()

      const themeField = schema.fields.find(
        (f) => String(f.name) === "settings.theme"
      )
      expect(themeField).toBeDefined()
      // Explicit path should be used, not auto-generated from field name
      expect(themeField?.path).toBe("$.data.user_settings.display_theme")
    })
  })

  describe("Compound Indexes with Nested Fields", () => {
    it("should support compound index with nested field and top-level field", () => {
      type Alert = Document<{
        name: string
        isActive: boolean
        watchlistItem?: {
          ticker: string
          symbol: string
        }
      }>

      const schema = createSchema<
        Omit<Alert, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("isActive", { type: "INTEGER", indexed: true })
        .field("watchlistItem.ticker", { type: "TEXT", indexed: true })
        .field("watchlistItem.symbol", { type: "TEXT", indexed: true })
        .compoundIndex("watchlist_active", ["watchlistItem.ticker", "isActive"])
        .build()

      expect(schema.compoundIndexes).toHaveLength(1)
      expect(schema.compoundIndexes[0].name).toBe("watchlist_active")
      expect(schema.compoundIndexes[0].fields).toEqual([
        "watchlistItem.ticker",
        "isActive",
      ])
    })

    it("should support compound index with multiple nested fields", () => {
      type Product = Document<{
        name: string
        inventory?: {
          warehouse: string
          location: string
          quantity: number
        }
      }>

      const schema = createSchema<
        Omit<Product, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("inventory.warehouse", { type: "TEXT", indexed: true })
        .field("inventory.location", { type: "TEXT", indexed: true })
        .field("inventory.quantity", { type: "INTEGER", indexed: true })
        .compoundIndex("inventory_lookup", [
          "inventory.warehouse",
          "inventory.location",
        ])
        .build()

      expect(schema.compoundIndexes).toHaveLength(1)
      expect(schema.compoundIndexes[0].fields).toEqual([
        "inventory.warehouse",
        "inventory.location",
      ])
    })

    it("should support compound index with deeply nested fields", () => {
      type Config = Document<{
        name: string
        settings?: {
          display?: {
            theme: string
            language: string
          }
        }
      }>

      const schema = createSchema<
        Omit<Config, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true })
        .field("settings.display.theme", { type: "TEXT", indexed: true })
        .field("settings.display.language", { type: "TEXT", indexed: true })
        .compoundIndex("display_settings", [
          "settings.display.theme",
          "settings.display.language",
        ])
        .build()

      expect(schema.compoundIndexes).toHaveLength(1)
      expect(schema.compoundIndexes[0].fields).toContain(
        "settings.display.theme"
      )
      expect(schema.compoundIndexes[0].fields).toContain(
        "settings.display.language"
      )
    })

    it("should support unique compound indexes with nested fields", () => {
      type User = Document<{
        email: string
        profile?: {
          tenantId: string
        }
      }>

      const schema = createSchema<
        Omit<User, "_id" | "createdAt" | "updatedAt">
      >()
        .field("email", { type: "TEXT", indexed: true })
        .field("profile.tenantId", { type: "TEXT", indexed: true })
        .compoundIndex("email_tenant", ["email", "profile.tenantId"], {
          unique: true,
        })
        .build()

      expect(schema.compoundIndexes).toHaveLength(1)
      expect(schema.compoundIndexes[0].unique).toBe(true)
      expect(schema.compoundIndexes[0].fields).toEqual([
        "email",
        "profile.tenantId",
      ])
    })
  })

  describe("Real-world Use Cases", () => {
    it("should handle Alert schema with nested watchlistItem", () => {
      type Alert = Document<{
        name: string
        priority: "low" | "medium" | "high" | "urgent"
        bias: "neutral" | "bullish" | "bearish"
        isActive: boolean
        logAlways: boolean
        isTemplate: boolean
        templateName?: string
        watchlistItem?: {
          ticker: string
          symbol: string
          bias: "neutral" | "bullish" | "bearish"
        }
      }>

      const schema = createSchema<
        Omit<Alert, "_id" | "createdAt" | "updatedAt">
      >()
        .field("name", { type: "TEXT", indexed: true, unique: true })
        .field("isActive", { type: "INTEGER", indexed: true })
        .field("isTemplate", { type: "INTEGER", indexed: true })
        .field("templateName", { type: "TEXT", indexed: true })
        .field("priority", { type: "TEXT", indexed: true })
        .field("bias", { type: "TEXT", indexed: true })
        .field("watchlistItem.ticker", { type: "TEXT", indexed: true })
        .field("watchlistItem.symbol", { type: "TEXT", indexed: true })
        .field("watchlistItem.bias", { type: "TEXT", indexed: true })
        .compoundIndex("watchlist_active", ["watchlistItem.ticker", "isActive"])
        .compoundIndex("template_name", ["isTemplate", "templateName"])
        .timestamps(true)
        .build()

      // Verify all fields exist
      expect(schema.fields).toHaveLength(9)

      // Verify nested fields have correct paths
      const tickerField = schema.fields.find(
        (f) => String(f.name) === "watchlistItem.ticker"
      )
      expect(tickerField?.path).toBe("$.watchlistItem.ticker")
      expect(tickerField?.indexed).toBe(true)

      // Verify compound indexes
      expect(schema.compoundIndexes).toHaveLength(2)
      const watchlistActiveIdx = schema.compoundIndexes.find(
        (i) => i.name === "watchlist_active"
      )
      expect(watchlistActiveIdx?.fields).toEqual([
        "watchlistItem.ticker",
        "isActive",
      ])
    })

    it("should handle User schema with nested profile settings", () => {
      type User = Document<{
        email: string
        username: string
        profile: {
          firstName: string
          lastName: string
          settings: {
            theme: "light" | "dark"
            notifications: boolean
            language: string
          }
        }
      }>

      const schema = createSchema<
        Omit<User, "_id" | "createdAt" | "updatedAt">
      >()
        .field("email", { type: "TEXT", indexed: true, unique: true })
        .field("username", { type: "TEXT", indexed: true, unique: true })
        .field("profile.firstName", { type: "TEXT", indexed: true })
        .field("profile.lastName", { type: "TEXT", indexed: true })
        .field("profile.settings.theme", { type: "TEXT", indexed: true })
        .field("profile.settings.notifications", {
          type: "INTEGER",
          indexed: false,
        })
        .field("profile.settings.language", { type: "TEXT", indexed: true })
        .compoundIndex("name_lookup", ["profile.firstName", "profile.lastName"])
        .build()

      expect(schema.fields).toHaveLength(7)

      // Verify deeply nested field
      const themeField = schema.fields.find(
        (f) => String(f.name) === "profile.settings.theme"
      )
      expect(themeField?.path).toBe("$.profile.settings.theme")

      // Verify compound index works with nested fields
      const nameLookupIdx = schema.compoundIndexes.find(
        (i) => i.name === "name_lookup"
      )
      expect(nameLookupIdx?.fields).toEqual([
        "profile.firstName",
        "profile.lastName",
      ])
    })
  })
})
