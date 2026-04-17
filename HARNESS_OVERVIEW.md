# Harness Overview

In this repository, "harness" means the operating surface around the app rather than a separate runtime.

## What The Harness Does Here

- Routes work to the narrowest agent before implementation expands.
- Records multi-step work in `docs/exec-plans/`.
- Separates repo promises from current repo drift.
- Defines future mechanical checks before adding automation.
- Keeps docs, ownership, and operating boundaries readable from the repo itself.

## Prompt Flow

1. Read `README.md` and `AGENTS.md`.
2. Route the request to an agent file under `.codex/agents/`.
3. Apply a skill from `.codex/skills/` if the task matches a repeatable workflow.
4. Create or update an active plan when the change spans multiple areas.
5. Update docs, templates, or code.
6. Re-check invariants and known drift before closing the task.

## Why It Matters In This Repo

The app already exists, so the harness is used to stabilize future changes without forcing a risky immediate restructure.
