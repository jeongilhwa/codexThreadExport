# CodexThreadViewer 하네스 부트스트랩

이 저장소는 Codex 스레드 데이터를 탐색하는 로컬 `.NET MAUI` + `Blazor Hybrid` 앱을 포함한다. 동시에 큰 구조 변경 전에 작업을 라우팅하고 문서화하고 점검할 수 있도록 저장소 로컬 하네스 레이어를 함께 둔다.

## 현재 상태

- 앱 코드는 이미 저장소 루트의 `CodexThreadViewer.csproj`, `Components/`, `Services/`, `Models/`, `Platforms/`, `wwwroot/` 아래에 있다.
- 하네스 메타데이터는 이제 `.codex/`, `docs/`, `templates/`, `scripts/`, `workspace/`, `logs/`, `config/` 아래에 둔다.
- `src/` 경계가 없는 점은 현재 저장소 drift로 인정하며 `docs/drift-management/current-drift-register.md`에서 관리한다.

## 읽는 순서

1. `README.ko.md`
2. `AGENTS.ko.md`
3. `ARCHITECTURE.ko.md`
4. `HARNESS_OVERVIEW.ko.md`
5. `docs/design-docs/README.md`
6. `docs/governance/development-guidelines.md`
7. `docs/exec-plans/active/2026-04-17-harness-bootstrap.md`

## 빌드

```powershell
dotnet build .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0 -v minimal
```

## 실행

Visual Studio에서 `CodexThreadViewer.sln`을 열고, 시작 대상을 **Windows Machine**으로 선택한 다음 **F5**를 누른다.

PowerShell에서는 다음 명령으로 실행할 수 있다.

```powershell
dotnet run --project .\CodexThreadViewer.csproj -f net10.0-windows10.0.19041.0
```
