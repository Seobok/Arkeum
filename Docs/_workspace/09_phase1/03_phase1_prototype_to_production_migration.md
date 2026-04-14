# Phase 1 Prototype to Production Migration

## 문서 목적
- 현재 `Prototype` 스크립트로 검증한 게임 규칙을 실제 구현 구조로 옮기기 위한 기준안을 정리한다.
- 단순한 파일명 변경이 아니라, 책임 분리와 의존성 방향을 기준으로 `Production` 구조를 제안한다.
- 이후 `Assets/Arkeum/Scripts/Production` 폴더를 만들 때 바로 참고할 수 있는 폴더 구조와 클래스 책임을 정의한다.

## 현재 상태 요약
- 프로토타입은 핵심 규칙 검증에 초점을 둔다.
- 현재 검증된 핵심 규칙:
  - 방향 입력 시 앞 칸에 적이 있으면 공격
  - 적이 없고 앞 칸에 상호작용 대상이 있으면 상호작용
  - 둘 다 없고 이동 가능하면 이동
  - 플레이어의 행동 후 적이 반응
  - 런 종료 후 일부 자원과 진행도는 영구 보존
- 현재 구조는 `PrototypeGameController` 중심으로 동작하며, 일부 규칙은 서비스 클래스로 분리되기 시작한 상태다.

## 마이그레이션 목표
- `Prototype` 코드를 그대로 비대하게 키우지 않고, 실제 구현용 구조로 옮긴다.
- 게임 규칙, 데이터, 표현을 분리한다.
- Unity 씬/프리팹/애니메이션이 붙어도 핵심 규칙은 순수 C# 계층에서 유지되도록 한다.
- 추후 전투, 상호작용, 영구 성장, 맵 구조가 커져도 시스템 단위로 교체 가능하도록 만든다.

## 권장 폴더 구조
```text
Assets/Arkeum/Scripts/
  Core/
    GameBootstrap.cs
    GameDirector.cs
    GameState.cs
    ServiceRegistry.cs

  Gameplay/
    Run/
      RunController.cs
      RunState.cs
      TurnSystem.cs
      RunResultBuilder.cs

    Combat/
      CombatSystem.cs
      DamageResolver.cs
      EnemyTurnSystem.cs
      TargetingService.cs

    Interaction/
      InteractionSystem.cs
      InteractionResolver.cs
      IInteractable.cs
      InteractableType.cs

    Map/
      MapService.cs
      MapGenerator.cs
      MapDefinition.cs
      TileOccupancyService.cs

    Actors/
      ActorEntity.cs
      ActorStats.cs
      ActorFactory.cs
      ActorRepository.cs
      BrainType.cs
      EnemyAiService.cs

    Progression/
      ProgressionService.cs
      UnlockService.cs
      QuestService.cs
      SaveProfile.cs

    Items/
      ItemService.cs
      InventoryState.cs
      ConsumableResolver.cs
      EquipmentResolver.cs

  Presentation/
    World/
      WorldPresenter.cs
      ActorView.cs
      TileView.cs
      WorldViewFactory.cs
      CameraController.cs

    UI/
      HudPresenter.cs
      HudView.cs
      ResultPanelPresenter.cs
      DialoguePresenter.cs

    Audio/
      AudioCuePlayer.cs

  Data/
    Definitions/
      ActorDefinition.cs
      ItemDefinition.cs
      UnlockDefinition.cs
      MapDefinitionAsset.cs

    Runtime/
      RuntimeActorData.cs
      RuntimeRunData.cs

  Infrastructure/
    Save/
      SaveService.cs
      SavePaths.cs

    Input/
      InputReader.cs
      InputActionRouter.cs

    Utilities/
      GridPosition.cs
      DirectionUtils.cs
      CollectionExtensions.cs
```

## 계층별 책임

### Core
- 게임 전체 흐름 조립
- 허브, 런, 결과 상태 전환
- 시스템 초기화와 연결

### Gameplay
- 실제 게임 규칙과 상태 변경 담당
- 전투, 이동, 상호작용, 적 턴, 영구 성장 처리
- Unity `GameObject` 생성 책임은 갖지 않음

### Presentation
- 뷰 생성, 애니메이션, HUD, 카메라, 이펙트 처리
- `Gameplay` 상태를 읽고 화면에 반영

### Data
- 적 정의, 아이템 정의, 해금 비용, 맵 설정 같은 정적 데이터 보관
- `ScriptableObject` 중심 구성 권장

### Infrastructure
- 저장, 입력, 경로, 공용 유틸리티 등 외부 연결 담당

## 의존 방향 원칙
```text
Core -> Gameplay
Core -> Presentation
Gameplay -> Data
Presentation -> Gameplay(read only or events)
Infrastructure -> Core/Gameplay 지원
```

추가 원칙:
- `Gameplay` 는 `Presentation` 을 직접 참조하지 않는다.
- `CombatSystem` 이 `GameObject` 를 생성하거나 `Transform` 을 직접 움직이지 않는다.
- 뷰는 상태 변경 결과를 반영만 하고, 규칙 판단은 하지 않는다.

## 현재 Prototype 파일과 Production 매핑

### 직접 매핑
- `PrototypeInputReader` -> `Infrastructure/Input/InputReader`
- `PrototypeCombatSystem` -> `Gameplay/Combat/CombatSystem`
- `PrototypeProgressionService` -> `Gameplay/Progression/ProgressionService`
- `PrototypeLayoutFactory` -> `Gameplay/Map/MapGenerator` 또는 `MapService`
- `PrototypeSaveService` -> `Infrastructure/Save/SaveService`

### 분리 매핑
- `PrototypeGameController` ->
  - `Core/GameDirector`
  - `Gameplay/Run/RunController`
  - `Presentation/World/WorldPresenter`
  - `Presentation/UI/HudPresenter`

- `PrototypeModels` ->
  - `Gameplay/Run/RunState`
  - `Gameplay/Actors/ActorEntity`
  - `Gameplay/Map/MapDefinition`
  - `Gameplay/Progression/SaveProfile`

- `PrototypeHud` ->
  - `Presentation/UI/HudPresenter`
  - `Presentation/UI/HudView`

## 핵심 클래스 설계안

### GameDirector
- 허브, 런, 결과 화면의 최상위 흐름 관리
- 각 시스템 초기화
- 씬 단위 진입과 종료 조립

### RunController
- 한 번의 런 진행 관리
- 플레이어 입력 요청을 게임 규칙으로 변환
- 턴 소비, 이동, 공격, 상호작용, 런 종료 판정 처리

### TurnSystem
- 플레이어 행동 1회를 턴으로 소비
- 적 반응 순서 제어
- 턴 카운트 증가와 관련 후처리 실행

### InteractionSystem
- 앞 칸 상호작용 판정
- 공격과 상호작용 우선순위 분기
- 상호작용 타입별 처리 위임

### CombatSystem
- 피해 계산
- 플레이어 공격, 적 공격 처리
- 사망과 보상 트리거 전달

### MapService
- 현재 맵 상태 보관
- 이동 가능 여부, 타일 속성, 깊이, 특수 지점 조회

### ActorRepository
- 현재 런에 존재하는 액터 목록 관리
- 위치 기반 탐색
- 플레이어, 적, NPC 검색 제공

### ProgressionService
- 런 종료 시 영구 자원 반영
- 해금, 퀘스트 완료, 누적 진행도 반영

### WorldPresenter
- 런타임 상태를 월드 뷰에 반영
- 액터 생성, 이동, 제거
- 타일 마커, 카메라 업데이트

### HudPresenter
- 현재 상태를 UI 친화적인 텍스트와 패널 상태로 변환
- 뷰 구현체와 규칙 계층 분리

## 핵심 규칙 고정안
Production 구조로 넘어가도 아래 규칙은 그대로 유지한다.

### 입력 처리 우선순위
`RunController.TryHandlePlayerAction(direction)`
1. 앞 칸에 적이 있으면 공격
2. 적이 없고 앞 칸에 상호작용 대상이 있으면 상호작용
3. 둘 다 없고 이동 가능하면 이동
4. 아니면 막힘 처리

### 턴 처리 규칙
1. 플레이어 행동 수행
2. 플레이어 상태 즉시 반영
3. 적 턴 실행
4. 사망, 클리어, 후처리 판정
5. HUD 및 월드 반영

## 데이터 분리 우선순위
우선적으로 코드에서 분리할 데이터:
- 적 스탯
- 아이템 효과량
- 해금 비용
- 맵 배치
- NPC 대사
- 런 보상 규칙

권장 방식:
- 정적 밸런스 데이터: `ScriptableObject`
- 세이브/런 상태: 직렬화 가능한 POCO 클래스
- 계산 로직: 서비스 클래스

## 실제 마이그레이션 순서

### 1단계. Production 폴더 생성
- `Assets/Arkeum/Scripts/Production` 또는 상기 제안 구조 생성
- 기존 `Prototype` 과 병행 유지

### 2단계. 상태 클래스 이동
- `RunState`
- `ActorEntity`
- `SaveProfile`
- `MapDefinition`

### 3단계. 핵심 서비스 이관
- `InputReader`
- `CombatSystem`
- `ProgressionService`
- `MapService`

### 4단계. 흐름 조립 분리
- `GameDirector`
- `RunController`
- `TurnSystem`
- `InteractionSystem`

### 5단계. 표현 계층 분리
- `WorldPresenter`
- `HudPresenter`
- `ActorView`, `TileView`, `CameraController`

### 6단계. Prototype 제거
- 새 구조가 기능적으로 동일한지 검증
- 이후 `Prototype` 접두사 제거

## 첫 이관 세트 권장안
처음부터 전체를 옮기지 말고 아래 6개를 우선 이관한다.
- `GameDirector`
- `RunController`
- `InputReader`
- `CombatSystem`
- `InteractionSystem`
- `ProgressionService`

이 조합이면 입력, 규칙, 영구 진행, 흐름 조립의 중심축이 먼저 Production 구조로 넘어간다.

## 주의사항
- 이름만 `Prototype` 에서 `Production` 으로 바꾸면 안 된다.
- `World` 생성, HUD 표시, 규칙 판단이 한 클래스에 다시 모이지 않도록 주의한다.
- Unity 씬과 프리팹 연결은 규칙 계층 분리가 끝난 뒤 붙이는 것이 안전하다.
- Production 구조의 성공 기준은 보기 좋은 이름이 아니라, 교체 가능성과 테스트 가능성이다.

## 다음 작업 제안
- `Assets/Arkeum/Scripts/Production` 기본 폴더 생성
- `GameDirector`, `RunController`, `CombatSystem`, `InteractionSystem` 뼈대 클래스 작성
- `Prototype` 에서 `Production` 으로 규칙 이관 체크리스트 작성
