---
id: task-175
title: Add RetryableError type and ResilienceOptions
status: Done
assignee:
  - '@claude'
created_date: '2025-12-31 19:04'
updated_date: '2025-12-31 19:16'
labels: []
dependencies: []
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Add RetryableError DU with presets (defaults, extended, all) and ResilienceOptions record type for configuring retry behavior
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [x] #1 Add RetryableError discriminated union with Busy, Locked, IOError, CantOpen, Connection, Transaction cases
- [x] #2 Add RetryableError.defaults preset (Busy, Locked)
- [x] #3 Add RetryableError.extended preset (includes IOError, CantOpen)
- [x] #4 Add RetryableError.all preset
- [x] #5 Add RetryableError.matches function to check if FractalError matches RetryableError
- [x] #6 Add RetryableError.shouldRetry function
- [x] #7 Add ResilienceOptions record type with RetryOn, MaxRetries, BaseDelay, MaxDelay, ExponentialBackoff, Jitter
- [x] #8 Add ResilienceOptions.defaults with MaxRetries=2
- [x] #9 Add ResilienceOptions.none, extended, and aggressive presets
<!-- AC:END -->

## Implementation Plan

<!-- SECTION:PLAN:BEGIN -->
1. Add RetryableError DU at end of Errors.fs (before DonaldExceptions module)
2. Add RetryableError module with defaults, extended, all presets
3. Add matches and shouldRetry functions
4. Add ResilienceOptions record type
5. Add ResilienceOptions module with defaults, none, extended, aggressive presets
<!-- SECTION:PLAN:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
Added RetryableError discriminated union with 6 cases:
- Busy, Locked, IOError, CantOpen, Connection, Transaction

Added RetryableError module with:
- defaults preset (Busy, Locked)
- extended preset (adds IOError, CantOpen)
- all preset (all 6 error types)
- matches function to check if FractalError matches RetryableError
- shouldRetry function to check if error should be retried

Added ResilienceOptions record type with:
- RetryOn, MaxRetries (default 2), BaseDelay, MaxDelay, ExponentialBackoff, Jitter

Added ResilienceOptions module with:
- defaults, none, extended, aggressive presets
<!-- SECTION:NOTES:END -->
