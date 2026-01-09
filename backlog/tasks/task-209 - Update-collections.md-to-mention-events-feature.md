---
id: task-209
title: Update collections.md to mention events feature
status: Done
assignee: []
created_date: '2026-01-09 15:58'
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
Update the existing collections documentation to mention and link to the new events feature.

## Instructions

1. Open `docs/guide/collections.md`
2. Add a brief mention of events capability in an appropriate section
3. Add a link to the events guide for more details
4. Example addition:
   ```markdown
   ## Events
   
   Collections emit events when documents are created, updated, or deleted. 
   This enables reactive patterns like audit logging, cache invalidation, 
   and real-time notifications.
   
   ```typescript
   users.on('insert', (event) => {
     console.log('New user:', event.document._id)
   })
   ```
   
   See the [Events Guide](/guide/events) for complete documentation.
   ```
5. Keep the addition brief - full details are in events.md
6. Commit changes after completion
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 docs/guide/collections.md updated
- [ ] #2 Brief events section added
- [ ] #3 Link to events guide included
- [ ] #4 Code example included
- [ ] #5 bun run lint:fix produces no new warnings or errors
- [ ] #6 Changes are committed with descriptive message
<!-- AC:END -->
