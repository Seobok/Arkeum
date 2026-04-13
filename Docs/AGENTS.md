# Game Narrative Harness

이 저장소는 게임 스토리, 퀘스트, 대사, 분기 시나리오 설계를 위한 내러티브 하네스다.

## 목적

게임 스토리·퀘스트·대사·분기 시나리오를 일관된 절차로 설계하고, 모든 결과물을 `_workspace/` 디렉토리에 정리한다.

## 저장소 구조

- `AGENTS.md` — 저장소 전반의 작업 규칙
- `.agents/skills/game-narrative/SKILL.md` — 전체 내러티브 파이프라인 오케스트레이션
- `.agents/skills/worldbuilding/SKILL.md` — 세계관 설계
- `.agents/skills/quest-design/SKILL.md` — 퀘스트 설계
- `.agents/skills/quest-design/references/quest-design-patterns.md` — 퀘스트 설계 방법론
- `.agents/skills/dialogue-writing/SKILL.md` — 대사 작성
- `.agents/skills/dialogue-writing/references/dialogue-systems.md` — 대사 작성 방법론
- `.agents/skills/branching-logic/SKILL.md` — 분기/엔딩/플래그 설계
- `.agents/skills/branching-logic/references/branching-patterns.md` — 분기 설계 방법론
- `.agents/skills/narrative-review/SKILL.md` — 내러티브 검증
- `_workspace/` — 산출물 저장 디렉토리

## 사용 원칙

- 게임 시나리오 전체 설계 요청에는 `game-narrative` 스킬을 우선 사용한다.
- 세계관만 필요하면 `worldbuilding`을 사용한다.
- 퀘스트만 필요하면 `quest-design`을 사용한다.
- NPC 대사, 컷신, 선택지만 필요하면 `dialogue-writing`을 사용한다.
- 분기 구조, 멀티 엔딩, 플래그 시스템만 필요하면 `branching-logic`을 사용한다.
- 전체 정합성 검토나 QA 보고서가 필요하면 `narrative-review`를 사용한다.
- 기존 `_workspace/` 문서가 있으면 불필요하게 전부 다시 쓰지 말고 필요한 부분만 수정하거나 확장한다.
- 설정 충돌, 동기 불일치, 분기 모순, 보상 밸런스 문제를 발견하면 숨기지 말고 명시적으로 보고한다.
- 용어, 세계관 규칙, 등장인물 성격, 퀘스트 목표, 선택지 의도, 엔딩 조건의 정합성을 항상 유지한다.

## 기본 워크플로우

사용자가 게임 내러티브 설계를 요청하면 아래 순서로 작업한다.

1. 사용자 요구사항을 정리하여 `_workspace/00_input.md` 에 기록한다.
2. 세계관, 배경, 세력, 역사, 규칙을 정리하여 `_workspace/01_worldbuilding.md` 에 기록한다.
3. 메인/사이드 퀘스트 구조, 목표, 보상, 진행 흐름을 정리하여 `_workspace/02_quest_design.md` 에 기록한다.
4. NPC 대사, 선택지, 감정 연출, 대화 흐름을 정리하여 `_workspace/03_dialogue_script.md` 에 기록한다.
5. 분기 구조, 플래그, 결말 조건, 엔딩 흐름을 정리하여 `_workspace/04_branch_map.md` 에 기록한다.
6. 전체 결과물을 검토하고 정합성, 품질, 개선사항을 `_workspace/05_review_report.md` 에 기록한다.

사용자가 일부 단계만 요청한 경우에는 해당 단계만 수행한다.

## 산출물 규칙

모든 산출물은 `_workspace/` 디렉토리에 저장한다.

- `00_input.md` — 사용자 입력 정리
- `01_worldbuilding.md` — 세계관 설정 문서
- `02_quest_design.md` — 퀘스트 설계 문서
- `03_dialogue_script.md` — 대사 스크립트
- `04_branch_map.md` — 분기 구조도
- `05_review_report.md` — 리뷰 보고서

## 품질 기준

### 세계관
- 배경 설정, 세력 관계, 역사, 기술 또는 마법 규칙이 서로 충돌하지 않아야 한다.
- 설정은 플레이 경험과 퀘스트 구조를 뒷받침해야 한다.

### 퀘스트
- 목표, 동기, 보상, 난이도 곡선이 자연스럽게 연결되어야 한다.
- 메인 퀘스트와 사이드 퀘스트의 역할이 구분되어야 한다.
- 보상은 플레이 진행도와 기대 심리에 맞아야 한다.

### 대사
- 각 캐릭터의 말투와 성격이 일관되어야 한다.
- 상황과 감정선에 맞는 대사여야 한다.
- 선택지는 플레이어의 의도를 분명히 구분할 수 있어야 한다.

### 분기
- 분기 조건은 추적 가능해야 한다.
- 플래그는 중복되거나 모순 없이 관리되어야 한다.
- 엔딩은 이전 선택과 행동의 결과로 설득력 있게 연결되어야 한다.

### 리뷰
- 리뷰 보고서에는 문제점, 리스크, 누락 요소, 개선 제안을 명확히 적는다.
- 필요하면 어떤 문서를 어떻게 수정해야 하는지도 함께 제안한다.

## 작업 시 주의사항

- 사용자가 명시하지 않은 설정을 임의로 과도하게 확정하지 않는다.
- 설정 공백이 있을 경우, 합리적인 가정을 사용하되 문서 안에 가정임을 명시한다.
- 한 문서의 수정이 다른 문서에 영향을 주면 관련 문서도 함께 점검한다.
- 산출물은 읽기 쉬운 실무 문서 형태를 유지한다.
- 문서 간 참조 관계가 있다면 파일명 기준으로 명확하게 연결한다.