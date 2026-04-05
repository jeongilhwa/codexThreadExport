# codexThreadExport

Local .NET MAUI Blazor app for browsing Codex thread data stored under the current user's `.codex` folder.

## What It Does

- Loads the current local Codex thread index
- Shows thread list, message counts, and prompt-history preview
- Displays prompt-history entries for the selected thread
- Exports a new thread snapshot back to the Desktop

## Build

```powershell
dotnet build .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0 -v minimal
```

## Run

```powershell
dotnet run --project .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0
```
