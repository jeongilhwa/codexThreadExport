# Development Guidelines

This file adapts the shared global `AGENTS.md` guidance to the current repository.

## Project Structure And Module Organization

- The current app already lives at the repository root.
- If a planned migration happens later, prefer `src/` for application code and `tests/` or sibling `*.Tests` projects for automated tests.
- Do not treat `bin/`, `obj/`, or other build output as authored source.

## Build, Test, And Development Commands

- Restore or build the local app with:

```powershell
dotnet build .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0 -v minimal
```

- Run the local Windows target with:

```powershell
dotnet run --project .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0
```

- No dedicated test project exists yet. When one is added, prefer the narrowest `dotnet test` target that covers the changed behavior.

## Coding Style And Naming

- Use 4-space indentation for C#.
- Prefer file-scoped namespaces, nullable reference types, and `Async` suffixes for asynchronous methods.
- Use `PascalCase` for types and public members.
- Use `camelCase` for locals and parameters.
- Use `_camelCase` for private fields where the existing project already does so.

## Testing Guidelines

- Add tests for new behavior and regression fixes when the repo includes a test project.
- Name test files after the target type, for example `CodexThreadStoreTests.cs`.
- Prefer test method names that describe behavior and condition clearly.

## Commit And Review Expectations

- Use short imperative commit messages.
- Keep unrelated changes out of the same commit.
- For review-ready changes, capture impacted areas and validation steps.

## Security And Configuration

- Never commit secrets, tokens, cookies, or machine-specific configuration.
- Treat local `.codex` data and exported thread snapshots as user data, not repo assets.
- Keep privileged behavior in MAUI or C# rather than browser-side code when possible.
