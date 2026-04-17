# .NET Build

## Use When

- The task needs restore, build verification, or project inspection.

## Default Command

```powershell
dotnet build .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0 -v minimal
```

## Notes

- Use the Windows target for local desktop verification in this repo.
- If build output reveals a repo-structure issue, document it before broad fixes.
