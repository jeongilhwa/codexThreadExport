# Architecture Map

## App Surface

- `CodexThreadViewer.csproj`: MAUI Blazor Hybrid project file.
- `Components/`: routing, layouts, pages, and page-local CSS.
- `Services/`: local `.codex` ingestion and desktop export logic.
- `Models/`: snapshot and message data contracts.
- `Platforms/`: platform-specific host code.
- `Resources/`, `wwwroot/`, `Properties/`: assets, static web resources, and launch settings.

## Harness Surface

- `.codex/agents/`: role instructions for planning, coding, reviewing, testing, and documentation.
- `.codex/skills/`: repeatable workflows for intake, structure review, drift review, build, and tests.
- `docs/`: the long-form source of truth for governance and repo operations.
- `templates/`: standard formats for plans and reports.

## Support Surface

- `scripts/`: future automation entry points.
- `workspace/`: future isolated work area.
- `logs/`: future operational logs.
- `config/`: future repo-local configuration files.
