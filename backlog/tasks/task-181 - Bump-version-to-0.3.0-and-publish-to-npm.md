---
id: task-181
title: Bump version to 0.3.0 and publish to npm
status: To Do
assignee: []
created_date: '2025-11-23 07:31'
labels:
  - release
  - phase-5
  - v0.3.0
dependencies:
  - task-180
priority: high
---

## Description

<!-- SECTION:DESCRIPTION:BEGIN -->
Update package.json version to 0.3.0, create git commit and tag, push to remote, and publish to npm.
<!-- SECTION:DESCRIPTION:END -->

## Acceptance Criteria
<!-- AC:BEGIN -->
- [ ] #1 Update version in package.json to '0.3.0'
- [ ] #2 Run `bun run build` to generate fresh dist files
- [ ] #3 Verify dist/*.d.ts files include new types
- [ ] #4 Create git commit with message 'chore: release v0.3.0'
- [ ] #5 Create git tag v0.3.0
- [ ] #6 Push commit and tag to remote
- [ ] #7 Run `npm publish` to publish to npm registry
- [ ] #8 Verify package is accessible on npmjs.com
<!-- AC:END -->
