---
id: task-189
title: Add event methods to Collection type interface
status: Done
assignee:
  - '@agent'
created_date: '2026-01-09 15:15'
updated_date: '2026-01-09 16:14'
labels:
  - feature
  - events
  - types
dependencies:
  - task-188
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update src/collection-types.ts to add event-related methods to the Collection type interface. This defines the public API for collection events.

## Instructions

1. Open `src/collection-types.ts`
2. Add import for event types from `./collection-events.js`:
   - CollectionEventName
   - CollectionEventMap
3. Add the following methods to the Collection<T> type:
   - `on<E extends CollectionEventName>(event: E, listener: (...args: CollectionEventMap<T>[E]) => void): this`
   - `once<E extends CollectionEventName>(event: E, listener: (...args: CollectionEventMap<T>[E]) => void): this`
   - `off<E extends CollectionEventName>(event: E, listener: (...args: CollectionEventMap<T>[E]) => void): this`
   - `removeAllListeners(event?: CollectionEventName): this`
   - `listenerCount(event: CollectionEventName): number`
   - `listeners<E extends CollectionEventName>(event: E): ((...args: CollectionEventMap<T>[E]) => void)[]`
4. Add full TSDoc comments with @example for each method
5. Commit changes after completion

## Reference
See COLLECTION_EVENTS_DESIGN.md lines 232-318 for exact type signatures and examples.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Import statement for event types added to collection-types.ts
- [x] #2 on() method added with correct generic signature
- [x] #3 once() method added with correct generic signature
- [x] #4 off() method added with correct generic signature
- [x] #5 removeAllListeners() method added
- [x] #6 listenerCount() method added
- [x] #7 listeners() method added with correct return type
- [x] #8 All methods have TSDoc comments with @example
- [x] #9 bun run typecheck passes with zero errors
- [x] #10 bun run lint:fix produces no new warnings or errors
- [x] #11 Changes are committed with descriptive message
<!-- AC:END -->
