# Phase 1 Design Gap Checklist

## 문서 목적
- `01_phase1_implementation_design.md`의 최초 설계안과 현재 `Assets/Arkeum` 구현 상태를 대조해 관리 가능한 체크리스트로 유지한다.
- 이미 구현된 항목, 설계와 달라진 항목, 아직 미구현인 항목을 분리한다.
- 이후 Phase 1 마무리 또는 Production 이관 시 우선순위를 판단하는 기준으로 사용한다.

## 상태 기준
- `[x]` 완료: 현재 구현이 최초 설계 의도를 충족한다.
- `[ ]` 미완료: 설계안에 있으나 현재 구현이 없거나 부족하다.
- `[ ]` 조정 필요: 구현은 있으나 최초 설계안과 방향이 다르므로 유지/수정 여부를 결정해야 한다.

## 1. 코어 루프
- [x] 거점에서 런을 시작할 수 있다.
  - 근거: `GameDirector.EnterHub`, `GameDirector.StartRun`
- [x] 런 종료 후 결과 화면을 거쳐 거점으로 복귀할 수 있다.
  - 근거: `GameDirector.ShowRunResult`, `GameDirector.UpdateRunResultInput`
- [x] `행동 1회 = 시간 1단위` 규칙이 적용된다.
  - 근거: `TurnSystem.ConsumePlayerAction`
- [x] 플레이어 행동 후 적 반응이 실행된다.
  - 근거: `RunController.ConsumeTurn`, `EnemyTurnSystem.ResolveEnemyTurn`
- [x] 사망 시 런 종료가 가능하다.
  - 근거: `RunController.ConsumeTurn`
- [x] 자발 귀환은 구현하지 않는다.
  - 최초 설계안의 확정 반영 사항과 일치한다.
- [x] 강제 종료 또는 클리어 지점 상호작용으로 런 종료가 가능하다.
  - 근거: `InteractionResolver.TryClaimReliquary`, `RunEndReason.DepthClear`

## 2. 이동과 턴 처리
- [x] 격자 기반 1칸 이동이 구현되어 있다.
  - 근거: `RunController.TryHandlePlayerAction`
- [x] 방향 입력으로 공격, 상호작용, 이동을 판정한다.
  - 근거: `RunController.TryHandlePlayerAction`
- [x] 대기 행동이 구현되어 있다.
  - 근거: `RunController.Wait`
- [x] 아이템 사용이 1행동으로 처리된다.
  - 근거: `RunController.UseBandage`, `RunController.UseDraught`
- [ ] 조정 필요: 입력 액션 맵 기반이 아니라 `Keyboard.current` 직접 입력 방식이다.
  - 현재 구현: `InputReader`
  - 결정 필요: Phase 1에서는 유지할지, Unity Input System 액션 맵으로 옮길지 결정한다.

## 3. 전투
- [x] 근접 공격 1종 중심 전투가 구현되어 있다.
  - 근거: `CombatSystem.ResolvePlayerAttack`, `CombatSystem.ResolveEnemyAttack`
- [x] 명중, 회피, 치명타 없는 확정 피해 기반이다.
  - 근거: `DamageResolver.ResolveDamage`
- [x] 피해 계산에 방어 수치가 반영된다.
  - 근거: `Mathf.Max(1, attackPower - defense)`
- [x] 적 처치 보상이 런 내 자원으로 반영된다.
  - 근거: `RunController.TryHandlePlayerAction`
- [ ] 조정 필요: 적 AI가 최초 설계보다 복잡하다.
  - 현재 구현: Behavior Tree, 준비 턴, 공격 패턴, 표적 타일 표시
  - 결정 필요: Phase 1 검증용으로 유지할지, 문서 기준의 단순 추적 AI로 축소할지 결정한다.

## 4. 적과 NPC
- [x] 적 추적과 공격이 구현되어 있다.
  - 근거: `EnemyTurnSystem`, `EnemyBehaviorActions`
- [x] 느리지만 강한 적 유형을 표현할 수 있다.
  - 근거: `ActorStats.ActionInterval`, 준비 턴 계열 필드
- [ ] 미완료: 상인/비전투 NPC가 ActorEntity 기반 캐릭터로 구현되어 있지 않다.
  - 현재 구현: `InteractableType.Merchant` 상호작용 지점
  - 필요 결정: 상인을 월드 액터로 승격할지, Phase 1에서는 상호작용 지점으로 유지할지 결정한다.
- [ ] 미완료: 설계안의 적 2~3종 데이터 에셋 구성이 확인되지 않는다.
  - 현재 확인 에셋: `MapAsset.asset`, `RunDefinition.asset`
  - 필요 작업: `EnemyDefinition` 에셋 생성 및 `RunDefinition` 연결 상태 점검

## 5. 자원, 아이템, 상인
- [x] 런 전용 자원 `혈편`에 해당하는 `BloodShards`가 구현되어 있다.
  - 근거: `RunState.BloodShards`
- [x] 영구 자원 `잔광`에 해당하는 `Gleam`이 구현되어 있다.
  - 근거: `SaveProfile.Gleam`
- [x] 런 종료 시 영구 자원 정산이 구현되어 있다.
  - 근거: `ProgressionService.ApplyRunEnd`
- [x] 던전 상인에게 런 내 자원으로 회복 소모품을 구매할 수 있다.
  - 근거: `InteractionResolver.TryBuyDraught`
- [x] 거점에서 영구 자원으로 시작 회복 아이템 해금을 할 수 있다.
  - 근거: `ProgressionService.TryUnlockStartingBandage`
- [ ] 조정 필요: 최초 설계 용어 `응고 지혈포`가 현재 구현에서는 `StartingBandage`로 되어 있다.
  - 결정 필요: 코드 명명까지 세계관 용어에 맞출지, UI/문서에서만 맞출지 결정한다.
- [ ] 미완료: 아이템 효과가 `ItemDefinition`/`EffectDefinition` 데이터로 분리되어 있지 않다.
  - 현재 구현: `RunController.UseBandage`, `RunController.UseDraught`에 직접 수치 포함
- [ ] 미완료: 무기 타입 데이터가 별도 `WeaponDefinition`으로 분리되어 있지 않다.
  - 현재 구현: 임시 무기 획득 시 `AttackBonus = 1`

## 6. 거점
- [x] 거점 상태와 거점 이동이 구현되어 있다.
  - 근거: `GameDirector.EnterHub`, `GameDirector.UpdateHubInput`
- [x] 거점에서 현재 보유 영구 자원, 회귀 횟수, 최고 깊이를 노출한다.
  - 근거: `HudPresenter.DrawTopBar`
- [x] 장례자 대사 변주가 구현되어 있다.
  - 근거: `ProgressionService.GetUndertakerGreeting`, `ProgressionService.SeedDialogue`
- [x] 해금 전/후 상태 문구가 구현되어 있다.
  - 근거: `GameDirector.UpdateHubLocationMessage`, `HudPresenter.DrawTopBar`
- [ ] 조정 필요: 설계안은 메뉴 허브에 가까운 거점을 상정했지만 현재는 격자 이동 거점이다.
  - 결정 필요: 현재 방식이 조작 학습에 유리하므로 유지할지, 메뉴 허브로 단순화할지 결정한다.

## 7. 저장과 진행도
- [x] `ProfileSave`에 해당하는 `SaveProfile` 데이터 타입이 있다.
  - 근거: `SaveProfile`
- [x] 총 회귀 횟수, 최고 도달 깊이, 잔광, 시작 해금 여부, 퀘스트/플래그 필드가 있다.
  - 근거: `SaveProfile.TotalReturns`, `HighestDepth`, `Gleam`, `StartingBandageUnlocked`, `UnlockedFlags`, `CompletedQuestIds`
- [ ] 미완료: JSON 파일 저장/로드가 구현되어 있지 않다.
  - 현재 구현: `GameBootstrap`에서 매 실행마다 `new SaveProfile()` 생성
  - 필요 작업: `SaveService`, `SavePaths`, JSON 직렬화, 로드 실패 대응 추가
- [ ] 미완료: `RunSave` 또는 `RunSnapshot` 타입이 별도 저장 모델로 구현되어 있지 않다.
  - 현재 구현: `RunState` 런타임 상태만 존재
- [ ] 미완료: 거점 영구 저장이 실제 디스크에 반영되지 않는다.
  - 필요 작업: 런 종료, 해금 구매 시점에 저장 호출
- [ ] 미완료: `MQ-01`은 완료 마킹만 있고 문구 노출/저장 정책이 명확하지 않다.
  - 근거: `QuestService.MarkPrototypeClear`

## 8. UI/UX
- [x] HUD 중심 UI가 구현되어 있다.
  - 근거: `HudPresenter.OnGUI`
- [x] HP, 런 내 자원, 아이템 수량, 턴 수가 표시된다.
  - 근거: `HudPresenter.DrawTopBar`
- [x] 영구 자원은 거점 상태에서 표시된다.
  - 근거: `HudPresenter.DrawTopBar`
- [x] 행동 후 적 반응 안내 문구가 있다.
  - 근거: `Rule: every action gives enemies a response.`
- [x] 런 결과창에서 잃은 것과 남는 것을 분리해 보여준다.
  - 근거: `HudPresenter.DrawRunResult`
- [ ] 조정 필요: 최초 설계 권장인 `uGUI`가 아니라 `OnGUI` 기반이다.
  - 결정 필요: Phase 1 검증 중에는 유지하고, Production UI 이관 시 교체할지 결정한다.
- [ ] 미완료: 최초 설계의 한국어 목표 문구가 UI에 반영되어 있지 않다.
  - 설계 문구: `잿빛 회랑으로 내려가 네 이름에 반응한 잔광을 회수하라.`
- [ ] 미완료: 세계관 용어가 UI 전반에서 영어 임시 문구로 표시된다.
  - 필요 작업: `Gleam`, `Blood shards`, `Bandage`, `Draught` 등의 표기 정책 결정

## 9. 씬, 프리팹, 에셋 구조
- [ ] 조정 필요: 최초 설계의 `Bootstrap`, `Hub`, `Dungeon_TestA` 씬 구성이 아니라 `SampleScene` 하나만 존재한다.
  - 현재 확인: `Assets/Arkeum/Scenes/SampleScene.unity`
  - 결정 필요: 단일 씬 상태 전환을 유지할지, 설계안대로 씬을 분리할지 결정한다.
- [ ] 조정 필요: 최초 설계는 수제 테스트 구역 1~2개였지만 현재는 절차 생성 성격의 던전 생성기가 있다.
  - 근거: `MapGenerator.CreateDungeonMap`
  - 결정 필요: Phase 1에서 절차 생성 유지 여부 결정
- [x] `MapAsset`과 `RunDefinition` ScriptableObject 구조가 있다.
  - 근거: `MapAsset`, `RunDefinition`
- [ ] 미완료: `Actors`, `Items`, `Weapons`, `Encounters`, `Progression` 하위 ScriptableObject 에셋 구성이 아직 부족하다.
  - 현재 확인: `MapAsset.asset`, `RunDefinition.asset`
- [ ] 미완료: 플레이어, 적, UI, 인터랙티브 프리팹 구성이 확인되지 않는다.
  - 필요 작업: 프리팹 기반 표현 계층으로 옮길 범위 결정

## 10. 설계 대비 초과 구현 또는 방향 차이
- [ ] 조정 필요: `Prototype` 단계를 넘어 Production 계층 분리가 상당 부분 진행되어 있다.
  - 현재 구조: `Core`, `Gameplay`, `Presentation`, `Infrastructure`
  - 결정 필요: `03_phase1_prototype_to_production_migration.md`를 현재 코드 기준으로 갱신한다.
- [ ] 조정 필요: Behavior Tree와 공격 패턴이 Phase 1 최초 설계보다 앞서 구현되어 있다.
  - 결정 필요: 테스트 가능한 복잡도로 유지할지, 문서의 Phase 1 범위를 갱신할지 결정한다.
- [ ] 조정 필요: 절차 생성 맵이 Phase 1 확정안의 `수제 테스트 구역`과 충돌한다.
  - 결정 필요: 최초 설계를 수정할지, 구현을 수제 맵 기반으로 되돌릴지 결정한다.

## 11. 우선순위 제안

### P0: Phase 1 마무리에 필요한 항목
- [ ] JSON 기반 `SaveService` 구현
- [ ] 런 종료와 해금 구매 시점에 프로필 저장 호출
- [ ] 실행 시작 시 저장된 프로필 로드
- [ ] 현재 단일 씬/절차 생성/OnGUI를 Phase 1에서 유지할지 문서로 확정

### P1: 데이터 분리와 제작 안정성
- [ ] `EnemyDefinition` 에셋 생성 및 런 스폰 연결 상태 점검
- [ ] 회복 아이템을 `ItemDefinition` 계열 데이터로 분리
- [ ] 임시 무기와 기본 무기를 `WeaponDefinition` 계열 데이터로 분리
- [ ] 해금 비용과 보상 규칙을 코드 상수에서 데이터로 이동

### P2: 표현과 콘텐츠 정리
- [ ] UI 표기 언어와 세계관 용어 통일
- [ ] 최초 목표 문구를 HUD 또는 거점 안내에 반영
- [ ] 상인을 월드 액터로 구현할지 결정
- [ ] 프리팹 기반 Actor/Tile/UI 표현으로 이관할 범위 결정

## 12. 다음 점검 질문
- [ ] Phase 1은 현재 구현처럼 `단일 씬 + 상태 전환`으로 확정할 것인가?
- [ ] Phase 1에서 절차 생성 맵을 유지할 것인가, 수제 테스트 맵으로 되돌릴 것인가?
- [ ] `OnGUI` HUD를 Phase 1 완료 조건으로 인정할 것인가?
- [ ] `응고 지혈포`, `혈편`, `잔광` 등 한국어 세계관 용어를 코드 명명에도 반영할 것인가?
- [ ] 저장 시스템을 Phase 1 완료의 필수 조건으로 둘 것인가?
