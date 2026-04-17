# Current Drift Register

## DRIFT-001 Root-Level App Layout

- Status: Open
- Summary: Product code lives at the repository root instead of a dedicated `src/` folder.
- Impact: Repo shape differs from the preferred MAUI app layout documented in prior working notes.
- Current handling: The harness docs explicitly acknowledge the root-level layout instead of pretending `src/` already exists.
- Next action: Revisit only through an explicit migration plan.

## DRIFT-002 Harness Validation Is Manual

- Status: Open
- Summary: Mechanical enforcement is documented, but no automation validates the harness structure yet.
- Impact: Rule regressions depend on manual review.
- Current handling: Rules and candidate checks live under `docs/mechanical-enforcement/`.
- Next action: Add lightweight presence checks after the repo shape settles.

## DRIFT-003 No Dedicated Test Project

- Status: Open
- Summary: The repository does not yet contain a separate test project.
- Impact: Regression coverage depends on manual verification.
- Current handling: `test-writer` and `run-tests` docs acknowledge the gap explicitly.
- Next action: Add a sibling test project when a change justifies test investment.
