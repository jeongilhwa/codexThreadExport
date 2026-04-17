# Checks Catalog

## Manual Checks In Use

- Root doc presence
- AGENTS English/Korean mirror presence
- `.codex/agents` and `.codex/skills` presence
- build-output directories excluded from authored repo changes
- Drift register updated when structure promises change

## Candidate Automated Checks

- Verify required root docs exist.
- Verify `AGENTS.md` and `AGENTS.ko.md` both exist when either changes.
- Verify `docs/drift-management/current-drift-register.md` exists.
- Verify `docs/exec-plans/active/` contains a plan for large repo-shape changes.
- Verify authored changes do not include `bin/` or `obj/`.
