# 하네스 부트스트랩 가이드

## 목적

이 문서는 지금 이 저장소에 적용한 `하네스 운영 구조`를 다른 저장소나 다른 폴더에 다시 적용할 수 있도록 정리한 복사용 가이드다.

핵심은 이것이다.

- 앱 프로젝트부터 만들지 않는다
- 먼저 하네스 운영 구조를 세운다
- `AGENTS`는 짧게 유지한다
- `docs/`를 진실의 원천으로 둔다
- 기계적 강제와 drift 관리를 따로 설계한다

## 이 가이드가 전제하는 저장소 성격

이 구조는 `제품 코드 저장소`가 아니라 `하네스 운영 저장소`를 먼저 만드는 방식이다.

즉, 먼저 만드는 것은 이런 것들이다.

- 루트 안내 문서
- 역할 라우팅
- docs 지식 구조
- 실행 계획 구조
- 기계적 강제 정책
- drift 관리 체계
- 템플릿

아직 만들지 않는 것은 이런 것들이다.

- `.sln`
- `.csproj`
- `src/`
- 앱 코드
- API
- 자동 실행 엔진

## 최상위 원칙

1. 사람은 방향을 잡고, 에이전트는 수행한다.
2. `AGENTS.md`는 긴 매뉴얼이 아니라 짧은 목차다.
3. 상세한 설명은 `docs/`에 둔다.
4. 반복되는 규칙은 나중에 기계적으로 점검할 수 있어야 한다.
5. drift는 문서화하고 분류하고 수정한다.
6. 구현 전에 운영 표면을 안정화한다.

## 기본 구조

```text
.
|-- README.md
|-- README.ko.md
|-- AGENTS.md
|-- AGENTS.ko.md
|-- ARCHITECTURE.md
|-- ARCHITECTURE.ko.md
|-- HARNESS_OVERVIEW.md
|-- HARNESS_OVERVIEW.ko.md
|-- .codex/
|   |-- agents/
|   `-- skills/
|-- docs/
|   |-- design-docs/
|   |-- exec-plans/
|   |-- governance/
|   |-- mechanical-enforcement/
|   |-- drift-management/
|   |-- product-specs/
|   |-- references/
|   |-- QUALITY_SCORE.md
|   |-- QUALITY_SCORE.ko.md
|   |-- RELIABILITY.md
|   |-- RELIABILITY.ko.md
|   |-- SECURITY.md
|   `-- SECURITY.ko.md
|-- templates/
|-- scripts/
|-- workspace/
|-- logs/
`-- config/
```

## 루트 파일 역할

### README.md / README.ko.md

- 저장소가 무엇인지 설명
- 읽는 순서 제공
- 이 저장소가 앱이 아니라 운영 하네스라는 점을 명확히 함

### AGENTS.md / AGENTS.ko.md

- 짧은 작업 합의서
- 역할 라우팅
- 문서 라우팅
- 기계적 강제와 drift 관리 원칙

### ARCHITECTURE.md / ARCHITECTURE.ko.md

- 저장소의 상위 레이어 설명
- 어떤 폴더가 무슨 역할인지 요약

### HARNESS_OVERVIEW.md / HARNESS_OVERVIEW.ko.md

- 하네스 엔지니어링이 이 저장소에서 무엇을 뜻하는지 설명

## .codex 구조

### `.codex/agents`

역할 문서를 둔다.

- `planner`: 범위 정리, 계획 수립, 분해
- `coder`: 저장소 변경 수행
- `reviewer`: 리스크 중심 검토
- `doc-gardener`: 문서 정리와 drift 정정

### `.codex/skills`

반복 절차를 둔다.

- `task-intake`: 작업 접수
- `repo-structure-review`: 저장소 구조 점검
- `drift-review`: drift 검토
- `doc-gardening`: 문서 정리

## docs 구조

### `docs/design-docs`

- 저장소 철학
- 구조 맵
- 읽기 인덱스

### `docs/exec-plans`

- active 계획
- completed 계획
- tech debt 추적

### `docs/governance`

- 결정 로그
- 소유권 맵
- 운영 경계

### `docs/mechanical-enforcement`

- 어떤 규칙을 나중에 기계적으로 강제할지 정의
- 불변 조건
- 점검 카탈로그
- 도입 순서

### `docs/drift-management`

- drift 정의
- 탐지 정책
- 수정 플레이북
- 문서 정리 플레이북

### `docs/product-specs`

- 아직 비워두거나 README만 둔다
- 실제 제품이 승인되기 전까지는 사용하지 않는다

### `docs/references`

- 외부 참고를 저장소 언어로 정리해 두는 곳

## templates 구조

### `TASK_SCHEMA.json`

- 작업을 어떤 형식으로 정의할지 결정하는 규격

### 실행 계획, drift 보고서, 결정 로그 템플릿

- 계획과 보고를 표준화한다

## support 폴더 역할

### `scripts`

- 나중에 자동화가 들어올 자리
- 지금은 README만 둔다

### `workspace`

- 미래 worktree 또는 sandbox 작업 영역 자리

### `logs`

- 미래 로그 자리

### `config`

- 미래 설정 파일 자리

## 지금 이 구조에서 프롬프트가 들어오면 생기는 흐름

현재는 자동 실행 엔진이 없다. 그래서 흐름은 아래처럼 간다.

1. 프롬프트가 들어온다
2. `README`와 `AGENTS`를 읽고 저장소 성격을 해석한다
3. 프롬프트 성격에 따라 역할을 고른다
4. 필요하면 `.codex/skills` 중 하나를 적용한다
5. 작업이 크면 `docs/exec-plans/active/`에 계획을 만든다
6. 관련 문서나 템플릿을 수정한다
7. `mechanical-enforcement`와 `drift-management` 기준으로 누락을 점검한다
8. 결과를 반환한다

한 줄로 요약하면:

`프롬프트 -> 라우팅 -> 계획 -> 문서/규칙 수정 -> drift/품질 점검 -> 결과`

## 다른 저장소에 적용하는 순서

### 1단계

루트에 아래 파일부터 만든다.

- `README.md`
- `AGENTS.md`
- `ARCHITECTURE.md`
- `HARNESS_OVERVIEW.md`

필요하면 바로 `*.ko.md`도 같이 만든다.

### 2단계

`.codex/agents`와 `.codex/skills`를 만든다.

이 단계에서 역할 경계를 먼저 정한다.

### 3단계

`docs/` 하위 구조를 만든다.

최소 필요 영역:

- `design-docs`
- `exec-plans`
- `governance`
- `mechanical-enforcement`
- `drift-management`

### 4단계

`templates/`를 만든다.

최소 필요 템플릿:

- task schema
- exec plan template
- drift report template
- decision log template

### 5단계

`scripts`, `workspace`, `logs`, `config` 폴더를 만든다.

지금은 실행 파일을 넣지 말고 README만 둔다.

### 6단계

초기 운용을 시작한다.

- 새 요청이 들어오면 `task-intake`
- 구조 점검은 `repo-structure-review`
- drift 점검은 `drift-review`
- 문서 정리는 `doc-gardening`

## 최소 적용 버전

다른 곳에 빨리 적용하려면 최소 이 파일들만 먼저 복사해도 된다.

- `README.md`
- `AGENTS.md`
- `ARCHITECTURE.md`
- `HARNESS_OVERVIEW.md`
- `.codex/agents/*`
- `.codex/skills/*`
- `docs/mechanical-enforcement/*`
- `docs/drift-management/*`
- `templates/TASK_SCHEMA.json`

## 전체 적용 버전

지금 이 저장소처럼 그대로 옮기고 싶으면 아래를 통째로 복사하면 된다.

- 루트 안내 문서 전체
- `.codex/agents`
- `.codex/skills`
- `docs/` 전체
- `templates/` 전체
- `scripts/logs/workspace/config` README

## 다른 저장소에 적용할 때 바꿔야 하는 부분

다음 항목은 대상 저장소에 맞게 꼭 수정해야 한다.

- 역할 이름과 경계
- ownership-map
- invariants
- checks-catalog
- drift definition
- references
- product-specs 사용 여부

## 아직 하지 말아야 하는 것

이 구조를 복사한 직후 바로 구현 쪽으로 들어가면 다시 저장소가 무너질 가능성이 크다.

복사 직후에는 먼저 아래만 한다.

1. 역할 확인
2. docs 읽기 순서 확인
3. invariants 보정
4. drift 정의 보정
5. 첫 active plan 작성

## 이 구조를 쓸 때 좋은 프롬프트 패턴

### 문서 작업

`이 저장소는 하네스 운영 저장소다. AGENTS와 docs 기준으로 역할을 고르고, 필요한 문서만 수정해라.`

### 구조 점검

`현재 저장소 구조를 architecture-map, invariants, ownership-map 기준으로 점검하고 drift를 보고해라.`

### drift 수정

`docs와 실제 구조 사이의 drift를 분류하고, remediation-playbook 기준으로 수정해라.`

## 다음 확장 단계

운영 구조가 안정되면 그다음에만 확장한다.

1. 기계적 점검 자동화
2. task runner
3. worktree 관리
4. validator
5. retry loop

## 한 줄 결론

이 구조는 `앱을 만들기 위한 저장소`가 아니라,  
`앱을 만들기 전에 AI와 사람이 같이 일할 운영 체계를 먼저 세우는 저장소`다.
