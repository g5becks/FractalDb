---
id: task-208
title: Update VitePress sidebar to include events guide
status: Done
assignee: []
created_date: '2026-01-09 15:56'
labels:
  - feature
  - events
  - documentation
dependencies:
  - task-207
priority: medium
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update the VitePress configuration to add events.md to the documentation sidebar.

## Instructions

1. Open `docs/.vitepress/config.ts`
2. Find the sidebar configuration for the guide section
3. Add an entry for events.md in an appropriate location (after collections.md or similar)
4. The sidebar entry should be:
   ```typescript
   { text: 'Events', link: '/guide/events' }
   ```
5. Verify the link works by building docs (if possible)
6. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 docs/.vitepress/config.ts updated
- [ ] #2 Events entry added to guide sidebar
- [ ] #3 Entry positioned logically in navigation
- [ ] #4 bun run lint:fix produces no new warnings or errors
- [ ] #5 Changes are committed with descriptive message
<!-- AC:END -->
