# Invariants

1. `AGENTS.md` and `AGENTS.ko.md` must stay aligned in section order and rule coverage.
2. Root operating docs must exist: `README`, `AGENTS`, `ARCHITECTURE`, and `HARNESS_OVERVIEW`.
3. Harness docs must not silently promise a `src/` layout while the app still lives at the root.
4. Multi-area repo changes should have an active plan.
5. Known repo/document mismatches must be recorded in the drift register.
6. Build output such as `bin/` and `obj/` must not be treated as source or committed as authored work.
