# 작업 입력 기록

## 2026-05-18 2D 탑다운 턴제 타일 로그라이크 1루프 개발 로드맵 정리

### 사용자의 요청 개요
- `Docs/_workspace/development_roadmap.md`를 스토리 중심 개발 계획이 아니라, 2D 탑다운 턴제 타일 로그라이크의 1루프 완성을 위한 개발 순서로 수정해달라는 요청.

### 핵심 요구사항
- 목표 루프를 `1층 시작 -> 열린 방 탐색 -> 위험 타일 예측 -> 이동/공격/무기 선택 -> 보상 획득 -> 여러 층 진행 -> 보스전 -> 사망 또는 클리어 후 다시 1층`으로 재정의.
- 핵심 재미를 `턴마다 위험 타일을 읽고 가장 좋은 위치와 공격 수단을 판단하는 긴장감`으로 정리.
- 타일 이동, 위험 타일 예고, 적 행동, 무기별 공격 범위, 타이밍 ON/OFF, 열린 방, 보상, 층 진행, 보스전, 루프 재시작 순서로 개발 단계를 재구성.
- 1차 완성 목표와 밸런싱 기준을 포함.

### 이번 작업 범위
- 기존 `Docs/_workspace/development_roadmap.md`의 내용을 요청된 신규 로드맵 구조로 전면 갱신.
- 실제 파일 변경이 발생했으므로 `Docs/input.md`를 새로 작성.

### 변경된 파일과 변경 목적
- `Docs/_workspace/07_development_roadmap.md`
  - 2D 탑다운 턴제 타일 로그라이크의 1루프 완성을 기준으로 개발 단계, 완료 기준, MVP 범위, 밸런싱 기준을 재작성.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 변경 파일, 수행 내역, 미확인 사항을 기록.

### 실제 수행한 작업 요약
- 1루프 목표와 핵심 방향을 문서 상단에 명시.
- 0단계부터 13단계까지 단계별 목표, 구현 요소, 완료 기준을 정리.
- 무기 1차 세트, 방 타입 1차 세트, 몬스터 1차 세트, 보상/성장, 층 구조, 보스전, 루프 종료/재시작을 개발 순서에 맞게 배치.
- 최종 개발 순서 요약과 1차 완성 목표를 별도 섹션으로 정리.
- 로드맵의 핵심 한 줄을 문서 말미에 추가.

### 빌드/테스트 여부
- 문서 수정 작업만 수행했으며 빌드나 테스트는 실행하지 않았다.

### 확인하지 못한 사항 또는 후속 점검 사항
- 실제 Unity 프로젝트 구현 상태와 문서 내용의 일치 여부는 확인하지 않았다.
- 기존 로드맵 파일은 콘솔 출력상 한글이 깨져 보여, 요청된 내용 기준으로 문서 전체를 새 구조로 정리했다.

## 2026-05-18 출구방 자동 배치 구현

### 사용자의 요청 개요
- 런 던전 생성 시 시작 방으로부터 그래프상 가장 멀리 떨어진 방을 출구방으로 삼는 쉬운 방식의 출구 배치 기능 구현 요청.

### 핵심 요구사항
- 실좌표 거리나 셀 이동 거리가 아니라 방과 통로를 노드/엣지 그래프로 보았을 때 시작 방에서 가장 멀리 떨어진 방을 선택한다.
- 선택된 방 내부에 `FloorExitPosition`을 배치해 기존 층 클리어/다음 층 이동 흐름과 연결한다.
- 기존 특수방/일반방 생성 구조는 크게 바꾸지 않는다.

### 이번 작업 범위
- 절차 생성이 완료된 `MapDefinition.Rooms`와 `MapDefinition.Corridors`를 기준으로 출구 위치를 후처리하는 로직을 추가했다.
- 출구 전용 `MapAsset` 또는 별도 출구방 타입은 추가하지 않았다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - `ApplyRunMarkers()`에서 시작 방 0번 기준 BFS로 가장 먼 방을 찾고, 해당 방 중앙에 가까운 셀을 `map.FloorExitPosition`으로 지정하도록 변경했다.
  - 방 그래프 인접 리스트 생성, 가장 먼 방 탐색, 방 조회, 출구 셀 선택 헬퍼를 추가했다.

### 실제 수행한 작업 요약
- 통로 정보를 양방향 그래프 엣지로 변환했다.
- 시작 방 `roomId = 0`에서 BFS를 수행해 그래프 depth가 가장 큰 방을 선택했다.
- 선택된 방의 `Min`/`Max` 중심점에 가장 가까운 방 셀을 출구 위치로 지정했다.
- 일반 던전 생성과 fallback 던전 생성 모두 기존 `ApplyRunMarkers()` 호출을 통해 같은 출구 배치 규칙을 사용한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 던전 생성 후 출구 마커 표시와 상호작용은 확인하지 못했다.
- 현재 `RunDefinition.asset`에는 1층만 등록되어 있어, 출구 사용 후 실제 다음 층 진행을 확인하려면 2층 이상의 `RunFloorDefinition` 추가가 필요하다.
- PowerShell 환경에서 `git` 명령을 찾지 못해 git diff는 확인하지 못했다.

## 2026-05-18 출구 전용 방 부착 방식 변경

### 사용자의 요청 개요
- 가장 먼 방 내부에 출구를 찍는 방식이 아니라, 시작 방에서 그래프상 가장 먼 방에 다음 층으로 넘어가는 특수 출구방을 붙이는 방식으로 변경 요청.

### 핵심 요구사항
- 방과 통로를 그래프로 보고 시작 방에서 가장 먼 방을 찾는다.
- 찾은 방에 별도의 출구 전용 방을 연결한다.
- 출구 전용 방 내부의 `FloorExitPosition`을 실제 층 이동 출구로 사용한다.
- 필요한 정보가 있으면 요청하되, 구현 가능한 기본 구조는 먼저 반영한다.

### 이번 작업 범위
- `RunFloorDefinition`에 출구 전용 방 에셋을 지정하는 `ExitRoomAsset` 필드를 추가했다.
- `MapGenerator`가 기본 방 생성을 마친 뒤 가장 먼 방에 `ExitRoomAsset` 기반 방을 추가로 붙이도록 변경했다.
- `ExitRoomAsset`이 없거나 연결에 실패하면 기존처럼 가장 먼 방 내부 중앙 근처에 출구를 배치하는 fallback을 유지했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunFloorDefinition.cs`
  - 층별 출구방 템플릿을 지정할 수 있도록 `ExitRoomAsset` 필드 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 출구방 템플릿 생성, 일반 방 후보에서 출구방 에셋 제외, 가장 먼 방에 출구방 부착, 출구방의 `FloorExitPosition` 반영 로직 추가.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과와 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 기존 생성된 `MapDefinition.Rooms`/`Corridors` 기준으로 시작 방 0번에서 가장 먼 방을 찾는다.
- 가장 먼 방의 상/하/좌/우 인접 grid 중 비어 있고 문/통로 연결이 가능한 방향을 찾아 출구방을 추가한다.
- 출구방 `MapAsset.FloorExitPosition`이 지정되어 있으면 해당 위치를 최종 `map.FloorExitPosition`으로 변환한다.
- 출구방에 `FloorExitPosition`이 없으면 출구방 중앙에 가까운 셀을 출구로 지정한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Inspector에서 각 층의 `ExitRoomAsset`을 실제 출구방 `MapAsset`으로 지정해야 출구방 부착 방식이 활성화된다.
- 출구방으로 쓸 `MapAsset`에는 연결 가능한 문 데이터와 출구 위치 `FloorExitPosition`을 지정하는 것이 좋다.
- Unity Play Mode에서 실제 생성 결과, 출구방 표시, 층 이동 상호작용은 아직 확인하지 못했다.
- 현재 `RunDefinition.asset`에는 1층만 등록되어 있어 다음 층 이동까지 확인하려면 2층 이상의 `RunFloorDefinition` 추가가 필요하다.

## 2026-05-18 특수방 타입 enum 및 출구방 특수방화

### 사용자의 요청 개요
- 특수방 enum을 만들고, 출구방도 별도 필드가 아니라 특수방의 한 종류로 취급하도록 수정 요청.

### 핵심 요구사항
- 특수방 타입을 enum으로 구분한다.
- 출구방은 특수방 타입 중 하나로 관리한다.
- 기존 가장 먼 방에 출구 전용 방을 붙이는 흐름은 유지한다.

### 이번 작업 범위
- `RunSpecialRoomType` enum을 추가했다.
- `RunSpecialRoomDefinition`에 `RoomType` 필드를 추가했다.
- 이전 작업에서 추가했던 `RunFloorDefinition.ExitRoomAsset` 별도 필드는 제거했다.
- `SpecialRooms` 중 `RoomType == FloorExit`인 항목을 출구방 템플릿으로 사용하도록 `MapGenerator`를 수정했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunFloorDefinition.cs`
  - `RunSpecialRoomType.Generic`, `RunSpecialRoomType.FloorExit` enum 추가.
  - `RunSpecialRoomDefinition.RoomType` 추가.
  - 출구방 별도 필드 `ExitRoomAsset` 제거.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapDefinition.cs`
  - 생성된 방이 어떤 특수방 타입인지 보존할 수 있도록 `DungeonRoomDefinition.SpecialRoomType` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 특수방 템플릿 생성 시 enum 타입을 함께 보존.
  - `FloorExit` 타입 특수방은 일반 특수방 슬롯이 아니라 가장 먼 방에 추가 부착되는 출구방으로 사용.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과와 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 일반 특수방은 `Generic` 타입으로 기존처럼 랜덤 특수방 슬롯에 배치된다.
- 출구방은 `SpecialRooms`에 `FloorExit` 타입으로 등록된 첫 번째 유효 항목을 사용한다.
- 출구방은 일반 방 후보와 일반 특수방 슬롯에서 제외되고, 가장 먼 방에 추가 연결된다.
- 생성된 `DungeonRoomDefinition`에 `IsSpecialRoom = true`와 `SpecialRoomType = FloorExit`가 함께 남는다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Inspector에서 `RunDefinition`의 각 층 `SpecialRooms`에 출구방 항목을 추가하고 `RoomType`을 `FloorExit`으로 지정해야 한다.
- 기존에 별도 `ExitRoomAsset` 필드를 Inspector에서 사용 중이었다면, 이제 `SpecialRooms` 항목으로 옮겨야 한다.
- Unity Play Mode에서 실제 출구방 부착, 출구 마커 표시, 층 이동 상호작용은 아직 확인하지 못했다.

## 2026-05-18 벽 오브젝트 데이터/에디터/차단 처리 추가

### 사용자의 요청 개요
- 보스방 문 닫힘과 향후 시야/공격 차단에 사용할 벽 오브젝트를 먼저 구현하고, 맵 에디터에서도 배치/수정할 수 있게 해달라는 요청.

### 핵심 요구사항
- 벽은 단순 비가시 장식이 아니라 이동, 시야, 공격을 차단하는 오브젝트로 취급한다.
- 맵 에디터에서 벽을 배치하고 제거할 수 있어야 한다.
- 기존 `MapAsset` 기반 방 템플릿과 절차 생성 맵에 벽 정보가 전달되어야 한다.

### 이번 작업 범위
- `MapCellData`에 벽 여부를 저장하는 `HasWall` 필드 추가.
- `MapDefinition`에 생성된 맵의 벽 좌표 목록 `WallCells` 추가.
- `MapAssetEditorWindow`에 `Wall`, `WallErase` 브러시 추가.
- `MapGenerator`가 방 템플릿의 벽 좌표를 최종 생성 맵으로 변환하도록 수정.
- `MapService`가 벽 셀을 이동 불가, 시야 차단, 공격 차단 셀로 조회할 수 있게 수정.
- 월드 렌더링에서 벽 셀을 별도 시각 요소로 표시하도록 수정.
- 적 이동, 적 감지/공격, 플레이어 공격이 벽 차단을 일부 반영하도록 수정.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Map/MapCellData.cs`
  - 셀 단위 벽 오브젝트 플래그 `HasWall` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapAsset.cs`
  - 벽 배치/제거용 `SetWall()` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapDefinition.cs`
  - 런타임 생성 맵의 벽 좌표 `WallCells` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - `MapAsset`의 벽 정보를 방 템플릿과 배치된 방으로 전달하고 `MapDefinition.WallCells`에 기록.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapService.cs`
  - `IsWalkable()`에서 벽 셀을 이동 불가로 처리.
  - `BlocksLineOfSight`, `BlocksAttack`, `BlocksLineOfSightBetween`, `BlocksAttackBetween` 조회 추가.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 적 이동은 벽을 통과하지 않도록 `IsWalkable()` 기준으로 변경.
  - 적 감지/공격이 벽 차단을 확인하도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 플레이어 공격이 공격 경로의 벽 차단을 확인하도록 변경.
- `Assets/Arkeum/Scripts/Presentation/World/WorldVisualSet.cs`
  - 벽 스프라이트/색상 설정 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 생성된 벽 셀을 화면에 표시.
- `Assets/Arkeum/Scripts/Editor/MapAssetEditorWindow.cs`
  - 벽 배치/삭제 도구와 검증 반영.

### 실제 수행한 작업 요약
- 벽은 walkable floor 위에 놓이는 blocking object로 구현했다.
- 벽이 있는 셀은 바닥은 남지만 플레이어/적 이동 불가로 처리된다.
- 에디터에서 벽을 찍으면 해당 셀의 출구/입구/스폰/문 데이터는 제거되어 잘못된 배치를 줄인다.
- 검증과 reachability는 벽이 없는 navigable cell 기준으로 동작하도록 변경했다.
- 시야/공격 차단은 두 좌표 사이의 직선 경로 중간 벽을 검사하는 API로 연결했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 벽 배치 맵의 실제 표시, 이동 차단, 적 감지/공격 차단은 아직 확인하지 못했다.
- 공격/시야 차단은 직선 또는 격자상 정렬 가능한 경로의 중간 셀을 기준으로 한다. 복잡한 공격 패턴에서 어떤 셀을 차단 경로로 볼지 추가 규칙이 필요할 수 있다.
- 보스방 문 닫힘에는 런타임에서 벽 셀을 추가/제거하는 API와 시각 갱신 로직이 추가로 필요하다.

## 2026-05-18 무기 획득 시 바닥 무기 제거 및 기존 무기 드롭 처리

### 사용자의 요청 개요
- 무기를 획득할 때 바닥에 있던 무기는 사라지고, 이미 손에 들고 있던 무기가 있다면 그 무기를 바닥에 내려놓도록 수정 요청.

### 핵심 요구사항
- 플레이어가 바닥 무기 칸으로 이동해 무기를 획득하면 해당 바닥 무기 스폰은 맵에서 제거한다.
- 기존 장착 무기가 있으면 획득한 칸에 기존 무기를 새 바닥 무기로 등록한다.
- 장착 무기가 없던 상태라면 바닥 무기만 제거하고 별도 드롭은 만들지 않는다.
- 기존 UI 갱신과 월드 리프레시 흐름에 맞춰 화면에서도 바닥 무기 상태가 반영되도록 한다.

### 이번 작업 범위
- 런타임 맵의 `WeaponSpawns` 목록을 무기 획득 시 직접 갱신하는 API 추가.
- 자동 픽업 로직이 새 API를 사용해 픽업/드롭을 한 번에 처리하도록 변경.
- 픽업 메시지에 기존 무기 드롭 내용을 포함.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Map/MapService.cs`
  - `TryPickupWeaponAt()` 추가. 지정 칸의 바닥 무기를 제거하고, 기존 장착 무기가 있으면 같은 칸에 드롭 무기를 다시 추가한다.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 자동 무기 픽업 로직을 `TryGetWeaponSpawn()` 조회 방식에서 `TryPickupWeaponAt()` 교체 방식으로 변경.
  - 기존 무기를 들고 있던 경우 픽업 메시지에 드롭 안내를 추가.
- `Docs/input.md`
  - 이번 요청, 변경 범위, 빌드 결과, 미확인 사항 기록.

### 실제 수행한 작업 요약
- 바닥 무기 획득 시 `CurrentMap.WeaponSpawns`에서 해당 스폰을 제거하도록 했다.
- 기존에 장착한 무기가 있으면 플레이어가 무기를 집은 위치에 기존 무기를 새 `WeaponSpawnDefinition`으로 추가하도록 했다.
- 무기 아이콘 UI 이벤트는 기존 `WeaponPickedUp` 이벤트를 그대로 사용하고, 월드 표시는 기존 액션 완료 후 `WorldPresenter.Refresh()` 흐름에서 갱신되도록 유지했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 무기 교체 시 바닥 무기 표시가 즉시 바뀌는지, 같은 위치에 기존 무기가 정상 표시되는지는 아직 직접 확인하지 못했다.

## 2026-05-19 보스층 고정 구조 및 보스방 입구 봉쇄 구현

### 사용자의 요청 개요
- 특수한 층에서 시작방/보스방/출구방만 생성되는 보스방 구조를 만들고, 보스방 진입 시 입구를 벽으로 막은 뒤 모든 몬스터가 죽으면 벽이 사라지도록 구현 요청.

### 핵심 요구사항
- 보스층에는 시작방, 보스방, 출구방만 존재한다.
- 방 배치는 위에서부터 출구방, 보스방, 시작방 순서로 고정한다.
- 문 구조는 시작방 위쪽 문, 보스방 아래/위 문, 출구방 아래 문을 사용한다.
- 보스방에 들어가면 입구가 벽으로 막힌다.
- 해당 층에는 보스방 외 몬스터가 없다는 전제이므로, 모든 몬스터가 죽으면 벽이 사라지는 방식으로 처리한다.

### 이번 작업 범위
- `Boss` 특수방 타입을 추가했다.
- `Boss` 특수방이 지정된 층은 일반 랜덤 던전 생성 대신 시작방-보스방-출구방 고정 배치 생성 경로를 사용하도록 변경했다.
- 보스방 진입/클리어 상태와 보스방 입구 봉쇄 셀 정보를 런타임 데이터에 추가했다.
- 기존 벽 시스템을 재사용해 보스방 입구 벽을 런타임에 추가/제거하도록 구현했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunFloorDefinition.cs`
  - `RunSpecialRoomType.Boss` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunState.cs`
  - 보스방 진입 여부와 클리어 여부를 저장하는 `BossRoomEntered`, `BossRoomCleared` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapDefinition.cs`
  - 생성된 보스방 ID와 보스방 입구 봉쇄용 셀 목록을 저장하는 `BossRoomId`, `BossEntranceBlockCells` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 보스방 특수방 템플릿 감지.
  - 보스층 고정 3방 생성 로직 추가.
  - 시작방-보스방 연결 통로의 보스방 입구 쪽 셀을 봉쇄 대상 셀로 기록.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapService.cs`
  - 런타임 중 벽 셀을 추가/제거하는 `SetRuntimeWall()` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 플레이어가 보스방에 처음 진입하면 보스방 입구 봉쇄 벽을 추가.
  - 보스방 진입 후 살아있는 적이 0명이 되면 봉쇄 벽을 제거.

### 실제 수행한 작업 요약
- `SpecialRooms`에 `RoomType == Boss`인 방이 있으면 해당 층은 보스층으로 간주한다.
- 보스층 생성 시 시작방은 원점, 보스방은 시작방 위, 출구방은 보스방 위에 배치한다.
- 세 방은 기존 문/통로 연결 검증을 그대로 사용하므로 각 `MapAsset`에 필요한 방향의 문이 있어야 한다.
- 보스방 아래 문 바로 바깥쪽 통로 셀을 입구 봉쇄 셀로 기록한다.
- 플레이어가 보스방 셀에 처음 들어가면 해당 봉쇄 셀에 런타임 벽을 추가한다.
- 보스방 진입 후 `ActorRepository.GetAliveEnemies()` 결과가 0명이 되면 런타임 벽을 제거한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 보스층 생성 결과, 입구 봉쇄 표시, 몬스터 전멸 후 벽 제거 동작은 아직 직접 확인하지 못했다.
- Unity Inspector에서 보스층으로 사용할 `RunFloorDefinition.SpecialRooms`에 `Boss` 타입 보스방과 `FloorExit` 타입 출구방을 지정해야 한다.
- 시작방/보스방/출구방 `MapAsset`에는 요구된 방향의 문이 있어야 한다. 문이 맞지 않으면 보스층 생성은 일반 던전 생성 fallback으로 돌아간다.
- PowerShell 환경에서 `git` 명령을 찾지 못해 git diff는 확인하지 못했다.

## 2026-05-21 상점 특수방 및 진열대 구매 기본 구현

### 사용자의 요청 개요
- 특수 방 중 하나로 상점을 만들고, 상점 내부에서는 적이 들어오거나 플레이어를 탐지하지 못하게 해 안전한 재화 파밍 악용을 막는 구조를 요청했다.
- 상점에는 여러 진열대가 존재하고, 진열대 앞에 서면 가격/효과를 짧게 보여주며, 방향키로 진열대와 상호작용하면 골드가 충분할 때 아이템을 구매하는 흐름을 요청했다.
- 무기를 이미 장착한 상태에서 무기를 구매하면 새 무기를 장착하고 기존 무기는 바닥에 떨어지도록 요청했다.

### 핵심 요구사항
- `Shop` 특수방 타입을 추가한다.
- 상점 방 셀은 플레이어가 이동할 수 있지만 적은 이동할 수 없어야 한다.
- 플레이어가 상점 방에 들어오면 적 탐지 대상에서 제외되어야 한다.
- 상점 진열대는 가격, 효과 요약, 구매 대상 무기를 데이터로 가진다.
- 플레이어가 진열대에 인접하면 HUD 메시지로 가격/효과를 표시한다.
- 방향키로 진열대를 향해 입력하면 구매를 시도하고, 골드가 충분하면 차감 후 무기를 장착한다.
- 기존 장착 무기가 있으면 플레이어 위치에 바닥 무기로 드롭한다.

### 이번 작업 범위
- 실제 방 형식의 상점 특수방을 우선 구현했다.
- 구매 대상은 현재 아이템 시스템 중 실제 장착/드롭 흐름이 존재하는 무기로 한정했다.
- 상점 진열대 배치는 `MapAsset.ShopOffers`에 수동으로 등록하는 방식으로 추가했다.
- 별도 상점 UI 패널은 만들지 않고 기존 HUD 메시지 라인을 사용해 가격/효과를 표시했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunFloorDefinition.cs`
  - `RunSpecialRoomType.Shop` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/ShopOfferDefinition.cs`
  - 상점 진열대의 위치, 무기, 가격, 효과 요약을 담는 직렬화 데이터 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapAsset.cs`
  - 방 에셋에 상점 진열대 목록 `ShopOffers` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapDefinition.cs`
  - 생성된 맵에 `ShopCells`, `ShopOffers` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 상점 특수방의 셀과 진열대 데이터를 생성된 맵 좌표로 변환하도록 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapService.cs`
  - 적 이동 금지용 `IsEnemyWalkable()`, 플레이어 은닉 판정, 상점 진열대 조회/구매/무기 드롭 API 추가.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 플레이어가 상점 셀에 있으면 적 타겟과 준비 행동을 해제하고, 적 이동은 상점 셀을 제외하도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 진열대 인접 설명, 구매 처리, 골드 차감, 구매 무기 장착, 기존 무기 바닥 드롭 처리 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 상점 진열대를 월드에 무기 아이콘 기반 마커로 표시.
- `Assembly-CSharp.csproj`
  - 새 `ShopOfferDefinition.cs` 컴파일 항목 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/ShopOfferDefinition.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Docs/input.md`
  - 이번 작업 기록 추가.

### 실제 수행한 작업 요약
- `RunSpecialRoomType.Shop` 타입을 추가해 기존 특수방 슬롯 시스템에서 상점 방을 배치할 수 있게 했다.
- `MapAsset.ShopOffers`에 등록된 진열대 데이터를 런타임 `MapDefinition.ShopOffers`로 복사하도록 했다.
- 상점 방으로 배치된 방의 모든 셀을 `ShopCells`로 기록하고, 적 AI 이동 판정에서 해당 셀을 제외했다.
- 적 AI가 타겟 갱신 시 플레이어가 상점 셀에 있으면 탐지하지 않고 기존 준비 공격/이동도 취소하도록 했다.
- 플레이어가 진열대 옆에 서거나 대기하면 `이름: 가격 gold. 효과` 형식 메시지를 보여준다.
- 플레이어가 진열대 방향으로 입력하면 골드를 확인하고, 충분하면 골드 차감 후 무기를 장착하며 기존 무기는 플레이어 위치에 드롭한다.
- 구매가 완료된 진열대는 맵의 `ShopOffers`에서 제거되어 다시 구매할 수 없게 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 상점 특수방 생성, 진열대 표시, 구매 흐름, 골드 차감, 기존 무기 드롭, 적 진입/탐지 차단은 직접 확인하지 못했다.
- Inspector에서 각 층의 `SpecialRooms`에 `Shop` 타입 방 에셋을 추가하고, 해당 `MapAsset.ShopOffers`에 초기 진열대 3개를 배치해야 한다.
- 현재 구매 대상은 무기만 지원한다. 회복, 버프, 소모품 등 다른 아이템을 판매하려면 공통 아이템/효과 구매 처리 구조가 추가로 필요하다.
- 별도 상점 UI 패널은 아직 없고 HUD 메시지로 가격/효과를 표시한다.
- PowerShell 환경에서 `git` 명령을 찾지 못해 git diff/status는 확인하지 못했다.

### 추가 보강
- 상점 특수방 에셋에 적 스폰이 실수로 포함되어 있어도 런타임 맵 생성 시 해당 상점 방의 적 스폰은 등록하지 않도록 보강했다.
- 보강 후 `dotnet build Assembly-CSharp.csproj -nologo`, `dotnet build Assembly-CSharp-Editor.csproj -nologo`를 다시 실행했고 모두 경고 0개, 오류 0개로 성공했다.

## 2026-05-26 타이밍 챌린지 Presenter 프리팹 분리

### 사용자의 요청 개요
- 무기별로 다른 타이밍 UI를 보여줄 수 있도록 `TimingChallengePresenterBase` 프리팹 형태로 Presenter를 관리하는 구조로 변경 요청.
- 코드로 구조를 읽어볼 수 있도록 필요한 부분에 주석을 추가해 달라는 요청.

### 핵심 요구사항
- 무기별 `TimingChallengeDefinition`에서 사용할 Presenter 프리팹을 지정할 수 있어야 한다.
- 기존 단일 입력 타이밍 UI는 별도 Presenter로 분리한다.
- `TimingPopupPresenter`는 특정 UI 구현을 직접 들고 있지 않고, 현재 타이밍 세션에 맞는 Presenter 프리팹을 생성/위임해야 한다.

### 이번 작업 범위
- 타이밍 UI Presentation 구조만 분리했다.
- 타이밍 입력 규칙 자체는 기존 단일 입력 구조를 유지했다.
- 실제 Unity UI 프리팹/에셋 생성 및 Inspector 연결은 이번 작업 범위에 포함하지 않았다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingChallengeDefinition.cs`
  - 타이밍 챌린지별 Presenter 프리팹을 지정할 수 있도록 `TimingChallengePresenterBase presenterPrefab` 참조와 공개 프로퍼티를 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/TimingChallengePresenterBase.cs`
  - 모든 타이밍 Presenter 프리팹이 상속할 기본 클래스 추가.
  - `Show`, `Refresh`, `Hide` 흐름을 공통화하고, 구체적인 화면 갱신은 하위 Presenter가 담당하도록 분리.
- `Assets/Arkeum/Scripts/Presentation/UI/SinglePressTimingChallengePresenter.cs`
  - 기존 `TimingPopupPresenter`에 있던 단일 입력 게이지 UI 표시 로직을 별도 Presenter로 이동.
  - `track`, `goodZone`, `perfectZone`, `marker` 기반 표시를 유지.
- `Assets/Arkeum/Scripts/Presentation/UI/TimingPopupPresenter.cs`
  - 특정 타이밍 UI를 직접 표시하던 구조에서, 세션의 `Definition.PresenterPrefab`을 인스턴스화하고 갱신을 위임하는 호스트 구조로 변경.
- `Assets/Arkeum/Scripts/Presentation/UI/TimingChallengePresenterBase.cs.meta`
  - 신규 Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/SinglePressTimingChallengePresenter.cs.meta`
  - 신규 Unity 스크립트 메타 파일 추가.
- `Assembly-CSharp.csproj`
  - 로컬 `dotnet build`가 신규 스크립트를 포함하도록 Compile 항목 추가.
- `Docs/input.md`
  - 이번 작업 기록 추가.

### 실제 수행한 작업 요약
- `TimingChallengeDefinition`에 Presenter 프리팹 참조를 추가해, 타이밍 규칙 데이터가 사용할 UI 프리팹을 선언할 수 있게 했다.
- `TimingPopupPresenter`는 현재 세션의 Presenter 프리팹을 찾아 생성하고, 이후 매 프레임 `Refresh()`를 위임하도록 변경했다.
- 기존 단일 입력 타이밍 UI는 `SinglePressTimingChallengePresenter`로 분리했다.
- 하위 Presenter가 각자 레이아웃과 위젯을 소유한다는 의도를 주석으로 남겼다.

### 빌드/테스트 여부
- 최초 `dotnet build Assembly-CSharp.csproj -nologo`는 사용자 홈 `.dotnet` sentinel 접근 권한 문제로 실패했다.
- 이후 `$env:DOTNET_CLI_HOME='D:\Unity\Private\.dotnet'; $env:HOME='D:\Unity\Private\.dotnet'; dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 타이밍 팝업 표시와 프리팹 생성 흐름은 직접 확인하지 못했다.
- 각 `TimingChallengeDefinition` 에셋에 실제 `TimingChallengePresenterBase` 기반 프리팹을 Inspector에서 연결해야 한다.
- 기존 무기 에셋의 `timingChallenge`가 비어 있으므로, 무기별 타이밍을 테스트하려면 타이밍 챌린지 에셋과 Presenter 프리팹 연결이 추가로 필요하다.
- 현재 입력/런타임 구조는 여전히 단일 입력 완료 방식이다. 여러 번 누르기나 버튼별 입력을 지원하려면 `ITimingChallengeRuntime`과 입력 전달 구조를 별도로 확장해야 한다.
- PowerShell 환경에서 `git` 명령을 찾지 못해 git diff/status는 확인하지 못했다.

### 추가 정리
- `TimingPopupPresenter`의 `activePresenterPrefab` 필드를 제거했다.
- `activePresenter`는 Inspector 연결 대상이 아니라 런타임에 생성한 Presenter 프리팹 인스턴스 참조로만 유지한다.
- `Show()` 호출 시 기존 Presenter 인스턴스를 제거하고 현재 세션의 `PresenterPrefab`을 새로 생성하도록 단순화했다.
- `Hide()` 호출 시 활성 Presenter 인스턴스를 제거하고 참조를 비우도록 정리했다.
- `$env:DOTNET_CLI_HOME='D:\Unity\Private\.dotnet'; $env:HOME='D:\Unity\Private\.dotnet'; dotnet build Assembly-CSharp.csproj -nologo` 재실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.
