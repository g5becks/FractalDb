[stratadb](../index.md) / defaultShouldRetry

# Function: defaultShouldRetry()

```ts
function defaultShouldRetry(context): boolean;
```

Defined in: [src/retry-utils.ts:43](https://github.com/g5becks/StrataDb/blob/56b93c15dc2c602cd539356668e05ed574e9a8c7/src/retry-utils.ts#L43)

Default retry logic for database operations.
Determines which errors should trigger a retry attempt.

## Parameters

### context

[`RetryContext`](../type-aliases/RetryContext.md)

Retry context containing error and attempt information

## Returns

`boolean`

true if the operation should be retried, false otherwise

## Example

```typescript
const options: RetryOptions = {
  retries: 3,
  shouldRetry: defaultShouldRetry
}
```
