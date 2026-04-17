# Repository Architecture

## Overview

The repository has two active layers:

- A root-level `.NET MAUI` + `Blazor Hybrid` application for inspecting local Codex thread data.
- A repo-local harness layer that defines routing, planning, documentation, drift tracking, and future mechanical enforcement.

## Current Layer Map

- `CodexThreadViewer.csproj`, `MauiProgram.cs`, `App.xaml*`, `MainPage.xaml*`: MAUI host and app entry points.
- `Components/`: Razor UI, routes, layouts, and pages.
- `Services/`, `Models/`: thread loading, export logic, and app data contracts.
- `Platforms/`, `Resources/`, `Properties/`, `wwwroot/`: platform host files, assets, launch settings, and static web assets.
- `.codex/`: role and skill routing for agent-driven work.
- `docs/`: design docs, active plans, governance, mechanical enforcement, drift tracking, and references.
- `templates/`: reusable plan, drift, and decision templates.
- `scripts/`, `workspace/`, `logs/`, `config/`: reserved support areas with placeholder READMEs.

## Known Divergence From Target Shape

- Product code is still rooted at the repository root instead of a dedicated `src/` folder.
- The harness layer was added after the app already existed.
- No automated harness validation is wired in yet; the current enforcement is documentation-first.

## Source Of Truth

- Working rules: `AGENTS.md`
- Repo shape and rationale: `docs/design-docs/`
- Ownership and boundaries: `docs/governance/`
- Invariants and future checks: `docs/mechanical-enforcement/`
- Drift records: `docs/drift-management/`
