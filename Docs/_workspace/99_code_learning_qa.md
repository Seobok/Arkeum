# 99. Code Learning Q&A

이 문서는 코드 이해를 위한 질문과 답변을 누적 정리한다.

## Q1. `InputReader`를 별개로 제작해서 사용하는 이유는 뭐야? 유니티 기존의 New Input System 같은 것을 사용하지 않고?

짧게 말하면, 이 프로젝트는 입력 처리를 "유니티 기능에 깊게 묶지 않고", 게임 규칙 쪽에서 필요한 최소 형태만 얇게 감싸서 쓰려는 의도가 보인다.

다만 먼저 정정할 점이 있다. 이 코드는 New Input System을 아예 안 쓰는 것이 아니다. `InputReader` 자체가 이미 `UnityEngine.InputSystem`을 사용하고 있고, `Keyboard.current`와 각 키의 `wasPressedThisFrame`를 직접 읽는 방식이다.

근거:
- `InputReader`가 `using UnityEngine.InputSystem;`를 사용한다. [InputReader.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Infrastructure/Input/InputReader.cs:2)
- 실제 입력도 `Keyboard`를 받아서 처리한다. [InputReader.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Infrastructure/Input/InputReader.cs:11)
- `GameDirector`도 `Keyboard.current`를 읽은 뒤 방향 해석만 `Services.InputReader`에 맡긴다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:140), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:146), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:173), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:180)

즉, 정확히는 "New Input System의 액션 에셋/`PlayerInput` 중심 구조를 채택하지 않고, 저수준 키보드 API를 얇은 래퍼로 감싼 상태"라고 보는 게 맞다.

### 왜 굳이 `InputReader`로 분리했나

1. 입력 해석 규칙을 한 곳에 모으기 위해서다.

현재 방향 입력 규칙은 `WASD`, 방향키, 그리고 "이번 프레임에 눌렸는가"라는 정책까지 포함한다. 이걸 `GameDirector` 안에 직접 쓰면 허브 입력, 런 입력, 결과 화면 입력마다 같은 규칙이 퍼지기 쉽다. 지금 구조는 "키를 어떻게 방향으로 바꾸는가"를 `InputReader` 한 곳에 모아둔다. [InputReader.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Infrastructure/Input/InputReader.cs:19)

2. `Core`의 흐름 제어와 입력 장치 세부사항을 분리하려는 구조다.

`GameDirector`의 역할은 상태별로 무엇을 할지 결정하는 것이다. 실제로 클래스는 허브/런/결과 상태 전환과 게임 진행을 관리하고, 방향 판독은 `Services.InputReader`에 위임한다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:37), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:138), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:171)

3. 프로토타입에서 프로덕션 구조로 옮기며 책임을 분리하려는 흐름과 맞는다.

마이그레이션 문서에서 `PrototypeInputReader`를 `Infrastructure/Input/InputReader`로 옮긴다고 명시되어 있다. 즉 입력은 게임 규칙 본체가 아니라 인프라 계층으로 취급하겠다는 방향이다. [03_phase1_prototype_to_production_migration.md](/D:/Unity/Arkeum/Docs/_workspace/phase1/03_phase1_prototype_to_production_migration.md:161)

4. 나중에 입력 구현을 바꾸기 쉽게 만들려는 의도다.

`InputReader` 내부 주석에 "추후 new Input System으로 교체 하는 방향 고려"라고 적혀 있다. 표현은 조금 애매하지만, 지금 구현은 임시/간소 버전이고 나중에 더 본격적인 액션 기반 입력으로 바꿀 수 있게 경계면을 세운 상태로 읽힌다. [InputReader.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Infrastructure/Input/InputReader.cs:8)

5. 지금 프로젝트 규모에 비해 더 단순한 구현이 충분했기 때문이다.

현재 `InputReader`가 처리하는 것은 사실상 4방향 이동뿐이다. 여기에 `Q`, `1`, `2`, `Enter` 같은 단발 키는 아직 `GameDirector`에서 직접 읽고 있다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:184), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:189), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:193), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:215)

즉 현재 단계에서는:
- 입력 종류가 적고
- 키 리바인딩도 없고
- 패드/복수 디바이스 대응도 없고
- 액션 맵/이벤트 기반 입력도 아직 필요하지 않다

그래서 `InputActionAsset`, `PlayerInput`, 콜백 바인딩까지 전부 도입하는 것보다, 얇은 `InputReader` 하나로 끝내는 편이 더 싸고 빠르다.

### 정리

`InputReader`를 따로 만든 이유는 "유니티 입력 시스템을 안 써서"가 아니라, "유니티의 입력 API 사용을 게임 로직에서 분리하고, 현재 필요한 최소 입력만 단순하게 관리하려고"라고 보는 게 맞다.

현재 구조의 장점:
- 입력 규칙이 한 곳에 모인다.
- `GameDirector`가 입력 장치 세부 구현에 덜 묶인다.
- 나중에 액션 기반 New Input System으로 바꾸기 쉬워진다.
- 프로토타입 단계에서는 구현 비용이 낮다.

현재 구조의 한계:
- `Q`, `1`, `2`, `Enter`는 아직 `InputReader` 밖에 있어서 입력 추상화가 완전하지 않다.
- `Keyboard.current`에 직접 의존하므로 패드/리바인딩/멀티 디바이스 확장에는 약하다.
- 테스트 가능한 인터페이스 수준까지 추상화된 것은 아니다.

그래서 이 코드는 "New Input System 미사용"이라기보다, "New Input System의 저수준 API만 쓰고 있고, 액션 기반 구조는 아직 도입하지 않은 중간 단계"라고 이해하면 가장 정확하다.

## Q2. 그럼 해당 질문에 이어서 `Q`, `1`, `2` 등 다른 키들은 다른 곳에서 입력 판정을 확인하는 것 같은데 `InputReader`로 통합하면 어떤 단점이 발생해?

짧게 말하면, 통합 자체가 문제는 아니다. 다만 "어떤 수준까지" 통합하느냐에 따라 `InputReader`가 단순 입력 해석기에서 게임 상태를 아는 로직 객체로 비대해질 수 있다는 게 단점이다.

현재 구조를 보면:
- 방향 입력은 `InputReader`가 맡는다. [InputReader.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Infrastructure/Input/InputReader.cs:11)
- 하지만 `Q`, `1`, `2`는 `GameDirector.UpdateRunInput()` 안에서 직접 읽는다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:171), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:184), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:189), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:193)
- `Enter`는 또 결과 화면 전용으로 `UpdateRunResultInput()`에서 직접 읽는다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:212), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:215)

즉 지금은 `InputReader`가 "방향 해석만 담당하는 아주 작은 객체"이고, 나머지는 상태별 입력 처리로 `GameDirector`에 남아 있다.

### `InputReader`로 통합할 때 생길 수 있는 단점

1. `InputReader`가 게임 상태를 알기 시작할 수 있다.

예를 들어 `Q`는 런 상태에서는 대기이지만, 허브에서는 아무 의미가 없고, 결과 화면에서도 의미가 없다. `Enter`도 결과 화면에서만 유효하다. 만약 `InputReader`가 이런 것까지 한 번에 판단하려면 결국 "지금 허브인지, 런인지, 결과 화면인지"를 알아야 한다.

그 순간 `InputReader`는 단순히 키를 읽는 인프라 객체가 아니라, 게임 상태 규칙을 일부 아는 객체가 된다. 그건 현재 계층 분리 의도와 어긋난다. `GameDirector`는 상태를 관리하는 쪽이고, `InputReader`는 인프라로 분리되어 있기 때문이다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:37), [03_phase1_prototype_to_production_migration.md](/D:/Unity/Arkeum/Docs/_workspace/phase1/03_phase1_prototype_to_production_migration.md:161)

2. 입력 해석과 입력 의미가 섞일 수 있다.

`W`를 눌렀다는 사실과 "플레이어가 위로 이동한다"는 것은 비교적 가까운 개념이다. 그래서 `TryGetMoveDirection()`은 자연스럽다. [InputReader.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Infrastructure/Input/InputReader.cs:11)

반면 `1`은 단순히 숫자 키일 뿐인데, 현재 맥락에서는 "붕대 사용"이라는 게임 규칙 의미를 가진다. `Q`는 "대기", `Enter`는 "결과 화면 종료"라는 뜻이다. 이것까지 `InputReader`가 다 반환하기 시작하면, 입력 계층이 게임 액션 의미를 직접 품게 된다.

즉 아래 두 단계가 섞일 위험이 있다.
- 입력 해석: 어떤 키가 눌렸는가
- 게임 의미 부여: 그 키가 현재 상태에서 무엇을 뜻하는가

3. 상태별 분기가 `InputReader` 내부로 이동해 더 커질 수 있다.

지금은 `GameDirector`가 상태별로 `UpdateHubInput()`, `UpdateRunInput()`, `UpdateRunResultInput()`로 나눠서 읽고 있다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:138), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:171), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:212)

이걸 무리하게 `InputReader` 하나로 합치면 내부가 이런 식으로 가기 쉽다.
- 허브면 방향만 허용
- 런이면 방향/Q/1/2 허용
- 결과면 Enter 허용

그러면 `InputReader`는 점점 거대한 `if/switch` 덩어리가 되고, 오히려 읽기 어려워질 수 있다.

4. 재사용성이 떨어질 수 있다.

현재의 `InputReader`는 "키보드 방향 입력을 읽는 도구"라서 비교적 범용적이다. 프로토타입에서도 거의 같은 형태였다. `PrototypeInputReader`도 방향만 읽는다. [PrototypeInputReader.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Prototype/PrototypeInputReader.cs:8)

그런데 여기에 붕대, 물약, 결과 화면 확인 같은 게임 전용 명령이 계속 들어가면, 이 클래스는 더 이상 범용 입력 리더가 아니라 "Arkeum 전용 플레이 규칙 입력기"가 된다. 그러면 다른 화면이나 다른 모드에서 재사용하기 어려워진다.

5. 오히려 책임 경계가 흐려질 수 있다.

지금 코드는 `GameBootstrap`에서 `InputReader`를 서비스로 생성하고, [GameBootstrap.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameBootstrap.cs:60) `GameDirector`가 그 서비스를 사용한다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:146)

이 구조에서 `InputReader`가 너무 많은 명령 의미를 품으면:
- `GameDirector`는 입력 정책을 덜 보게 되고
- `InputReader`는 상태/액션 정책을 더 많이 가지게 된다

결국 "누가 입력 흐름을 책임지는가"가 흐릿해질 수 있다.

### 그렇다고 통합이 항상 나쁜 건 아니다

중요한 건 `InputReader`로 통합하더라도 "게임 상태 판단"까지 넣지 않으면 오히려 더 좋아질 수 있다는 점이다.

예를 들어 이런 방향은 괜찮다.
- `TryGetMoveDirection()`
- `IsWaitPressed()`
- `IsBandagePressed()`
- `IsDraughtPressed()`
- `IsConfirmPressed()`

이렇게 만들면 `InputReader`는 여전히 "키 입력을 읽는 역할"만 한다. 그리고 그 입력을 현재 상태에서 어떻게 해석할지는 `GameDirector`가 결정하면 된다.

이 방식의 장점:
- 키 판정 코드가 한 곳에 모인다.
- 리바인딩이나 입력 장치 교체 시 수정 지점이 줄어든다.
- 상태 판단은 계속 `GameDirector`에 남으므로 책임 분리가 유지된다.

즉 단점은 "`InputReader`로 통합한다" 자체에 있는 게 아니라, "`InputReader`가 현재 게임 상태에 따른 액션 의미까지 판단하게 만들 때" 발생한다.

### 정리

`Q`, `1`, `2`, `Enter`를 `InputReader`로 옮기는 것은 가능하다. 하지만 아래 두 방식은 결과가 다르다.

좋은 통합:
- `InputReader`는 "어떤 입력이 발생했는가"만 알려준다.
- `GameDirector`는 "그 입력을 현재 상태에서 어떻게 처리할 것인가"를 결정한다.

나쁜 통합:
- `InputReader`가 허브/런/결과 상태를 알고
- `Q`는 대기, `1`은 붕대, `Enter`는 복귀 같은 게임 의미까지 직접 결정한다.

그래서 현재 코드 기준으로 보면, 가장 현실적인 개선은 "`Q`, `1`, `2`, `Enter`의 키 판정은 `InputReader`로 모으되, 상태별 해석은 `GameDirector`에 남기는 것"이다.

## Q3. `RunState` 클래스에 있는 변수는 각각 어떤 의미야?

`RunState`는 "현재 한 판의 런(run)에 대한 진행 상태 묶음"이다. 저장용 프로필인 `SaveProfile`이 영구 진행도를 들고 있다면, `RunState`는 이번 판에서만 유효한 임시 상태를 들고 있다고 보면 된다. 선언은 [RunState.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunState.cs:7)에 있다.

생성 시 초기값은 `RunController.CreateRunState()`에서 세팅된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:48)

### 필드별 의미

`RunIndex`
- 몇 번째 런인지 나타내는 번호다.
- `profile.TotalReturns + 1`로 시작하므로, 누적 귀환 횟수를 기준으로 이번 런의 순번을 매긴다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:53)

`TurnCount`
- 이번 런에서 플레이어가 몇 번 행동했는지 나타내는 턴 수다.
- 플레이어 행동이 소비될 때 `TurnSystem`이 증가시킨다.
- 적 행동 주기 계산에도 사용된다. `ActionInterval`이 2인 적은 `TurnCount % 2 == 0`일 때만 움직인다. [TurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/TurnSystem.cs:12), [EnemyTurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/EnemyTurnSystem.cs:35)

`DepthReached`
- 이번 런에서 플레이어가 도달한 가장 깊은 층/구역 깊이다.
- 시작은 1이고, 이동할 때 현재 칸의 깊이와 비교해서 더 깊은 값으로 갱신된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:55), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:104)
- 런 종료 후 영구 기록 갱신과 보상 계산에도 쓰인다. [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:25), [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:31)

`CurrentHp`
- 런 기준 플레이어의 현재 체력이다.
- 실제 `ActorEntity Player.CurrentHp`와 거의 같은 정보를 들고 있지만, HUD와 결과 계산이 `RunState`만 보고도 되게 하려고 같이 들고 있는 구조다.
- 시작 시 세팅되고, 붕대/드로트 사용 후 갱신되며, 적 턴 이후에도 다시 동기화된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:56), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:131), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:158), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:251)

`MaxHp`
- 런 기준 최대 체력이다.
- 현재는 기본값 12로 시작하고, `Begin()`에서 실제 플레이어 스탯과 다시 맞춘다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:57), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:44)
- HUD에 `CurrentHp/MaxHp` 형태로 출력된다.

`BloodShards`
- 이번 런에서 모은 소모성 자원이다.
- 적 처치 시 증가하고, 상점에서 드로트를 살 때 3개 소모된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:84), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:239)
- 런 종료 시 잃는 자원으로 결과 화면에 표시된다. [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:80)

`BandageCount`
- 이번 런에서 남아 있는 붕대 개수다.
- 시작 붕대 해금이 되어 있으면 1개로 시작하고, 아니면 0개로 시작한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:50), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:59)
- 사용 시 1 감소하고 체력을 4 회복한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:118), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:130)

`DraughtCount`
- 이번 런에서 들고 있는 드로트 개수다.
- 시작은 0개이고, 상점에서 구매하면 증가하며, 사용하면 감소한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:60), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:240), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:157)
- 런 종료 시 남은 드로트는 잃는 것으로 처리된다. [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:81)

`AttackBonus`
- 이번 런에서 추가로 얻은 공격력 보너스다.
- 현재는 임시 무기를 주웠을 때 `+1`이 된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:270)
- 기본 공격력과 합쳐서 실제 공격력 계산에 사용된다.

`GleamReward`
- 런 종료 후 영구 자원으로 환산되어 얻는 `gleam` 보상량이다.
- 런 도중에는 의미가 없고, 종료 처리 시 `ProgressionService.ApplyRunEnd()`에서 계산된다. [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:30), [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:36)

`TemporaryWeaponEquipped`
- 이번 런에서 임시 무기를 현재 장착 중인지 나타낸다.
- 임시 무기를 주우면 `true`가 되고, 결과 화면에서 "잃는 것"으로 표현된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:269), [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:82)

`ReliquaryClaimed`
- 런의 목표 오브젝트인 reliquary를 이미 회수했는지 나타낸다.
- 이 값이 `false`일 때만 reliquary 상호작용이 유효하고, 회수하면 `DepthClear`로 런이 종료된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:63), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:195), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:199)

`TemporaryWeaponCollected`
- 임시 무기 스폰 지점을 이미 한 번 먹었는지 나타내는 플래그다.
- 이 값 덕분에 같은 위치에서 반복해서 무기를 다시 먹는 일이 막힌다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:261), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:268)

`DraughtStock`
- 상점이 이번 런에서 추가로 팔 수 있는 드로트 재고 수량이다.
- 시작은 2이고, 구매할 때마다 1씩 줄어든다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:65), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:233), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:241)

`EndReason`
- 런이 왜 끝났는지 나타내는 종료 사유다.
- 시작은 `None`, 죽으면 `Death`, 목표 회수에 성공하면 `DepthClear`가 들어간다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:66), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:183), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:254)
- 보상량 계산에도 직접 영향을 준다. [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:30)

`Player`
- 이번 런에서 조작하는 실제 플레이어 액터 참조다.
- `RunState`가 단순 숫자 묶음만 가진 것이 아니라, 실제 월드 상의 플레이어 엔티티와 연결되어 있다는 뜻이다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:99), [RunState.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunState.cs:24)
- 적 AI는 이 참조를 이용해 플레이어 위치를 찾고 공격한다. [EnemyTurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/EnemyTurnSystem.cs:40), [EnemyTurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/EnemyTurnSystem.cs:52)

`EffectiveAttack`
- 저장 필드가 아니라 계산 프로퍼티다.
- 현재 공식은 `기본 공격력 3 + AttackBonus`다. [RunState.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunState.cs:26)
- 임시 무기를 주운 뒤 실제 플레이어 공격력에도 이 값을 반영한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:271)

### 한 줄로 분류하면

진행도/메타 정보:
- `RunIndex`
- `TurnCount`
- `DepthReached`
- `EndReason`

전투/생존 상태:
- `CurrentHp`
- `MaxHp`
- `AttackBonus`
- `EffectiveAttack`
- `Player`

런 내 소모 자원:
- `BloodShards`
- `BandageCount`
- `DraughtCount`
- `DraughtStock`

런 내 이벤트/획득 플래그:
- `TemporaryWeaponEquipped`
- `TemporaryWeaponCollected`
- `ReliquaryClaimed`

런 종료 보상:
- `GleamReward`

### 구조적으로 보면

`RunState`는 완전히 순수한 데이터 객체라기보다, 현재는 약간 타협된 형태다.
- 숫자 상태도 들고 있고
- `Player`라는 실제 액터 참조도 들고 있고
- `EffectiveAttack` 같은 계산값도 포함한다

즉 "런 전체를 조작하기 쉽게 한곳에 모아둔 상태 컨테이너"에 가깝다.

이 구조의 장점:
- HUD, 전투, 진행도 계산이 같은 객체를 공유하기 쉽다.
- 한 판의 상태를 한곳에서 보기가 쉽다.

이 구조의 주의점:
- `CurrentHp`와 `Player.CurrentHp`처럼 중복 데이터가 생긴다.
- 그래서 지금 코드도 행동 후마다 둘을 계속 동기화하고 있다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:132), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:159), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:251)

## Q4. `TurnSystem` 클래스가 별개로 필요한 이유가 뭐야?

현재 코드만 놓고 보면 `TurnSystem`은 매우 작다. 실제로 하는 일은 `RunState.TurnCount`를 1 올리는 것뿐이다. [TurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/TurnSystem.cs:5), [TurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/TurnSystem.cs:12)

그래서 지금 시점의 솔직한 답은 이렇다.
- "당장 꼭 필요해서"라기보다
- 턴 소비 규칙을 별도 책임으로 빼 두기 위해 미리 분리한 구조다

### 왜 굳이 지금부터 분리했나

1. 턴 소비를 하나의 규칙 포인트로 모으기 위해서다.

현재 플레이어가 행동하면 `RunController.ConsumeTurn()`에서 먼저 `turnSystem.ConsumePlayerAction(CurrentRun)`을 호출하고, 그 다음 적 턴을 처리한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:247), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:249), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:250)

즉 "플레이어 행동 1회가 턴 카운트를 어떻게 소모시키는가"를 `RunController` 안의 임의 코드로 두지 않고, 한 메서드 호출로 모아 둔 것이다.

2. 적 행동 규칙이 턴 수에 의존하기 때문이다.

`EnemyTurnSystem`은 적의 `ActionInterval`에 따라 `runState.TurnCount`를 보고 행동 여부를 결정한다. [EnemyTurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/EnemyTurnSystem.cs:35)

즉 턴 카운트는 단순 UI 숫자가 아니라, 적 AI 동작 타이밍을 결정하는 핵심 규칙 값이다. 이런 값은 보통 아무 데서나 `runState.TurnCount += 1` 하게 두기보다, 한 경로로 증가시키는 편이 안전하다.

3. 앞으로 턴 규칙이 커질 가능성이 높다.

지금은 증가만 하지만, 턴 시스템이 커지면 보통 여기에 이런 것들이 붙는다.
- 행동 속도 차이
- 버프/디버프 지속 턴 감소
- 독/화상 같은 지속 피해
- 적별 행동 예약
- 한 행동이 턴을 소비하지 않는 예외 규칙
- 특정 행동은 2턴 소비 같은 규칙

이런 가능성을 보면 `TurnSystem`을 미리 분리한 것은 "아직은 빈 껍데기에 가깝지만 확장 지점을 먼저 만든 것"이라고 볼 수 있다.

4. `RunController`의 역할을 과하게 키우지 않기 위해서다.

이미 `RunController`는:
- 이동 처리
- 공격 처리
- 상호작용 처리
- 아이템 사용
- 런 종료 판정
- 자동 획득 처리

를 맡고 있다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:70), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:111), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:138), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:176)

여기에 턴 진행 세부 규칙까지 다 넣으면, 런 전체 오케스트레이션과 턴 규칙이 한 클래스에 섞인다. 지금은 단순해 보여도, 구조적으로는 분리해 두는 편이 맞다.

5. 마이그레이션 설계 의도와 맞는다.

프로덕션 구조 문서에서도 `TurnSystem`은 `RunController`와 별도 책임으로 분리된 구성요소로 적혀 있다. 즉 처음부터 "턴 규칙은 별도 시스템"으로 보겠다는 설계다. [03_phase1_prototype_to_production_migration.md](/D:/Unity/Arkeum/Docs/_workspace/phase1/03_phase1_prototype_to_production_migration.md:282), [03_phase1_prototype_to_production_migration.md](/D:/Unity/Arkeum/Docs/_workspace/phase1/03_phase1_prototype_to_production_migration.md:285)

### 그럼 현재 시점에서는 과한 분리 아닌가

맞다. 현재 구현만 보면 약간 과분리처럼 보일 수 있다.

이유:
- 클래스가 너무 작다
- 메서드가 사실상 `TurnCount += 1` 한 줄이다
- 아직 `TurnSystem`만의 독립 규칙이나 상태가 거의 없다

그래서 "지금 당장 실용성"만 보면 `RunController` 내부 private 메서드여도 돌아간다.

하지만 이 코드베이스는 프로토타입에서 프로덕션 구조로 옮기면서 책임을 나눠 두는 방향을 택했다. 그 관점에서는 `TurnSystem`이 지금 작더라도 이상한 건 아니다. 미리 경계선을 세운 셈이다.

### 가장 정확한 해석

현재의 `TurnSystem`은 "완성된 시스템"이라기보다 "턴 관련 규칙이 앞으로 들어올 자리"에 가깝다.

즉 지금 필요한 이유는 두 가지다.
- 턴 카운트 증가를 한 곳으로 모아 규칙 포인트를 만들기 위해
- 이후 턴 진행 규칙이 커질 때 `RunController`를 비대하게 만들지 않기 위해

### 정리

현재 코드 기준으로 보면:
- 기능적으로는 없어도 된다
- 구조적으로는 있는 편이 낫다

즉 `TurnSystem`은 지금 당장 복잡한 일을 해서 필요한 클래스라기보다, "턴 소비 규칙을 독립된 책임으로 관리하려는 설계 의도" 때문에 분리되어 있다고 이해하면 맞다.

## Q5. `CombatSystem`에 있는 `ResolvePlayerAttack`에는 왜 `RunState`가 매개변수로 들어가는지, 또 `PlayerAttack`과 `EnemyAttack`을 구분해둔 이유는 무엇인지

코드 기준으로 먼저 결론부터 말하면:
- `ResolvePlayerAttack(RunState runState, ...)`의 `runState`는 현재 실제로 사용되지 않는다.
- `ResolvePlayerAttack`과 `ResolveEnemyAttack`도 현재 동작은 사실상 같다.

즉 둘 다 "지금 꼭 필요한 구조"라기보다, 향후 규칙 분리를 염두에 둔 흔적이 남아 있는 상태로 보는 게 가장 정확하다.

근거는 `CombatSystem` 자체에 TODO가 직접 적혀 있다는 점이다. [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:15), [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:23)

### 1. 왜 `ResolvePlayerAttack`에 `RunState`가 들어가나

현재 구현에서는 안 쓴다.

실제 메서드 내용을 보면:
- 공격력 계산
- 피해 적용
- 피해량 반환

만 하고, `runState`를 참조하는 코드는 없다. [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:16), [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:18), [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:19)

즉 현재 기준으로는:
- 불필요한 매개변수이거나
- 앞으로 쓸 계획이 있었는데 아직 구현되지 않은 상태다

#### 그럼 원래 어떤 용도로 들어갈 가능성이 있었나

플레이어 공격은 적 공격보다 런 전체 상태에 더 많이 연결되기 쉽다. 예를 들면:
- 이번 런에서 얻은 임시 무기 효과
- 특정 층(`DepthReached`)에서만 발동하는 보너스
- 런 내 버프/디버프
- 적 처치 시 자원 획득, 콤보, 퀘스트 카운트
- 플레이어 전용 치명타, 출혈, 추가타 같은 효과

이런 규칙은 단순히 `attacker`와 `defender`만으로는 부족하고, "이번 런 전체 상태"를 봐야 할 수 있다. 그래서 `RunState`를 미리 넣어 둔 것으로 해석할 수 있다.

다만 중요한 건, 현재는 그런 로직이 없다. 예를 들어 공격력 보정도 지금은 `attacker.Stats.AttackPower`에 이미 반영된 값을 그냥 읽는다. [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:18), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:271)

그래서 지금 시점의 엄밀한 답은:
- `RunState`는 현재 필요하지 않다
- 미래 확장용으로 남아 있는 흔적이다

### 2. 왜 `PlayerAttack`과 `EnemyAttack`을 구분해뒀나

현재는 둘이 똑같다.

두 메서드 모두:
- `damageResolver.ResolveDamage(attacker.Stats.AttackPower, defender.Stats.Defense)` 호출
- `ApplyDamage(defender, damage)` 호출
- `damage` 반환

으로 완전히 같은 흐름이다. [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:16), [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:24)

그러니 기능적으로는 지금 하나의 메서드로 합쳐도 된다.

#### 그런데 왜 굳이 둘로 나눴을까

이유는 플레이어 공격과 적 공격이 "앞으로 달라질 가능성"이 높기 때문이다.

플레이어 공격에만 붙기 쉬운 것:
- 무기 효과
- 아이템/버프 보너스
- 처치 보상
- 특수 공격 연출
- 플레이어 전용 상태이상 부여

적 공격에만 붙기 쉬운 것:
- 적 종류별 특수 패턴
- 플레이어에게만 적용되는 디버프
- 방어 무시, 넉백, 중독 같은 적 전용 효과
- 난이도 계수

즉 지금은 같아도, 설계 관점에서는 "행위 주체가 다르면 규칙도 곧 달라질 것"이라고 보고 API를 미리 분리했을 가능성이 높다.

### 3. 현재 호출 위치를 보면 더 분명하다

플레이어 공격은 `RunController`에서 호출된다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:80)

적 공격은 `EnemyTurnSystem`에서 호출된다. [EnemyTurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/EnemyTurnSystem.cs:42)

즉 상위 호출자도 이미 분리되어 있다.
- 플레이어 턴 흐름 안의 공격
- 적 턴 흐름 안의 공격

그래서 메서드 이름도 그 흐름에 맞춰 나뉜 것으로 볼 수 있다. 상위 레벨에서 읽을 때는 `ResolvePlayerAttack` / `ResolveEnemyAttack`가 `ResolveAttack`보다 의미가 더 즉시 드러난다.

### 4. 지금 구조의 장점과 단점

장점:
- 읽을 때 의도가 분명하다.
- 나중에 플레이어/적 전투 규칙을 따로 확장하기 쉽다.
- 호출자 쪽에서 "지금 누구의 공격을 처리하는지" 바로 보인다.

단점:
- 현재는 중복 코드다.
- `RunState`처럼 아직 안 쓰는 매개변수가 남는다.
- 구현과 인터페이스가 아직 완전히 정리되지 않은 상태로 보인다.

### 5. 가장 현실적인 해석

현재 `CombatSystem`은 "구조는 미래 확장을 바라보는데, 구현은 아직 최소 버전"인 상태다.

그래서 질문에 대한 가장 정확한 답은 이렇다.

`ResolvePlayerAttack`에 `RunState`가 들어가는 이유:
- 현재는 실사용되지 않는다.
- 플레이어 공격이 런 상태와 연결될 미래 확장을 염두에 둔 흔적일 가능성이 크다.

`PlayerAttack`과 `EnemyAttack`을 구분한 이유:
- 현재 계산은 같지만
- 플레이어와 적의 전투 규칙이 앞으로 달라질 가능성이 높아서 API를 미리 분리해 둔 것이다.

즉 지금 코드 기준으로는 둘 다 "설계상 선행 분리, 구현상 미완성"이라고 이해하면 맞다.

## Q6. `RunState`에도 HP가 있고 `Player`에게도 HP가 있는데 굳이 이렇게 만든 이유가 있을까? 아니면 코드 구현하고 남은 잔여물인가?

결론부터 말하면, 완전히 우연한 실수라기보다는 "런 상태 요약값을 `RunState`에 두려는 의도"가 있었던 것으로 보이지만, 현재 구현 수준에서는 정리 덜 된 중복 상태, 즉 잔여물이 섞여 있다고 보는 게 가장 정확하다.

### 왜 중복이 생겼나

현재 구조에는 두 종류의 상태가 있다.

1. 월드 액터 상태
- `ActorEntity.Player.CurrentHp`
- 실제 전투와 생존 판정은 이 값을 직접 사용한다. [ActorEntity.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorEntity.cs:14), [ActorEntity.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorEntity.cs:18)

2. 런 요약 상태
- `RunState.CurrentHp`
- HUD, 결과 처리, 런 전체 상태 묶음 쪽에서 보기 쉬운 값이다. [HudPresenter.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Presentation/UI/HudPresenter.cs:101)

즉 설계 의도만 보면:
- `Player`는 실제 월드에 존재하는 플레이어 엔티티
- `RunState`는 이번 한 판의 상태 스냅샷

으로 나누려 했다고 볼 수 있다.

### 의도는 보이지만, 현재는 중복 상태다

문제는 지금 코드가 두 값을 계속 동기화하고 있다는 점이다.

초기 시작 시:
- `RunController.Begin()`에서 `CurrentRun.CurrentHp = CurrentRun.Player.CurrentHp`로 맞춘다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:43)

붕대 사용 시:
- `CurrentRun.CurrentHp`를 바꾸고
- 다시 `CurrentRun.Player.CurrentHp = CurrentRun.CurrentHp`로 복사한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:131), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:132)

드로트 사용 시:
- 같은 방식으로 다시 맞춘다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:158), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:159)

적 턴 이후:
- 이번에는 반대로 `CurrentRun.CurrentHp = CurrentRun.Player.CurrentHp`로 다시 가져온다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:251)
- `EnemyTurnSystem` 안에서도 같은 동기화가 또 있다. [EnemyTurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/EnemyTurnSystem.cs:48)

이 패턴은 "두 값이 모두 필요하다"기보다, "둘 중 하나만 진실의 원천(source of truth)이어야 하는데 아직 둘 다 들고 있는 상태"라는 신호에 가깝다.

### 그럼 왜 이렇게 뒀을까

가능성은 두 가지다.

1. `RunState`를 화면/UI/결과 계산용 DTO처럼 쓰려는 의도

이 경우에는 `RunState`가 런의 숫자 상태를 한 번에 보여주는 객체가 된다.
- HP
- 턴
- 소모품
- 보상
- 깊이

이런 값들을 `HudPresenter`가 한 객체에서 쉽게 읽을 수 있다. 실제로 HUD는 `boundRun.CurrentHp`, `boundRun.MaxHp`를 직접 본다. [HudPresenter.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Presentation/UI/HudPresenter.cs:101)

이 관점에서는 어느 정도 이유가 있다.

2. 프로토타입에서 프로덕션 구조로 옮기며 남은 중간 단계

하지만 지금 구현을 보면, 월드 쪽은 여전히 `Player`를 직접 본다.
- 카메라 추적은 `CurrentRun.Player.GridPosition` 기준이다. [WorldPresenter.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Presentation/World/WorldPresenter.cs:66)
- 생존 여부도 `ActorEntity.IsAlive`로 본다. [WorldPresenter.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Presentation/World/WorldPresenter.cs:121), [ActorEntity.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorEntity.cs:18)

즉 한쪽은 `RunState`, 다른 한쪽은 `Player`를 보고 있어서 데이터 책임이 아직 완전히 정리되지 않았다.

그래서 현재 상태를 가장 솔직하게 표현하면:
- 처음에는 `RunState`를 런 요약 상태 객체로 두려는 의도가 있었고
- 구현 과정에서는 `Player`가 실제 전투 주체로 남았으며
- 그 결과 HP가 두 군데 존재하게 된 중간 단계다

### 지금 기준으로 더 자연스러운 구조는 무엇인가

보통은 둘 중 하나로 정리한다.

1. `Player.CurrentHp`를 진실의 원천으로 두고
- `RunState`에서는 HP를 없애거나 읽기 전용 계산값으로만 제공한다
- HUD도 `RunState.Player.CurrentHp`를 보게 한다

2. 반대로 `RunState.CurrentHp`를 진실의 원천으로 두고
- 전투 계산도 `RunState`를 기준으로 돌리고
- `Player`는 위치/표현 중심 데이터로 축소한다

현재 코드베이스는 1번이 더 자연스럽다.

이유:
- 실제 전투 피해는 `ActorEntity`에 들어간다. [CombatSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/CombatSystem.cs:31)
- 생존 판정도 `ActorEntity.IsAlive`가 맡는다. [ActorEntity.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorEntity.cs:18)
- 월드 표현도 `Player` 참조를 그대로 쓴다. [WorldPresenter.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Presentation/World/WorldPresenter.cs:66)

즉 실제 게임플레이 쪽의 진짜 HP는 이미 `Player.CurrentHp`에 더 가깝다.

### 정리

질문에 직접 답하면:

"굳이 이렇게 만든 이유가 있나?"
- 어느 정도는 있다.
- `RunState`를 한 판의 숫자 상태 요약 객체로 쓰려는 의도는 읽힌다.

"아니면 구현하고 남은 잔여물인가?"
- 현재 코드 기준으로는 그렇기도 하다.
- 특히 HP는 중복 보관과 반복 동기화가 발생하므로, 구조가 덜 정리된 흔적이라고 보는 편이 맞다.

가장 정확한 한 문장 정리:
- 의도된 요약 상태 설계 위에, 아직 제거되지 않은 중복 구현이 남아 있는 상태다.

## Q7. `InteractionResolver`는 어떻게 확장될 여지가 있어? 왜 따로 클래스로 생성했는지 궁금해.

현재 코드만 보면 `InteractionResolver`는 매우 작다. 실제로 하는 일은:
- `interactable`과 `actor`가 null인지 확인하고
- `interactable.Interact(actor)`를 호출한 뒤
- 성공 여부를 반환하는 것뿐이다. [InteractionResolver.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/InteractionResolver.cs:5)

그래서 지금 시점의 솔직한 답은:
- 당장 복잡해서 따로 뺀 것은 아니다
- 상호작용 "해결 규칙"을 넣을 자리를 미리 분리해 둔 구조에 가깝다

### 현재 역할 분리

지금 `InteractionSystem`은 주로 두 가지를 한다.
- 상호작용 가능한 객체 목록을 관리한다
- 특정 칸에 상호작용 대상이 있는지 찾는다

실제로 `SetInteractables()`로 목록을 받고, `TryGetInteractableAt()`으로 위치 검색을 한다. [InteractionSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/InteractionSystem.cs:17), [InteractionSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/InteractionSystem.cs:38)

반면 실제 "실행"은 `interactionResolver.Resolve(interactable, actor)`로 넘긴다. [InteractionSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/InteractionSystem.cs:28), [InteractionSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/InteractionSystem.cs:32)

즉 현재 구조는:
- `InteractionSystem`: 누구를 상호작용 대상으로 볼지 찾는 쪽
- `InteractionResolver`: 찾은 대상을 실제로 어떻게 처리할지 실행하는 쪽

으로 나뉘어 있다.

### 확장될 여지

현재는 `IInteractable.Interact(actor)` 한 번 호출하면 끝나지만, 상호작용 규칙은 보통 빠르게 커진다. 이럴 때 `InteractionResolver`가 들어갈 자리가 생긴다.

1. 상호작용 가능 여부 검증

예:
- 플레이어만 상호작용 가능
- 특정 아이템이 있어야 열리는 문
- 퀘스트 상태가 맞아야 대화 가능
- 이미 한 번 사용한 오브젝트는 재사용 불가

이런 규칙이 붙으면 `Resolve()` 안에서:
- 대상 타입 확인
- 사용자 타입 확인
- 선행 조건 확인
- 실패 메시지 반환

같은 처리가 필요해진다.

2. 타입별 분기

지금은 `GridInteractable`의 콜백이 직접 모든 일을 한다. [GridInteractable.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/GridInteractable.cs:14)

하지만 나중에 상호작용 종류가 늘어나면:
- 문
- 상인
- 보물상자
- 제단
- NPC 대화
- 함정 해제

처럼 타입별 전처리/후처리가 생길 수 있다. 그러면 `InteractionResolver`가 `InteractableType`에 따라 공통 분기를 담당할 수 있다.

3. 상호작용 비용 처리

예:
- 상호작용도 턴을 소비하는지
- 자원을 지불해야 하는지
- 체력/열쇠/포인트를 차감하는지

현재는 이런 비용 처리가 각 호출자에 흩어질 가능성이 있다. `InteractionResolver`가 있으면 상호작용 실행 직전에 공통 비용 규칙을 넣기 쉽다.

4. 결과/이벤트 후처리

상호작용 후에 자주 붙는 것들:
- 로그 메시지 출력
- 사운드 재생
- 애니메이션 트리거
- 퀘스트 진행도 갱신
- 업적 카운트
- 한 번 사용한 상호작용 제거

이런 후처리를 모든 `Interactable` 구현체에 직접 넣으면 중복이 생긴다. `InteractionResolver`는 공통 후처리 지점이 될 수 있다.

5. 실패/성공 결과를 구조화

현재 `Resolve()`는 `bool`만 반환한다. [InteractionResolver.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/InteractionResolver.cs:7)

나중에는 이런 식으로 커질 수 있다.
- 성공/실패 여부
- 실패 이유
- 소비된 자원
- 턴 소비 여부
- 메시지 텍스트

즉 `InteractionResult` 같은 값을 반환하도록 발전시키기 쉬운 자리가 바로 `InteractionResolver`다.

### 왜 `InteractionSystem`에 그냥 넣지 않았을까

현재도 넣을 수는 있다. 하지만 그러면 `InteractionSystem`이 동시에:
- 목록 저장
- 위치 검색
- 실행 검증
- 타입별 처리
- 후처리

를 다 맡게 된다.

지금은 작아 보여도, 상호작용 시스템은 대개 금방 커진다. 특히 RPG/로그라이크에서는 "찾기"와 "해결하기"를 분리해 두는 편이 낫다.

즉 이 분리는 `TurnSystem`과 비슷한 성격이다.
- 현재는 다소 과분리처럼 보일 수 있지만
- 커질 책임을 미리 따로 떼어 둔 설계다

### 이 프로젝트 코드 기준에서 특히 의미 있는 이유

현재 `RunController`는 이미 `TryHandleRunInteractionAt()` 안에서 상인, reliquary 같은 특수 규칙을 직접 처리하고 있다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:186)

즉 상호작용 규칙의 일부는 이미 `RunController`로 새고 있다.

이 상태에서 상호작용이 더 많아지면:
- 일부는 `RunController`
- 일부는 `GridInteractable` 콜백
- 일부는 `InteractionSystem`

에 흩어질 위험이 있다.

`InteractionResolver`를 유지하면 나중에 이런 로직을 한곳으로 다시 모을 중심점이 생긴다.

### 정리

현재 `InteractionResolver`는 기능적으로는 매우 얇다.
- 지금 당장 꼭 분리되어야 할 정도로 복잡하지는 않다

하지만 구조적으로는 확장 포인트로 의미가 있다.
- `InteractionSystem`은 "대상을 찾는 역할"
- `InteractionResolver`는 "찾은 대상을 실제 규칙에 따라 해결하는 역할"

그래서 가장 정확한 해석은:
- 지금은 얇은 껍데기지만
- 상호작용 검증, 비용, 결과 처리, 타입별 분기, 공통 후처리가 들어갈 자리를 미리 분리해 둔 것이다.

## Q8. 이어서 질문하자면 지금 `RunController`로 새고있는 특수 규칙 일부도 `InteractionResolver`로 이동하는 작업이 필요해?

짧게 답하면:
- "반드시 지금 당장"은 아니다
- 하지만 책임 관점에서는 일부는 `InteractionResolver` 쪽으로 옮기는 편이 더 맞다

즉 우선순위 문제이지, 방향 자체는 옮기는 쪽이 자연스럽다.

### 지금 `RunController`에 새고 있는 규칙은 무엇인가

대표적으로 `TryHandleRunInteractionAt()` 안에 이런 특수 처리들이 있다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:186)

`Merchant`
- 피 조각이 3개 이상인지 확인
- 재고가 남았는지 확인
- 드로트를 구매하고 수량/재고/자원을 갱신

`Reliquary`
- 아직 회수하지 않았는지 확인
- 회수 처리 후 `DepthClear`로 런 종료

이건 단순히 "그 칸에 상호작용 대상이 있는가"가 아니라, "그 대상과 상호작용할 때 어떤 규칙이 적용되는가"에 해당한다. 그래서 책임상 `InteractionResolver`에 더 가깝다.

### 왜 지금은 `RunController`에 있는가

현재 `RunController`는 플레이어 행동의 우선순위를 조정하는 중심 오케스트레이터다.
- 적이 있으면 공격
- 아니면 상호작용
- 아니면 이동

이 흐름 제어 자체는 `RunController`에 있는 게 맞다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:77), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:92), [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:97)

문제는 그 안에 "상호작용 상세 규칙"까지 들어와 있다는 점이다. 그래서 현재는:
- 행동 흐름 제어
- 상호작용 내용 해석

이 한 클래스에 같이 들어가 있다.

### 어떤 것은 옮기고, 어떤 것은 남겨야 하나

이 구분이 중요하다.

`RunController`에 남겨야 하는 것:
- 행동 우선순위 결정
- 이동/공격/상호작용 중 무엇을 시도할지 결정
- 턴 소비 시점 결정
- 런 전체 흐름 제어

`InteractionResolver`로 옮기기 좋은 것:
- 특정 상호작용 타입의 성공 조건
- 상호작용 비용 처리
- 상호작용 결과 적용
- 성공/실패 메시지 생성에 필요한 규칙
- 상호작용 후 발생하는 공통 후처리

즉 "상호작용을 할지 말지"는 `RunController`,
"상호작용이 실제로 무슨 의미인지"는 `InteractionResolver`
로 나누는 게 가장 자연스럽다.

### 그래서 지금 옮겨야 할까

현재 프로젝트 규모만 보면 급한 작업은 아닐 가능성이 높다.

이유:
- 상호작용 종류가 아직 많지 않다
- `Merchant`, `Reliquary` 정도만 있어서 당장 폭발하진 않는다
- 지금 옮기면 `InteractionResolver`가 `RunState`, 메시지, 결과 타입 등 더 많은 의존성을 받게 되어 리팩터링 범위가 커진다

즉 지금 당장 손대면 구조는 좋아질 수 있지만, 작업 대비 실익은 아직 크지 않을 수 있다.

### 하지만 나중에는 옮기는 게 맞는 신호

아래 상황이 오면 옮기는 우선순위가 올라간다.

1. 상호작용 타입이 늘어날 때
- 문
- 잠긴 상자
- NPC 대화 분기
- 함정
- 제단
- 레버

2. 상호작용 조건이 복잡해질 때
- 특정 아이템 필요
- 퀘스트 상태 필요
- 한 번만 가능
- 캐릭터 타입별 반응 다름

3. 상호작용 결과가 다양해질 때
- 메시지
- 보상
- 연출
- 퀘스트 갱신
- 런 종료

4. `RunController.TryHandleRunInteractionAt()`가 길어질 때

이 메서드가 `switch`와 예외 케이스로 계속 커지기 시작하면, 이미 책임이 넘친 신호다.

### 가장 실용적인 결론

현재 기준으로는 "전면 이동"보다 "단계적 이동"이 더 맞다.

추천 방향:
- 지금은 `RunController`가 상호작용 시도 여부를 결정한다
- `Merchant`, `Reliquary` 같은 타입별 규칙만 점진적으로 `InteractionResolver`로 옮긴다
- 나중에 `InteractionResult` 같은 반환 구조를 도입해서
  - 성공 여부
  - 메시지
  - 턴 소비 여부
  - 런 종료 여부
  - 자원 변화
  를 묶어 처리하게 만든다

이렇게 하면 `RunController`는 "무슨 행동을 시도할지"에 집중하고, `InteractionResolver`는 "그 상호작용이 실제로 무엇을 일으키는지"를 담당하게 된다.

### 정리

질문에 직접 답하면:

"이동 작업이 필요해?"
- 장기적으로는 그렇다
- 단기적으로는 필수는 아니다

"왜?"
- 현재 `RunController` 안의 일부 코드는 이미 상호작용 규칙 그 자체이기 때문이다
- 그 책임은 `InteractionResolver` 쪽이 더 자연스럽다

가장 현실적인 한 문장 정리:
- 지금 당장 전부 옮길 필요는 없지만, 상호작용 종류가 늘어나기 전에 타입별 특수 규칙은 `InteractionResolver`로 점진적으로 옮기는 방향이 맞다.

## Q9. `TileOccupancyService`는 당장 쓰는 곳이 없는 것 같은데 어떤 역할이야?

정확히 말하면 "완전히 안 쓰는 것"은 아니다. 다만 "연결은 되어 있는데, 실제 데이터가 채워지지 않아서 지금은 거의 효과가 없는 상태"다.

### 현재 코드에서 맡기로 되어 있는 역할

`TileOccupancyService`는 이름 그대로 "어떤 타일이 점유되어 있는가"를 관리하는 서비스다.

들고 있는 기능은 아주 단순하다.
- 특정 칸이 점유 상태인지 확인
- 특정 칸을 점유/비점유로 표시
- 전체 초기화

코드는 [TileOccupancyService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/TileOccupancyService.cs:6)에 있다.

즉 의도상으로는:
- 플레이어
- 적
- 장애물성 오브젝트
- 혹은 일시적으로 예약된 칸

같은 것을 맵 좌표 기준으로 관리하려는 서비스다.

### 실제로 어디에 연결돼 있나

`GameBootstrap`에서 생성되고, `MapService`에 주입된다. [GameBootstrap.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameBootstrap.cs:69), [GameBootstrap.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameBootstrap.cs:71)

그리고 `MapService.IsWalkable()`는 이 서비스를 사용해서:
- 그 칸이 걸을 수 있는 타일인지
- 그리고 현재 점유되어 있지 않은지

를 같이 체크한다. [MapService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/MapService.cs:28), [MapService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/MapService.cs:33)

즉 설계상 의도는 분명하다.
- 맵의 고정 보행 가능 여부는 `walkableCells`
- 현재 동적인 점유 여부는 `TileOccupancyService`

로 분리하려 한 것이다.

### 그런데 왜 지금은 안 쓰는 것처럼 보이나

핵심 이유는 `SetOccupied()`가 아무 데서도 호출되지 않기 때문이다.

즉:
- 점유 확인 메서드는 있다
- `MapService.IsWalkable()`도 그걸 본다
- 그런데 실제로 플레이어나 적이 움직일 때 점유 정보를 기록하지 않는다

그래서 `occupiedCells`는 항상 비어 있고, 결과적으로 `IsWalkable()`는 지금 거의 `walkableCells.Contains(cell)`처럼 동작한다. [TileOccupancyService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/TileOccupancyService.cs:15)

현재 검색 기준으로도 `SetOccupied()` 호출처는 없다.

### 그럼 지금 누가 점유를 관리하나

실제로는 `ActorRepository`가 이 역할 일부를 대신하고 있다.

예를 들어:
- 특정 칸에 적이 있는지는 `TryGetEnemyAt()`로 확인한다
- 특정 칸이 적에게 점유됐는지는 `IsEnemyOccupied()`로 확인한다
- 아무 액터나 있는지는 `IsOccupied()`로 확인한다

코드는 [ActorRepository.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorRepository.cs:32), [ActorRepository.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorRepository.cs:48), [ActorRepository.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorRepository.cs:62)에 있다.

실제 이동/전투 로직도 이쪽을 더 많이 신뢰한다.
- 플레이어 이동은 적 존재 여부를 `ActorRepository.TryGetEnemyAt()`로 먼저 확인한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:78)
- 적 이동은 `actorRepository.IsEnemyOccupied(targetCell)`로 막는다. [EnemyTurnSystem.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Combat/EnemyTurnSystem.cs:61)

즉 현재 구조는:
- 설계상 점유 서비스: `TileOccupancyService`
- 실사용 점유 판정: `ActorRepository`

로 이중화되어 있고, 실제로는 후자가 쓰이고 있다.

### 왜 이런 클래스가 생겼을까

의도는 나쁘지 않다. 보통 점유 관리는 맵 계층 쪽에 두는 것이 자연스럽다.

이 클래스가 제대로 쓰이면 좋은 점:
- 액터 말고도 여러 점유 요소를 한곳에서 관리 가능
- 임시 예약 칸 같은 것도 표현 가능
- `MapService.IsWalkable()` 하나로 동적 점유까지 판단 가능
- `ActorRepository`와 맵 보행 판정 책임을 더 깔끔하게 분리 가능

즉 원래는 "현재 맵에서 이동 가능한가"를 맵 서비스가 완전히 책임지게 하려 했던 흔적이라고 볼 수 있다.

### 현재 코드 기준 해석

그래서 이 클래스는 두 가지 성격을 동시에 가진다.

1. 설계상 역할
- 동적 타일 점유 상태를 관리하는 서비스

2. 현재 구현 상태
- 아직 실제 흐름에 fully 통합되지 않은 미완성 서비스

즉 "쓸모없는 죽은 코드"라고 단정하긴 어렵지만, 현재는 사실상 미사용에 가깝다.

### 정리

질문에 직접 답하면:

"어떤 역할이야?"
- 원래는 맵 위의 동적 점유 상태를 관리하려는 서비스다.

"왜 지금 안 쓰는 것처럼 보이냐?"
- `MapService`에는 연결돼 있지만, 실제 점유 값을 넣는 `SetOccupied()` 호출이 없어서다.

"그럼 현재 상태는?"
- 의도는 살아 있지만 구현 통합이 덜 된 중간 단계다.
- 실제 점유 판정은 지금 `ActorRepository`가 대신 맡고 있다.

## Q10. 이어서 질문하자면 그럼 `TileOccupancyService`는 삭제해도 되는 클래스야? 아니면 추후 확장될 가능성이 있는 클래스야? 확장된다면 어떤 식으로 사용하게 될까?

짧게 답하면:
- 현재 코드만 기준으로 보면 삭제해도 큰 문제 없이 정리 가능하다
- 하지만 구조적으로는 확장 가치가 있는 클래스다

즉 "지금은 없어도 되지만, 방향만 보면 살려서 제대로 쓰는 편이 더 좋을 수 있다"가 가장 정확하다.

### 1. 지금 삭제해도 되나

현재 상태만 놓고 보면 거의 된다.

이유:
- `TileOccupancyService`의 핵심 기능은 `MapService.IsWalkable()` 안에만 연결돼 있다. [MapService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/MapService.cs:28)
- 그런데 실제 점유 데이터는 아무도 넣지 않는다
- 이동/전투 중 실제 점유 판정은 `ActorRepository` 쪽이 하고 있다

즉 지금은:
- `MapService.IsWalkable()`의 `!tileOccupancyService.IsOccupied(cell)` 부분을 제거해도
- 현재 게임 동작은 거의 달라지지 않을 가능성이 높다

그래서 "지금 코드 정리"만 목표라면 삭제 후보가 맞다.

### 2. 그래도 왜 남길 가치가 있나

이 클래스의 방향 자체는 나쁘지 않다. 오히려 확장되면 유용하다.

핵심 이유는 `ActorRepository` 기반 점유 판정에는 한계가 있기 때문이다.

`ActorRepository`는 말 그대로 액터만 안다.
- 플레이어
- 적
- NPC

하지만 실제 게임에서 타일 점유는 액터만으로 끝나지 않는 경우가 많다.

예:
- 문이 닫혀 있을 때는 타일 점유
- 떨어지는 바위나 함정이 잠시 칸을 막음
- 소환물/투사체/잔류 장판이 칸을 차지함
- 예약 이동 시스템에서 "다음 턴에 이 칸은 들어올 예정" 상태 필요
- 상호작용 오브젝트가 물리적으로 길을 막을 수 있음

이런 것들은 `ActorRepository`로 처리하면 책임이 어색해진다. 반면 `TileOccupancyService`는 "좌표 기반 점유"만 알면 되므로 맵 계층에 두기 적합하다.

### 3. 확장된다면 어떤 식으로 쓰이게 될까

가장 자연스러운 방향은:
- 고정 지형: `walkableCells`
- 동적 점유: `TileOccupancyService`
- 액터 목록/조회: `ActorRepository`

로 역할을 분리하는 것이다.

즉 `MapService.IsWalkable(cell)`이 진짜로:
- 지형상 걸을 수 있는가
- 현재 누가/무엇이 그 칸을 점유 중인가

를 함께 판단하는 중심 메서드가 되게 만드는 방향이다.

#### 확장 예시 1. 액터 이동 점유 동기화

플레이어나 적이 움직일 때:
- 이전 칸 `SetOccupied(oldCell, false)`
- 새 칸 `SetOccupied(newCell, true)`

를 같이 호출한다.

그러면 이동 가능 여부를 `MapService.IsWalkable()` 하나로 더 일관되게 체크할 수 있다.

#### 확장 예시 2. 액터 외 오브젝트 점유

예:
- 닫힌 문
- 파괴 가능한 오브젝트
- 밀어야 하는 상자
- 소환된 벽

이런 것들도 점유 서비스에 등록하면, 액터가 아니어도 보행 불가 판정을 자연스럽게 줄 수 있다.

#### 확장 예시 3. 턴 기반 예약 점유

로그라이크/전술 게임에서는 이런 규칙이 생길 수 있다.
- 적 A가 다음 턴에 저 칸으로 이동 예약
- 플레이어 스킬이 다음 턴에 특정 칸을 막음

이 경우 단순 현재 점유뿐 아니라 "예약 점유" 개념이 필요하다. `TileOccupancyService`는 이런 좌표 기반 상태를 담는 확장 포인트가 될 수 있다.

#### 확장 예시 4. 캐시 역할

지금 `ActorRepository.IsOccupied()`는 매번 액터 목록을 순회한다. [ActorRepository.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Actors/ActorRepository.cs:48)

액터 수가 많아지면 점유 여부를 좌표 집합으로 바로 확인하는 편이 더 효율적일 수 있다. `TileOccupancyService`는 이런 캐시 레이어로도 쓸 수 있다.

### 4. 그럼 지금 프로젝트에서는 어떤 선택이 맞나

선택지는 둘이다.

1. 진짜로 정리할 생각이면 삭제
- 현재 안 쓰는 구조를 걷어낸다
- `MapService`도 더 단순해진다
- 대신 나중에 필요하면 다시 만들어야 한다

2. 확장 방향을 유지할 생각이면 제대로 통합
- 액터 배치/이동 시 점유 상태를 동기화한다
- `MapService.IsWalkable()`를 실제 진실의 원천으로 만든다
- 점유 책임을 `ActorRepository`에서 일부 분리한다

어중간하게 두는 게 제일 안 좋다.

지금 상태는:
- 클래스는 있는데
- 실질적으로 안 쓰이고
- 실제 점유는 다른 클래스가 처리한다

그래서 존재 이유가 흐릿하다.

### 5. 가장 현실적인 판단

현재 코드 규모와 복잡도 기준으로는 삭제해도 된다.

하지만 앞으로 아래 중 하나라도 계획이 있다면 남기고 확장하는 편이 낫다.
- 문/상자/장판 같은 비액터 점유물 추가
- 이동 예약/충돌 규칙 추가
- 좌표 기반 점유 판정을 맵 계층으로 일원화
- 액터 수 증가로 점유 조회 최적화 필요

### 정리

질문에 직접 답하면:

"삭제해도 되는 클래스야?"
- 현재 구현만 기준으로는 그렇다
- 지금 게임 흐름에 핵심적으로 쓰이고 있지는 않다

"추후 확장될 가능성이 있는 클래스야?"
- 충분히 있다
- 특히 액터 외 점유물, 예약 이동, 좌표 기반 충돌 관리가 생기면 자연스럽게 중요해진다

"확장된다면 어떤 식으로 쓰이게 될까?"
- 이동 시 칸 점유를 등록/해제하고
- `MapService.IsWalkable()`가 동적 충돌까지 책임지며
- 액터 외 오브젝트나 예약 점유도 같은 서비스에서 관리하는 방향이 가장 자연스럽다

가장 짧은 한 문장 정리:
- 지금은 삭제 후보지만, 점유를 맵 계층의 책임으로 키울 생각이라면 나중에 꽤 중요한 서비스가 될 수 있다.

## Q11. 이어서 질문하자면 문, 상자, 장판 같은 건 `Interaction`에서 미리 검사해서 `IsWalkable`까지 안 넘어갈 것 같은데 그래도 남겨놓는 게 좋을까?

짧게 답하면:
- "상호작용 가능한 것"만 생각하면 꼭 필요하지 않을 수 있다
- 하지만 "충돌/점유"와 "상호작용"은 성격이 달라서, 둘을 완전히 하나로 보면 나중에 꼬일 가능성이 있다

즉 문, 상자, 장판을 전부 `Interaction`으로만 처리할 수는 있지만, 구조적으로는 점유 계층을 별도로 두는 편이 더 안정적이다.

### 왜 그렇게 보나

현재 플레이어 입력 흐름은 대략 이렇다.
- 적이 있으면 공격
- 아니면 상호작용 시도
- 아니면 이동 가능 여부 검사

이 순서는 `RunController.TryHandlePlayerAction()`에 있다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:70)

이 흐름에서는 분명히 어떤 오브젝트는 `Interaction`에서 먼저 걸러진다.
- 상인
- reliquary
- 기타 상호작용 오브젝트

그래서 질문처럼 "문, 상자도 상호작용으로 먼저 막히면 `IsWalkable`까지 안 가는 것 아닌가?"라는 생각이 자연스럽다.

맞다. 일부는 실제로 그렇게 처리할 수 있다.

### 그런데 상호작용과 점유는 역할이 다르다

핵심 차이는 이것이다.

`Interaction`
- 플레이어가 무언가를 "사용"하거나 "작동"시키는 의미
- 예: 대화하기, 열기, 구매하기, 활성화하기

`Occupancy/Collision`
- 그 칸에 "들어갈 수 있는가"라는 물리적/규칙적 상태
- 예: 막혀 있음, 누가 서 있음, 임시로 위험 영역임

문은 둘 다 가질 수 있다.
- 닫힌 문은 상호작용 대상일 수 있다
- 동시에 닫혀 있을 때는 이동 불가 타일이기도 하다

이 두 성질을 전부 `Interaction`만으로 처리하면, "누가 상호작용하지 않는 상황"에서는 충돌 규칙이 비게 된다.

### `Interaction`만으로 처리할 때 생기는 문제

1. 플레이어가 아닌 주체는 어떻게 할 것인가

현재 상호작용 흐름은 사실상 플레이어 행동 중심이다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:92)

그런데 적 AI나 다른 시스템이 이동할 때도 문/상자/장판의 막힘 여부를 알아야 한다. 이때는 `Interaction`이 아니라 그냥 "들어갈 수 있나?"를 물어야 한다.

즉:
- 플레이어는 상호작용할 수 있지만
- 적은 상호작용하지 못할 수 있고
- 그래도 둘 다 충돌 규칙은 공유해야 한다

이런 경우 점유/충돌 계층이 별도로 있어야 깔끔하다.

2. 상호작용하지 않는 막힘도 존재한다

예:
- 단순 장애물
- 함정 장판
- 일시적으로 막힌 칸
- 소환된 벽
- 낙하물 예약 위치

이런 것은 굳이 `Interactable`이 아닐 수 있다. 그런데 이동 가능 여부는 막아야 한다. 이건 `Interaction`보다 `Occupancy/Collision`의 책임에 가깝다.

3. "상호작용 가능"과 "통과 가능"은 독립적으로 바뀔 수 있다

예를 들어 문:
- 닫힘: 상호작용 가능, 통과 불가
- 열림: 상호작용 가능할 수도 있고, 통과 가능
- 잠김: 상호작용 가능, 통과 불가
- 파괴됨: 상호작용 불가, 통과 가능

이런 상태 변화는 결국:
- 상호작용 규칙
- 통과 규칙

을 분리해 두는 편이 관리하기 쉽다는 뜻이다.

### 그럼 지금 프로젝트에서는 어떤 판단이 맞나

현재 규모에서는 두 가지 중 하나를 명확히 선택하는 게 좋다.

1. 정말 단순하게 갈 거면 `TileOccupancyService` 삭제
- 현재처럼 상호작용은 `Interaction`
- 액터 점유는 `ActorRepository`
- 지형 통과 여부는 `MapService`

로만 유지한다

이 경우는 구조는 덜 일반적이지만 지금 규모에서는 관리 가능하다.

2. 앞으로 오브젝트 종류가 늘어날 거면 `TileOccupancyService`를 살려서 역할 분리
- `Interaction`: 사용/작동 의미
- `Occupancy`: 물리적/규칙적 점유

로 나눈다

이 경우 문/상자 같은 오브젝트는 두 계층을 동시에 가질 수 있다.

예:
- `Interaction`: 문을 연다
- `Occupancy`: 닫혀 있으면 칸 막힘, 열리면 해제

### 가장 현실적인 답

질문의 전제는 맞다.
- 플레이어 상호작용만 보면 문/상자 중 일부는 `Interaction`에서 먼저 잡힌다

하지만 그 사실만으로 `TileOccupancyService`가 완전히 불필요해지지는 않는다.

왜냐하면 이 서비스는 "플레이어 상호작용 이전 검사"용이 아니라, "누가 움직이든 공통으로 적용되는 좌표 점유/충돌 규칙"을 담는 쪽이 더 맞기 때문이다.

그래서 가장 현실적인 판단은 이렇다.

- 프로젝트를 계속 단순하게 유지할 생각이면 삭제해도 된다
- 문/상자/장판/예약 이동 같은 규칙이 늘어날 계획이면 남겨두고, `Interaction`과는 별개 계층으로 키우는 편이 낫다

### 정리

질문에 직접 답하면:

"그래도 남겨놓는 게 좋을까?"
- 현재 범위만 보면 꼭 그렇진 않다
- 하지만 확장 가능성을 보면 남겨둘 가치가 있다

"왜?"
- `Interaction`은 사용/작동 의미를 담당하고
- `Occupancy`는 통과 가능 여부를 담당하기 때문이다

가장 짧은 한 문장 정리:
- 문, 상자 중 일부는 `Interaction`에서 먼저 처리되더라도, 장기적으로는 "상호작용"과 "점유/충돌"을 분리해 두는 편이 더 견고하다.

## Q12. 맵에 있는 `Depth`는 무슨 역할일까? 원래는 층인 줄 알았는데 Cell마다 다르게 조절할 수 있더라고.

코드 기준으로 보면, 현재의 `Depth`는 "맵 전체 층 번호"라기보다 "각 칸이 어느 정도 깊은 구역에 속하는가"를 나타내는 칸별 메타데이터에 가깝다.

즉 지금 구조에서 `Depth`는:
- 전통적인 의미의 "맵 1층, 2층" 같은 전역 층 번호라기보다
- 좌표별로 지정할 수 있는 "구역 깊이 레벨"이라고 보는 게 맞다

### 왜 그렇게 보이나

`MapDefinition` 자체가 `DepthByCell`을 들고 있다. 즉 깊이 정보가 맵 전체 값 하나가 아니라, `Vector2Int -> int` 형태의 칸별 데이터다. [MapDefinition.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/MapDefinition.cs:18)

에셋 쪽도 마찬가지다.
- `MapCellData`에 `Depth`가 들어 있다. [MapCellData.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/MapCellData.cs:11)
- `MapAsset.SetCell(position, walkable, depth)`도 칸마다 depth를 받는다. [MapAsset.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/MapAsset.cs:35)

즉 설계 단계부터 "depth는 cell별 속성"으로 만들어져 있다.

### 현재 실제로 어디에 쓰이나

1. 플레이어가 이번 런에서 얼마나 깊게 들어갔는지 계산하는 기준

플레이어가 이동하면 `RunController`가 그 칸의 depth를 읽고, `DepthReached`를 갱신한다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:104)

즉 의미는:
- 현재 플레이어가 밟은 칸 중
- 가장 큰 depth 값
= 이번 런에서 도달한 최고 깊이

이 값은 보상과 영구 진행도에도 쓰인다. [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:25), [ProgressionService.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Progression/ProgressionService.cs:30)

2. 현재 구역 이름 표시

`RunController.GetDepthName()`은 depth 값에 따라:
- `<= 1`이면 `"Outer Corridor"`
- `>= 2`면 `"Deep Corridor"`

로 텍스트를 바꾼다. [RunController.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Run/RunController.cs:278)

즉 지금 구현에서는 depth가 사실상 "외곽 구역인지, 깊은 구역인지"를 나누는 레벨로 쓰인다.

3. 맵 시각 표현

`WorldPresenter`도 각 cell의 depth를 보고 바닥색을 바꾼다. `depth >= 2`면 더 깊은 구역 색으로 칠한다. [WorldPresenter.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Presentation/World/WorldPresenter.cs:84)

즉 depth는 현재:
- 진행도 판정
- 텍스트 메시지
- 시각적 구역 표현

에 모두 쓰이고 있다.

### 왜 전역 "층"이 아니라 cell별로 두었을까

이렇게 하면 한 맵 안에서도 구역별 깊이를 세밀하게 줄 수 있다.

예를 들어:
- 입구 방은 depth 1
- 중간 복도는 depth 1
- 더 안쪽 복도는 depth 2
- 최심부 방은 depth 3

처럼 설계할 수 있다.

이 방식의 장점:
- 하나의 맵 안에서 점진적으로 "깊은 곳으로 들어간다"는 감각을 줄 수 있다
- 방/복도 단위로 구역 레벨을 다르게 줄 수 있다
- 꼭 맵을 여러 장으로 나누지 않아도 깊이 진행도를 만들 수 있다

즉 이것은 "층"보다 "영역 단계"에 더 가까운 개념이다.

### 기본 생성 맵을 보면 더 명확하다

`MapGenerator.CreateRunMap()`을 보면:
- 첫 방과 중간 복도 일부는 depth 1
- 뒤쪽 복도와 마지막 방은 depth 2

로 나뉜다. [MapGenerator.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Map/MapGenerator.cs:22)

즉 실제 기본 맵도:
- 맵 전체가 한 층이 아니라
- 같은 맵 안의 구역별 깊이를 나눠 둔 구조다

### 그럼 "층" 개념은 전혀 아닌가

완전히 아니진 않다. 현재 프로젝트에서는 플레이어가 이해하는 체감상 "얼마나 안쪽/깊은 층으로 들어갔는가"를 나타내는 역할을 한다.

다만 구현 방식은 전통적인 층 구조와 다르다.

전통적인 층 구조:
- 맵 전체가 1층, 2층, 3층으로 나뉨

현재 구조:
- 하나의 맵 안에서도 cell마다 depth가 다를 수 있음
- 플레이어는 그중 가장 높은 값을 `DepthReached`로 기록함

그래서 플레이 감각으로는 층처럼 보일 수 있지만, 데이터 구조로는 "칸별 구역 깊이"가 맞다.

### 가장 정확한 한 문장 해석

현재의 `Depth`는 "이 cell이 맵 내에서 얼마나 깊은 구역에 속하는가"를 나타내는 값이고, 이를 이용해 플레이어의 최고 도달 깊이와 구역 연출을 계산하는 구조다.

## Q13. `GameDirector` 스크립트의 역할은 뭐라고 정의할 수 있을까?

가장 정확하게 말하면 `GameDirector`는 "게임의 상위 흐름을 조율하는 상태 기반 오케스트레이터"다.

즉 이 클래스는:
- 전투 계산을 직접 담당하지도 않고
- 맵 생성 세부 구현을 직접 담당하지도 않고
- UI를 직접 그리지도 않지만

게임이 지금 어떤 상태에 있고, 그 상태에서 어떤 시스템을 호출해야 하는지를 결정하는 중심 제어기 역할을 한다.

### 왜 그렇게 정의할 수 있나

`Update()`를 보면 `CurrentState`에 따라:
- 허브 입력 처리
- 런 입력 처리
- 결과 화면 입력 처리

를 분기한다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:30), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:37)

즉 `GameDirector`는 "무슨 규칙으로 싸울지"보다, "지금 어떤 상태니까 어떤 흐름을 실행할지"를 결정하는 클래스다.

### 실제 책임을 풀어쓰면

1. 게임 상태 전환 관리

대표적으로 이 클래스는:
- 시작 상태 설정
- 허브 진입
- 런 준비
- 런 시작
- 런 결과 표시

를 관리한다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:51), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:60), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:77), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:83), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:116)

즉 상태 머신의 상위 제어자 역할이 핵심이다.

2. 여러 시스템 연결 및 호출 순서 조율

`GameDirector`는 `ServiceRegistry`를 통해:
- `MapService`
- `InteractionSystem`
- `ProgressionService`
- `WorldPresenter`
- `HudPresenter`
- `RunController`

등을 묶어서 사용한다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:26)

중요한 점은, 이 클래스가 각 시스템 내부 로직을 구현하지는 않는다는 것이다. 대신:
- 언제 맵을 로드할지
- 언제 액터를 세팅할지
- 언제 HUD를 바인딩할지
- 언제 결과를 계산할지

를 순서대로 조합한다.

즉 "서비스를 만든 클래스"라기보다 "서비스들을 순서 있게 엮어 게임 흐름으로 만드는 클래스"다.

3. 상태별 입력 진입점

허브 입력은 `UpdateHubInput()`, 런 입력은 `UpdateRunInput()`, 결과 입력은 `UpdateRunResultInput()`으로 나뉜다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:138), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:171), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:212)

따라서 `GameDirector`는 입력을 세밀하게 해석하는 클래스라기보다, "현재 상태에서 어느 입력 루틴으로 들어갈지"를 정하는 상위 진입점이다.

4. Presentation과 Gameplay 사이 연결

예를 들어 런이 시작되면:
- 맵 로드
- 액터 구성
- 런 상태 생성
- `RunController.Begin()`
- `WorldPresenter.BindRun()`
- `HudPresenter.BindRun()`

같은 흐름을 한 번에 연결한다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:83)

즉 `GameDirector`는 gameplay 결과를 presentation에 반영하도록 이어주는 조정자 역할도 한다.

5. 런 전체 수명주기 관리

한 판이 시작되고 끝날 때의 큰 생명주기를 관리한다.

예:
- 새 프로필로 초기화
- 허브 진입
- 런 시작
- 런 종료 보상 적용
- 결과 화면 노출
- 다시 허브 복귀

이 수명주기 전체를 한 클래스에서 조율한다는 점에서, 단순 입력 처리기나 단순 매니저보다 역할이 더 분명하다.

### 반대로 `GameDirector`의 역할이 아닌 것

이걸 같이 보면 역할 정의가 더 정확해진다.

`GameDirector`가 직접 하지 않는 것:
- 전투 데미지 계산: `CombatSystem`
- 적 턴 계산: `EnemyTurnSystem`
- 세부 런 행동 처리: `RunController`
- 입력 방향 해석: `InputReader`
- 맵 생성: `MapGenerator`
- HUD 그리기: `HudPresenter`
- 월드 렌더링: `WorldPresenter`

즉 `GameDirector`는 "모든 것을 직접 하는 관리자"가 아니라, "각 하위 시스템을 상태에 맞게 호출하는 상위 조율자"다.

### 한 줄 정의 후보

가장 추천하는 정의:

`GameDirector`는 허브, 런, 결과 화면 사이의 상태 전환과 각 상태에서 필요한 시스템 호출 순서를 조율하는 게임 상위 흐름 제어기다.

조금 더 짧게 말하면:

`GameDirector`는 게임의 상태 머신이자 상위 오케스트레이터다.

### 구조적으로 보면

현재 아키텍처에서 역할 계층은 대략 이렇게 읽힌다.

- `GameBootstrap`: 서비스 생성과 초기 연결
- `GameDirector`: 상위 상태/흐름 조율
- `RunController`: 런 내부 규칙 진행
- 개별 시스템들: 전투, 상호작용, 맵, 진행도 등 세부 처리
- Presenters: 화면 반영

이 관점에서 보면 `GameDirector`는 "게임 전체의 상위 레벨 유스케이스를 실행하는 컨트롤러"라고도 정의할 수 있다.

### 주의할 점

현재 코드에서는 `GameDirector`가 상호작용 배치, 액터 생성 같은 구체 작업도 조금 안고 있다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:221), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:249), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:273)

그래서 완전히 순수한 오케스트레이터라고 보기는 어렵고, 일부 조립 책임까지 함께 가진 상태다.

즉 현재는:
- 이상적인 역할: 상위 흐름 조율자
- 실제 구현: 상위 흐름 조율 + 일부 런/허브 조립 작업

으로 보는 게 가장 현실적이다.

### 가장 정확한 한 문장 정리

`GameDirector`는 게임의 현재 상태를 관리하고, 허브/런/결과 화면의 전환 과정에서 필요한 gameplay와 presentation 시스템을 순서대로 조율하는 상위 흐름 제어기다.

## Q14. `SceneInteractableMarker`를 Hub일 때와 Run일 때로 분리해둔 이유가 뭘까?

코드 기준으로 보면, 이건 "허브용 마커 클래스"와 "런용 마커 클래스"를 따로 만든 게 아니라, 하나의 마커에 "허브에서 쓸지 / 런에서 쓸지" 플래그를 둔 구조다.

즉 의도는:
- 같은 `SceneInteractableMarker` 컴포넌트를 재사용하면서
- 현재 게임 상태에 따라 필요한 마커만 골라 쓰기 위해서다

### 근거

`SceneInteractableMarker`는 `useInHub`, `useInRun` 두 플래그를 가진다. [SceneInteractableMarker.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/SceneInteractableMarker.cs:8)

그리고 `GameDirector.FindSceneInteractableMarkers(bool isHub)`는:
- 허브면 `UseInHub == true`
- 런이면 `UseInRun == true`

인 마커만 골라낸다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:393)

이 마커들은 다시:
- 위치를 맵 데이터에 반영할 때도 쓰이고
- 실제 상호작용 목록을 만들 때도 쓰인다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:360), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:261), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:285)

### 왜 굳이 이렇게 했을까

1. 같은 씬 배치 방식을 허브와 런에서 공통으로 쓰기 위해서다.

이 구조에서는 디자이너나 개발자가 씬에 marker를 배치해 두고:
- 이건 허브에서만 쓰기
- 이건 런에서만 쓰기
- 이건 둘 다 쓰기

를 인스펙터 체크박스로 조절할 수 있다.

즉 클래스나 로직을 두 벌 만들지 않고, 하나의 편집 방식으로 두 상태를 모두 커버하려는 의도다.

2. 맵 데이터와 씬 배치 사이를 느슨하게 연결하려는 의도다.

`GameDirector`는 기본 `MapDefinition` 안의 위치값을 가지고 시작하지만, 씬에 마커가 있으면 그 위치로 덮어쓴다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:360)

이때 허브와 런이 서로 다른 위치 세트를 가져야 하므로, "지금 찾은 marker가 어느 상태에 속하는가"를 구분할 필요가 있다.

즉 이 플래그는 단순 시각용이 아니라, 런타임에 맵 데이터 오버라이드 범위를 정하는 역할도 한다.

3. 완전히 다른 클래스 두 개를 만들 필요가 없기 때문이다.

예를 들어 굳이 나누면 이런 식이 된다.
- `HubInteractableMarker`
- `RunInteractableMarker`

하지만 현재 두 클래스가 가져야 할 데이터는 거의 같다.
- `InteractableType`
- `GridPosition`
- 상태별 사용 여부

그렇다면 타입을 쪼개기보다 플래그로 제어하는 편이 더 단순하다.

4. 같은 마커를 양쪽에서 재사용할 여지를 남기기 위해서다.

지금은 허브/런의 상호작용 타입이 다를 수 있지만, 구조상으로는 어떤 마커는 둘 다에서 유효할 수도 있다.

예:
- 테스트용 공통 상호작용 포인트
- 상태에 따라 같은 위치에서 다른 의미를 가질 수 있는 오브젝트
- 허브/런 전환 후에도 유지되는 연출용 기준점

이럴 때 `useInHub`, `useInRun`을 둘 다 켜 두면 하나의 마커를 공용으로 쓸 수 있다.

### 실제 코드 흐름으로 보면

허브 진입 시:
- `ApplySceneInteractablePositions(isHub: true)`
- `BuildHubInteractables()`

런 시작 시:
- `ApplySceneInteractablePositions(isHub: false)`
- `BuildRunInteractables()`

가 호출된다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:74), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:76), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:97), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:99)

즉 같은 "marker 검색" 메커니즘을 두 상태에서 공통으로 쓰되, 필터만 바꾸는 구조다.

이건 꽤 실용적인 패턴이다.
- 편집 방식은 하나
- 사용 조건만 상태별로 다름

### 장점

- 허브/런 모두 같은 컴포넌트로 관리 가능
- 씬에서 배치 후 체크박스로 제어 가능
- 위치 덮어쓰기와 상호작용 생성 로직을 공통화할 수 있음
- 필요하면 한 마커를 양쪽 상태에서 공유 가능

### 한계

이 구조는 단순할 때는 좋지만, 상태별 규칙이 크게 달라지면 한계가 있다.

예를 들어 나중에:
- 허브 마커만 필요한 속성
- 런 마커만 필요한 속성
- 상태별로 완전히 다른 동작 데이터

가 많아지면, 단순 `useInHub` / `useInRun` 플래그만으로는 부족해질 수 있다.

그때는:
- marker 데이터를 더 세분화하거나
- 타입별 전용 컴포넌트로 나누는 쪽

이 더 나아질 수 있다.

### 가장 정확한 한 문장 정리

`SceneInteractableMarker`를 Hub/Run으로 나눠 둔 이유는, 같은 마커 배치 시스템을 재사용하면서 현재 게임 상태에 맞는 상호작용 위치와 오브젝트만 선택적으로 활성화하기 위해서다.

## Q15. 좀 더 자세하게 설명해줘. 그냥 플래그를 안 쓰고 인터렉션을 하면 문제가 발생해?

짧게 답하면:
- 네, 현재 구조에서는 플래그 없이 모든 marker를 항상 읽으면 실제로 문제가 생길 수 있다
- 특히 허브와 런이 같은 씬/같은 marker 검색 로직을 공유하는 구조라서 더 그렇다

핵심은 `SceneInteractableMarker`가 단순 표시용이 아니라:
- 맵 위치를 덮어쓰는 데도 쓰이고
- 실제 상호작용 목록을 만드는 데도 쓰인다는 점이다

즉 플래그가 없으면 "보이면 상호작용된다" 수준의 문제가 아니라, 아예 허브/런용 데이터가 서로 섞일 수 있다.

### 현재 흐름을 먼저 보면

허브에 들어갈 때:
- `ApplySceneInteractablePositions(isHub: true)`
- `BuildHubInteractables()`

런을 시작할 때:
- `ApplySceneInteractablePositions(isHub: false)`
- `BuildRunInteractables()`

가 호출된다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:74), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:76), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:97), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:99)

그리고 이 둘은 공통으로 `FindSceneInteractableMarkers(isHub)`를 호출해서 marker를 찾는다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:393)

즉 플래그는 "찾을 marker 집합"을 나누는 필터 역할이다.

### 플래그가 없으면 어떤 문제가 생기나

#### 1. 허브용 marker가 런에도 섞일 수 있다

예를 들어 씬에 이런 marker들이 있다고 하자.
- `StartAltar`
- `UnlockAltar`
- `Undertaker`
- `Merchant`
- `Reliquary`

이 중 앞의 3개는 허브용이고, 뒤의 2개는 런용이다.

플래그 없이 전부 읽으면 런 시작 시에도 허브용 marker를 같이 읽게 된다.

그러면 `ApplySceneInteractablePositions(isHub: false)`에서:
- 허브에서만 의미 있어야 할 위치값까지 현재 맵에 덮어쓸 수 있다

그리고 `BuildRunInteractables()`에서도:
- 런에서 필요 없는 marker가 상호작용 목록에 포함될 수 있다

즉 문제는 단순히 "사용 안 할 marker가 좀 더 들어간다" 수준이 아니라, 현재 상태의 맵 정의 자체가 오염될 수 있다는 점이다.

#### 2. 런용 marker가 허브에도 섞일 수 있다

반대로 허브 진입 시에도 런용 marker가 같이 읽히면:
- `MerchantPosition`
- `ReliquaryPosition`

같은 런 전용 위치가 허브 맵 데이터에 반영될 수 있다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:378), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:381)

현재 허브에서는 이 값들을 직접 적극적으로 쓰지 않을 수는 있다. 하지만 구조상 허브 맵에 런 전용 데이터가 섞이는 것은 좋지 않다.

이건 "지금 당장 버그가 난다"보다 "상태별 데이터 경계가 무너진다"는 문제다.

#### 3. 상호작용 목록이 상태와 무관하게 섞인다

`BuildHubInteractables()`와 `BuildRunInteractables()`는 marker를 기반으로 `GridInteractable` 목록을 만든다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:261), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:285)

플래그가 없으면:
- 허브에서 Merchant/Reliquary marker가 목록에 들어갈 수 있고
- 런에서 StartAltar/UnlockAltar/Undertaker marker가 목록에 들어갈 수 있다

그러면 현재 상태에서 의미 없는 오브젝트가 상호작용 대상으로 등록된다.

이건 나중에 이런 식의 버그로 이어질 수 있다.
- 허브에서 런 전용 오브젝트와 충돌
- 런에서 허브용 오브젝트와 상호작용 시도
- 메시지나 동작이 현재 상태와 맞지 않게 실행

#### 4. fallback 로직을 깨뜨릴 수 있다

현재 `BuildRunInteractables()`와 `BuildHubInteractables()`는:
- scene marker가 하나라도 있으면 그것을 우선 사용하고
- 없으면 기본 위치값으로 fallback 한다

는 구조다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:264), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:288)

여기서 플래그 없이 "상태와 상관없는 marker"가 몇 개라도 잡히면:
- 사실상 해당 상태용 marker가 없는 상황인데도
- marker가 있는 것으로 간주되어 fallback이 막힐 수 있다

이게 꽤 중요하다.

예를 들어 허브용 marker는 없고 런용 marker만 씬에 있는데, 플래그가 없으면 허브에서도 "scene marker가 있다"고 판단해 버릴 수 있다. 그러면 원래 써야 할 기본 허브 배치가 무시될 수 있다.

즉 플래그는 단순 필터가 아니라 fallback 판단을 정확하게 만드는 장치이기도 하다.

#### 5. 같은 타입이라도 상태마다 의미가 다를 수 있다

중요한 점은 "같은 marker 타입이면 어디서든 써도 되는 것 아니냐"가 아니라는 것이다.

예를 들어 `InteractableType.StartAltar`는 허브에서만 `StartRun()`을 의미한다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:393), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:409)

반대로 `Merchant`나 `Reliquary`는 런에서만 의미가 있다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:261)

즉 같은 "상호작용 marker 시스템"을 공유하더라도, 그 marker가 어느 상태에서 유효한지는 별도 정보가 필요하다. 그 역할을 하는 것이 `useInHub` / `useInRun` 플래그다.

### 플래그 없이도 할 수는 없는가

할 수는 있다. 다만 다른 방식으로 상태 구분을 해야 한다.

예를 들면:
- 허브 씬과 런 씬을 완전히 분리
- 허브용 marker 컴포넌트와 런용 marker 컴포넌트를 분리
- marker를 상태별 부모 오브젝트 아래 배치해서 계층으로 필터링
- 타입 자체를 허브 전용 / 런 전용으로 더 세분화

즉 플래그가 꼭 유일한 방법은 아니다.

하지만 현재 구조처럼:
- 같은 `SceneInteractableMarker` 타입을 쓰고
- `FindObjectsByType<SceneInteractableMarker>()`로 전부 수집한 뒤
- 그중 일부만 쓰는 방식이면

플래그 같은 필터 정보가 거의 필수다.

### 왜 지금 구조에서 특히 중요한가

현재 구현은 "씬에 있는 marker를 읽어서 맵 데이터와 상호작용 데이터를 동시에 구성"하는 패턴이다.

즉 marker는 단순 장식이 아니라 런타임 구성 데이터다.

그래서 플래그가 없으면:
- 허브와 런의 데이터 경계가 흐려지고
- fallback가 틀어질 수 있고
- 상태에 맞지 않는 상호작용이 등록되며
- 나중에 씬 배치가 조금만 복잡해져도 디버깅이 어려워진다

### 가장 현실적인 정리

질문에 직접 답하면:

"플래그를 안 쓰고 인터렉션을 하면 문제가 발생해?"
- 네, 현재 구조에서는 충분히 발생할 수 있다
- 특히 marker가 위치 오버라이드와 상호작용 등록 둘 다에 쓰이기 때문에 더 그렇다

"어떤 문제?"
- 허브/런용 marker 데이터가 서로 섞임
- 상태에 맞지 않는 상호작용이 등록됨
- fallback 배치가 깨짐
- 씬 배치가 복잡해질수록 예측이 어려워짐

가장 짧은 한 문장 정리:
- 현재 구조에서 플래그는 단순 편의 기능이 아니라, 허브와 런의 marker 데이터를 섞이지 않게 하는 안전장치다.

## Q16. 현재 구조에서 만약 런의 구조가 바뀐다면 문제가 생기지 않을까? 현재는 프로토 타입이라 한층을 내려가면 회랑으로 다시 돌아오지만 원 기획에서는 한층을 내려가면 그다음층이 나와야해. 죽기 전까지는 회랑으로 돌아갈 수 없는 구조야. 그러면 층마다 플래그를 만들어야 하는거야?

짧게 답하면:
- 네, 현재 구조는 "허브 vs 런" 정도의 2상태 전제에 강하게 맞춰져 있어서 다층 구조로 가면 한계가 드러난다
- 하지만 해결책이 "층마다 플래그 추가"는 아니다

즉 문제 인식은 맞고, 해법은 플래그 확장이 아니라 구조 변경 쪽이 맞다.

### 왜 현재 구조가 한계에 부딪히나

현재 marker 구조는:
- `useInHub`
- `useInRun`

두 플래그만 가지고 있다. [SceneInteractableMarker.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Gameplay/Interaction/SceneInteractableMarker.cs:8)

그리고 `GameDirector`도 상태를 크게:
- `Hub`
- `InRun`
- `RunResult`

같은 상위 단계로 나눈다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:37)

즉 현재 아키텍처는 "허브냐 아니냐" 수준의 분기에는 잘 맞지만,
"런 내부에 여러 층이 있고 각 층이 서로 다른 구성/배치를 가진다"는 구조에는 직접 맞지 않는다.

지금은 런 전체가 사실상 하나의 맵 묶음처럼 취급된다.
- `LoadRunMap()`
- 런용 interactable 세팅
- reliquary 회수 후 종료

라는 흐름으로 끝난다. [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:85), [GameDirector.cs](/D:/Unity/Arkeum/Assets/Arkeum/Scripts/Production/Core/GameDirector.cs:88)

그러니 원 기획처럼:
- 1층
- 2층
- 3층
- 죽기 전까지 허브 복귀 없음

이 되면, 현재의 "런 하나" 안에 여러 floor lifecycle이 들어와야 한다.

### 그럼 층마다 플래그를 만들면 되나

권장하지 않는다.

예를 들어 이런 식이 되기 쉽다.
- `useInHub`
- `useInRunFloor1`
- `useInRunFloor2`
- `useInRunFloor3`
- ...

이건 금방 망가지는 구조다.

이유:

1. 상태가 늘어날수록 marker가 데이터 폭탄이 된다

marker 하나마다 층 수만큼 체크박스를 들고 있어야 한다. 층이 늘거나 줄면 모든 marker 정의가 흔들린다.

2. "어느 층에서 쓰는가"를 bool 집합으로 표현하는 건 정보 구조가 좋지 않다

층 번호는 본질적으로:
- 0
- 1
- 2
- 3

처럼 값 하나로 표현할 수 있는 정보다.

그런데 bool 여러 개로 쪼개면:
- 중복 체크 가능
- 모순 가능
- 실수 가능

이 커진다.

3. 층별 전용 규칙이 늘수록 더 버티기 어렵다

나중에 층마다 달라질 수 있는 건 단순 사용 여부만이 아니다.
- 등장 오브젝트
- 상호작용 타입
- 적 배치
- 시각 연출
- 맵 규칙
- 배경음

즉 결국 "층은 별도 데이터 단위"가 되어야지, marker의 체크박스 옵션으로 감당할 문제가 아니다.

### 더 자연스러운 방향은 무엇인가

런 내부에 "현재 층" 개념을 따로 두는 쪽이 맞다.

즉 구조를 이렇게 바꾸는 게 자연스럽다.

- 허브 여부는 상위 게임 상태로 관리
- 런 내부 floor index / floor id는 `RunState`나 별도 floor state로 관리
- marker/맵/배치는 현재 층 데이터에 따라 로드

이렇게 되면 marker도 "Hub냐 Run이냐" 플래그보다,
"이 marker는 어느 floor definition에 속하는가"
로 관리하는 쪽이 맞다.

### 구체적으로는 어떤 식이 좋나

#### 방향 1. 층마다 맵 데이터를 분리

가장 단순하고 튼튼한 방식이다.

예:
- `Floor1MapAsset`
- `Floor2MapAsset`
- `Floor3MapAsset`

혹은
- `RunFloorDefinition[]`

처럼 런 내부 층 데이터를 따로 둔다.

그러면 각 층은:
- 자체 walkable cell
- 자체 interactable 위치
- 자체 적 배치
- 자체 depth/테마

를 가진다.

이 경우 marker도 굳이 한 씬에서 bool 필터링할 필요가 줄어든다. 아예 floor asset 자체에 속하는 데이터로 넣는 편이 낫다.

#### 방향 2. marker에 floor id 하나를 둔다

정말 scene marker 방식을 유지하고 싶다면 bool 여러 개보다 이쪽이 낫다.

예:
- `Scope = Hub`
- `Scope = Run`
- `FloorIndex = 0, 1, 2 ...`

혹은
- `MarkerContext = Hub / Floor`
- `FloorId = 2`

처럼 "소속 층"을 값 하나로 표현하는 방식이다.

이 방식은 bool 나열보다 훨씬 낫지만, 층 구성이 커질수록 결국 asset 중심 구조가 더 좋아질 가능성이 높다.

#### 방향 3. 층 전환을 `RunController`/`GameDirector`가 별도 처리

원 기획처럼 층을 내려가면:
- 런은 계속 유지되고
- 현재 floor만 교체되며
- 플레이어 상태와 런 자원은 유지

되어야 한다.

그럼 필요한 것은 "허브 복귀"가 아니라 "다음 floor 로드"다.

즉 현재의:
- `StartRun()`
- `ShowRunResult()`
- `EnterHub()`

중간에
- `AdvanceToNextFloor()`

같은 lifecycle이 들어가야 한다.

이건 marker 플래그 문제 이전에, 런 수명주기 구조가 바뀌어야 한다는 뜻이다.

### 현재 코드에서 어디가 특히 바뀌어야 하나

1. `GameDirector`
- 지금은 런 하나를 하나의 맵으로 본다
- 다층 구조면 "런 내부 floor 전환"을 관리해야 한다

2. `RunState`
- 현재는 `DepthReached` 정도만 있지만
- 실제 `CurrentFloorIndex`나 `VisitedFloors` 같은 상태가 필요할 수 있다

3. `MapService`
- 현재 맵 하나만 들고 있다
- floor 교체를 지원해야 한다

4. interactable/actor 배치
- 층 진입 시마다 다시 구성해야 한다

즉 핵심 문제는 marker 플래그 수가 아니라, "런 내부 floor lifecycle"을 현재 구조가 아직 직접 표현하지 못한다는 점이다.

### 가장 정확한 정리

질문에 직접 답하면:

"현재 구조에서 런 구조가 바뀌면 문제가 생기지 않을까?"
- 맞다
- 지금 구조는 단층 프로토타입에 더 잘 맞는다

"그러면 층마다 플래그를 만들어야 하는 거야?"
- 아니다
- 그건 임시방편이고 구조적으로 좋지 않다

"그럼 어떻게 해야 해?"
- 층을 marker 플래그가 아니라 별도 floor 데이터/상태로 승격해야 한다
- 허브/런은 상위 상태
- floor는 런 내부 상태

로 나누는 게 맞다

가장 짧은 한 문장 정리:
- 원 기획처럼 다층 런으로 갈 거면, 층은 체크박스 집합으로 표현할 게 아니라 런 내부의 독립된 데이터와 상태로 표현해야 한다.
