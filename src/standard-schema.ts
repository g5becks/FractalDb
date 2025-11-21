/**
 * Standard Schema v1 specification types.
 *
 * @remarks
 * This file contains the complete Standard Schema v1 interface specification.
 * Standard Schema is a common interface designed to be implemented by JavaScript
 * and TypeScript schema libraries (Zod, Valibot, ArkType, etc.).
 *
 * The specification was designed by the creators of Zod, Valibot, and ArkType
 * to make it easier for ecosystem tools to accept user-defined type validators
 * without needing custom adapters for each library.
 *
 * @see https://github.com/standard-schema/standard-schema
 *
 * @internal
 * These types are used internally by StrataDB's validator wrapper.
 *
 * NOTE: This file uses interfaces and namespaces as specified by the official
 * Standard Schema specification. Linting rules are disabled to preserve the
 * exact specification format.
 */

/** The Standard Schema interface. */
export type StandardSchemaV1<Input = unknown, Output = Input> = {
  /** The Standard Schema properties. */
  readonly "~standard": StandardSchemaV1.Props<Input, Output>
}

/* biome-ignore lint/style/noNamespace: Official Standard Schema spec uses namespaces */
export declare namespace StandardSchemaV1 {
  /** The Standard Schema properties interface. */
  export type Props<Input = unknown, Output = Input> = {
    /** The version number of the standard. */
    readonly version: 1
    /** The vendor name of the schema library. */
    readonly vendor: string
    /** Validates unknown input values. */
    readonly validate: (
      value: unknown
    ) => Result<Output> | Promise<Result<Output>>
    /** Inferred types associated with the schema. */
    readonly types?: Types<Input, Output> | undefined
  }

  /** The result interface of the validate function. */
  export type Result<Output> = SuccessResult<Output> | FailureResult

  /** The result interface if validation succeeds. */
  export type SuccessResult<Output> = {
    /** The typed output value. */
    readonly value: Output
    /** The non-existent issues. */
    readonly issues?: undefined
  }

  /** The result interface if validation fails. */
  export type FailureResult = {
    /** The issues of failed validation. */
    readonly issues: readonly Issue[]
  }

  /** The issue interface of the failure output. */
  export type Issue = {
    /** The error message of the issue. */
    readonly message: string
    /** The path of the issue, if any. */
    readonly path?: ReadonlyArray<PropertyKey | PathSegment> | undefined
  }

  /** The path segment interface of the issue. */
  export type PathSegment = {
    /** The key representing a path segment. */
    readonly key: PropertyKey
  }

  /** The Standard Schema types interface. */
  export type Types<Input = unknown, Output = Input> = {
    /** The input type of the schema. */
    readonly input: Input
    /** The output type of the schema. */
    readonly output: Output
  }

  /** Infers the input type of a Standard Schema. */
  export type InferInput<Schema extends StandardSchemaV1> = NonNullable<
    Schema["~standard"]["types"]
  >["input"]

  /** Infers the output type of a Standard Schema. */
  export type InferOutput<Schema extends StandardSchemaV1> = NonNullable<
    Schema["~standard"]["types"]
  >["output"]
}
