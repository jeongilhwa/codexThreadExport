# Reliability

- App features that read local Codex data or export snapshots should fail clearly, not silently.
- Repo bootstrap work should avoid changing user-owned app behavior unless the task explicitly asks for it.
- Drift records should stay current so repo promises remain trustworthy.
