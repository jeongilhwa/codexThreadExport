# CodexThreadViewer Harness Bootstrap

This repository contains a local `.NET MAUI` + `Blazor Hybrid` app for browsing Codex thread data. It now also includes a repo-local harness layer so future work can be routed, documented, and checked before large structural edits are made.

## Current State

- Application code already exists at the repository root in `CodexThreadViewer.csproj`, `Components/`, `Services/`, `Models/`, `Platforms/`, and `wwwroot/`.
- Harness metadata now lives under `.codex/`, `docs/`, `templates/`, `scripts/`, `workspace/`, `logs/`, and `config/`.
- The missing `src/` boundary is an acknowledged repo drift item and is tracked in `docs/drift-management/current-drift-register.md`.

## Read In Order

1. `README.md`
2. `AGENTS.md`
3. `ARCHITECTURE.md`
4. `HARNESS_OVERVIEW.md`
5. `docs/design-docs/README.md`
6. `docs/governance/development-guidelines.md`
7. `docs/exec-plans/active/2026-04-17-harness-bootstrap.md`

## Build

```powershell
dotnet build .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0 -v minimal
```

## Run

Open `CodexThreadViewer.sln` in Visual Studio, select **Windows Machine** as the launch target, and press **F5**.

To run from PowerShell instead:

```powershell
dotnet run --project .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0
```
