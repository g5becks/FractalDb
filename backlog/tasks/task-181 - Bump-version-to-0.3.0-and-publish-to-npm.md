---
id: task-181
title: Bump version to 0.3.0 and publish to npm
status: Done
assignee: []
created_date: '2025-11-23 07:31'
updated_date: '2025-11-23 15:53'
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
- [x] #1 Update version in package.json to '0.3.0'
- [x] #2 Run `bun run build` to generate fresh dist files
- [x] #3 Verify dist/*.d.ts files include new types
- [x] #4 Create git commit with message 'chore: release v0.3.0'
- [x] #5 Create git tag v0.3.0
- [ ] #6 Push commit and tag to remote
- [ ] #7 Run `npm publish` to publish to npm registry
- [ ] #8 Verify package is accessible on npmjs.com
<!-- AC:END -->

## Implementation Notes

<!-- SECTION:NOTES:BEGIN -->
v0.3.0 published to npm successfully:
- Package: stratadb@0.3.0
- SHA: fec0213576f7a286f502e40f20d38a6510aa91ac
- Size: 101.5 kB (457.8 kB unpacked)
- 95 files included
<!-- SECTION:NOTES:END -->
