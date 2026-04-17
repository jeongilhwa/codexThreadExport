# Repository Working Agreement

## Language Sync

- `AGENTS.md`는 이 저장소의 기준이 되는 영어 버전이다.
- `AGENTS.ko.md`는 같은 정책, 섹션 순서, 라우팅 항목, 규칙 범위를 그대로 반영해야 한다.
- 두 파일은 항상 같은 수정에서 함께 갱신한다.

## Goals

- `.codex/agents`를 역할별 동작과 명확한 소유권에 사용한다.
- `.codex/skills`를 반복 가능한 .NET 워크플로에 사용한다.
- 계획된 마이그레이션이 승인되면 애플리케이션 코드는 `src/` 아래를 선호한다.
- 두 구현 에이전트가 동시에 같은 `.csproj`를 수정하지 않도록 한다.

## Technology Stack

- 클라이언트는 `.NET MAUI` 애플리케이션으로 유지한다.
- UI는 `Blazor Hybrid`를 우선 사용한다.
- 프런트엔드는 먼저 Razor, HTML, CSS로 다룬다.
- JavaScript는 최소한의 브라우저 전용 동작에만 사용한다.
- 네이티브 연동은 Razor 컴포넌트가 아니라 MAUI 플랫폼 코드에 둔다.

## Default Operating Rules

- 편집 전에 워크스페이스를 점검한다.
- 사용자 변경을 덮어쓰지 않는다.
- 넓은 재작성보다 좁은 변경을 우선한다.
- 자동화를 추가할 때는 관련 skill 파일에 실행 방법을 문서화한다.
- 작업은 가장 좁게 안전하게 처리할 수 있는 에이전트로 라우팅한다.
- 구조, 소유권, 운영 규칙이 바뀌면 같은 수정에서 관련 문서를 함께 갱신한다.
- 명시적인 계획 없이는 현재 루트 앱을 `src/`로 옮기지 않는다.

## Agent Routing

- 디스패치나 조정이 필요한 코딩 작업: `.codex/agents/coder.md`
- 계획 수립, 분해, 의존성 매핑: `.codex/agents/planner.md`
- API, 모델, 서비스, 플랫폼, 애플리케이션 로직: `.codex/agents/backendcoder.md`
- Razor 컴포넌트, CSS, 레이아웃, 최소한의 JS interop: `.codex/agents/frontend.md`
- 테스트 작성과 수리: `.codex/agents/test-writer.md`
- 리스크 검토, 회귀 검토, 릴리스 준비도: `.codex/agents/reviewer.md`
- 문서 정리, drift 수정, 문서 정합성 유지: `.codex/agents/doc-gardener.md`

## Multi-Agent Conventions

- `planner`는 스타일 문제가 아니라 프로젝트나 폴더 경계로 작업을 나눈다.
- `backendcoder`는 제품 로직, 플랫폼 코드, 프로젝트 파일 수정을 맡는다.
- `frontend`는 Razor 컴포넌트, CSS, 레이아웃, 최소한의 JS interop을 맡는다.
- `test-writer`는 테스트 프로젝트와 픽스처 업데이트를 맡는다.
- `reviewer`는 요청이 없는 한 구현을 하지 않는다.
- 같은 `.csproj`를 여러 구현 에이전트에 병렬로 할당하지 않는다.
- 앱 코드와 테스트가 모두 바뀌는 경우 제품 코드를 먼저 끝내고 `test-writer`로 넘긴다.
- `coder`는 코딩 요청을 `backendcoder` 또는 `frontend`로 디스패치해 프로젝트 편집 충돌을 막는다.

## Skill Routing

- 작업 접수와 크기 산정: `.codex/skills/task-intake.md`
- 구조 점검: `.codex/skills/repo-structure-review.md`
- drift 검토와 수정: `.codex/skills/drift-review.md`
- 문서 정리: `.codex/skills/doc-gardening.md`
- 빌드, 복원, 솔루션 점검: `.codex/skills/dotnet-build.md`
- 테스트 실행과 실패 분류: `.codex/skills/run-tests.md`

## Document Routing

- 저장소 형태와 레이어 관련 질문: `ARCHITECTURE.md`와 `docs/design-docs/`
- 소유권과 운영 경계: `docs/governance/`
- 빌드, 테스트, 스타일, 전달 규칙: `docs/governance/development-guidelines.md`
- 기계적 점검과 불변 조건: `docs/mechanical-enforcement/`
- 알려진 drift와 수정 기준: `docs/drift-management/`
- 활성 작업 계획: `docs/exec-plans/`
- 제품 전용 메모: `docs/product-specs/`

## Repo Expectations

- 메인 클라이언트는 MAUI Blazor Hybrid 구조를 선호한다.
- 향후 테스트 프로젝트는 `tests/` 또는 형제 `*.Tests` 프로젝트에 둔다.
- `bin/`, `obj/` 같은 빌드 산출물은 커밋하지 않는다.
- 이 파일의 지침은 실제 저장소 도구 체인과 맞아야 한다.

## Drift And Enforcement

- 저장소 불변 조건은 `docs/mechanical-enforcement/invariants.md`에서 관리한다.
- 알려진 저장소 drift는 `docs/drift-management/current-drift-register.md`에서 관리한다.
- 문서나 폴더의 약속이 더 이상 사실이 아니면, 같은 수정에서 저장소를 고치거나 drift를 기록한다.
