# 2026-04-17 Harness Bootstrap

## Status

Completed

## Objective

Apply the harness bootstrap structure to the existing `CodexThreadViewer` repository without disrupting user-owned app edits.

## Scope

- Add root operating docs and their Korean mirrors where required.
- Add `.codex/agents` and `.codex/skills`.
- Add `docs/`, `templates/`, `scripts/`, `workspace/`, `logs/`, and `config/` scaffolding.
- Record the current repo drift introduced by the pre-existing root-level app layout.

## Out Of Scope

- Moving the app into `src/`
- Refactoring user-modified app features
- Adding automated validation or CI
- Creating a new test project

## Deliverables

- Root docs
- Agent and skill routing files
- Governance, mechanical-enforcement, and drift-management docs
- Initial templates and support-folder READMEs

## Notes

- Existing uncommitted changes in `Components/Pages/Home.razor`, `Components/Pages/Home.razor.css`, `Models/CodexThreadSnapshot.cs`, and `Services/CodexThreadStore.cs` were left untouched.
