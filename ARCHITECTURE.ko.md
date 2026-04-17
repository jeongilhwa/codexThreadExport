# 저장소 아키텍처

## 개요

이 저장소는 두 개의 활성 레이어로 구성된다.

- 로컬 Codex 스레드 데이터를 살펴보는 루트 수준 `.NET MAUI` + `Blazor Hybrid` 애플리케이션
- 라우팅, 계획, 문서화, drift 추적, 향후 기계적 강제를 정의하는 저장소 로컬 하네스 레이어

## 현재 레이어 맵

- `CodexThreadViewer.csproj`, `MauiProgram.cs`, `App.xaml*`, `MainPage.xaml*`: MAUI 호스트와 앱 진입점
- `Components/`: Razor UI, 라우트, 레이아웃, 페이지
- `Services/`, `Models/`: 스레드 로딩, 내보내기 로직, 앱 데이터 계약
- `Platforms/`, `Resources/`, `Properties/`, `wwwroot/`: 플랫폼 호스트 파일, 에셋, 실행 설정, 정적 웹 에셋
- `.codex/`: 에이전트 작업용 역할 및 스킬 라우팅
- `docs/`: 설계 문서, 활성 계획, 거버넌스, 기계적 강제, drift 추적, 참고 자료
- `templates/`: 계획, drift, 결정 로그 템플릿
- `scripts/`, `workspace/`, `logs/`, `config/`: README만 둔 예비 지원 영역

## 목표 구조와의 차이

- 제품 코드는 아직 전용 `src/` 폴더가 아니라 저장소 루트에 있다.
- 하네스 레이어는 앱이 이미 존재한 뒤에 추가되었다.
- 하네스 검증 자동화는 아직 연결되지 않았고, 현재 강제 방식은 문서 중심이다.

## 진실의 원천

- 작업 규칙: `AGENTS.md`
- 저장소 형태와 이유: `docs/design-docs/`
- 소유권과 경계: `docs/governance/`
- 불변 조건과 향후 점검: `docs/mechanical-enforcement/`
- drift 기록: `docs/drift-management/`
