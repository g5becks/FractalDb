# Document: updateOne Implementation Changes and Issues

## Current State
I was working on Task 109 - Update updateOne implementation in sqlite-collection.ts to use normalizeFilter and findOne instead of findById.

## Changes Made So Far

### 1. Updated Method Signature
- Changed parameter from `id: string` to `filter: string | QueryFilter<T>`
- Added `async` keyword since we're now using `findOne` which is async
- Added import: `import { generateId } from "./id-generator.js"`

### 2. Updated Implementation
- Added `const normalizedFilter = this.normalizeFilter(filter)`
- Changed `const existing = this.db.prepare(...).get(id)` to `const existing = await this.findOne(normalizedFilter)`
- Added logic to handle upsert with merged filter fields

## Current Type Errors

```
src/sqlite-collection.ts(597,11): error TS2322: Type 'Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">' is not assignable to type 'Partial<T>'.
```

## The Problem I Was Creating

I was overcomplicating the upsert logic by trying to force type compatibility between:
- `update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">` (the method parameter)
- `Partial<T>` (what I was trying to assign it to)

I kept trying various type casting approaches instead of working with the existing type system properly.

## What Needs to Be Done

1. **Fix the type error** on line 597 where `upsertData = update` fails
2. **Complete the upsert logic** to properly merge filter fields when filter is a QueryFilter object
3. **Test the implementation** to ensure it works correctly
4. **Verify all acceptance criteria** are met

## Acceptance Criteria Status
- ✅ Type compiles without errors (currently failing)
- ✅ Linting passes
- ✅ Uses normalizeFilter helper
- ✅ Uses findOne instead of findById
- ⏳ Upsert handles merged filter fields correctly (needs completion)

## Files Modified
- `/Users/takinprofit/Dev/stratadb/src/sqlite-collection.ts`
  - Updated updateOne method signature and implementation
  - Added generateId import

## Context
This is part of Phase 3 of the MongoDB compatibility implementation, where we're adding uniform filter support to collection methods so they can accept either string IDs or QueryFilter objects.

## Technical Details

The type error occurs because:
- Method parameter: `update: Omit<Partial<T>, "_id" | "createdAt" | "updatedAt">`
- Variable declaration: `let upsertData: Partial<T>`
- Assignment: `upsertData = update` (fails due to omitted keys)

The fix requires either:
1. Change the variable type declaration, or
2. Handle the type compatibility properly without excessive casting