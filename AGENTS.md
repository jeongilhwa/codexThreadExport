# Repository Working Agreement

## Language Sync

- `AGENTS.md` is the canonical English version in this repository.
- `AGENTS.ko.md` must mirror the same policy, section order, routing entries, and rule coverage.
- Update both files in the same edit.

## Goals

- Use `.codex/agents` for role-specific behavior with clear ownership.
- Use `.codex/skills` for repeatable .NET workflows.
- Prefer application code under `src/` when a planned migration is approved.
- Avoid two implementation agents editing the same `.csproj` at the same time.

## Technology Stack

- Build the client as a `.NET MAUI` application.
- Prefer `Blazor Hybrid` for UI.
- Treat the frontend as Razor, HTML, and CSS first.
- Use JavaScript only for minimal browser-only behavior.
- Keep native integrations in MAUI platform code, not in Razor components.

## Default Operating Rules

- Inspect the workspace before editing.
- Do not overwrite user changes.
- Prefer narrow changes over broad rewrites.
- When adding automation, document how to run it inside the relevant skill file.
- Route the task to the narrowest agent that can complete it safely.
- If structure, ownership, or operating rules change, update the relevant docs in the same edit.
- Do not move the current root app into `src/` without an explicit plan.

## Agent Routing

- Coding tasks that need dispatch or coordination: `.codex/agents/coder.md`
- Planning, decomposition, dependency mapping: `.codex/agents/planner.md`
- API, models, services, platform, and application logic: `.codex/agents/backendcoder.md`
- Razor components, CSS, layout, and minimal JS interop: `.codex/agents/frontend.md`
- Test authoring and repair: `.codex/agents/test-writer.md`
- Risk review, regression review, release readiness: `.codex/agents/reviewer.md`
- Documentation gardening, drift remediation, and doc alignment: `.codex/agents/doc-gardener.md`

## Multi-Agent Conventions

- Let `planner` split work by project or folder, not by arbitrary style concerns.
- Let `backendcoder` own production logic, platform code, and project-file edits.
- Let `frontend` own Razor components, CSS, layout, and minimal JS interop.
- Let `test-writer` own test projects and fixture updates.
- Let `reviewer` avoid implementation unless the request explicitly asks for fixes.
- Do not assign the same `.csproj` to multiple implementation agents in parallel.
- When a change touches both app code and tests, finish production edits first, then hand off to `test-writer`.
- Let `coder` dispatch coding requests to `backendcoder` or `frontend` to prevent overlapping project edits.

## Skill Routing

- Intake and sizing: `.codex/skills/task-intake.md`
- Structure review: `.codex/skills/repo-structure-review.md`
- Drift review and remediation: `.codex/skills/drift-review.md`
- Documentation cleanup: `.codex/skills/doc-gardening.md`
- Build, restore, and solution inspection: `.codex/skills/dotnet-build.md`
- Test execution and failure triage: `.codex/skills/run-tests.md`

## Document Routing

- Repo shape and layer questions: `ARCHITECTURE.md` and `docs/design-docs/`
- Ownership and operating boundaries: `docs/governance/`
- Build, test, style, and delivery conventions: `docs/governance/development-guidelines.md`
- Mechanical checks and invariants: `docs/mechanical-enforcement/`
- Known drift and remediation: `docs/drift-management/`
- Active work planning: `docs/exec-plans/`
- Product-specific notes: `docs/product-specs/`

## Repo Expectations

- Prefer a MAUI Blazor Hybrid app structure for the main client.
- Keep future test projects in `tests/` or a sibling `*.Tests` project.
- Do not commit build output such as `bin/` or `obj/`.
- Keep instructions in this file aligned with the actual toolchain in the repository.

## Drift And Enforcement

- Keep repo invariants in `docs/mechanical-enforcement/invariants.md`.
- Keep known repo drift in `docs/drift-management/current-drift-register.md`.
- When a doc or folder promise becomes false, either fix the repo or record the drift in the same edit.
