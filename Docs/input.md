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

## 2026-05-25 보스방 진입 시 출구방 방향 봉쇄 추가

### 사용자의 요청 개요
- 보스맵 스테이지에서 보스방에 들어가면 입구방 방향은 닫히는 것을 확인했으며, 출구방 방향도 함께 닫히도록 수정 요청.

### 핵심 요구사항
- 보스방 진입 시 기존 입구 봉쇄와 동일한 타이밍에 출구방으로 이어지는 경로도 벽으로 막는다.
- 보스방 클리어 시 기존 봉쇄 해제 흐름으로 입구/출구 방향 벽이 함께 제거되도록 한다.

### 이번 작업 범위
- 보스층 고정 배치 생성 로직에서 보스방의 입구 문과 출구 문 양쪽에 봉쇄 대상 셀을 등록하도록 수정.
- 런타임 봉쇄/해제 처리 로직은 기존 `BossEntranceBlockCells`와 `SetBossEntranceWalls()` 흐름을 그대로 사용.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 보스방과 출구방 연결 정보를 보존하고, 보스방 출구 문 바깥 셀도 보스방 봉쇄 셀 목록에 추가.
  - 기존 입구 전용 helper를 보스방 문 공통 helper로 변경.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- `CreateBossDungeonMap()`에서 `boss -> exit` 연결의 `DoorConnection`을 받아오도록 변경.
- 보스방 입구는 `bossEntranceConnection.ToDoor`, 보스방 출구는 `bossExitConnection.FromDoor` 기준으로 문 바깥 셀을 `BossEntranceBlockCells`에 등록.
- 보스방 진입 시 기존 `SetBossEntranceWalls(true)`가 두 봉쇄 셀을 모두 벽으로 만들고, 클리어 시 `SetBossEntranceWalls(false)`가 함께 해제하도록 구성.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 보스방 진입 시 입구/출구 방향이 모두 시각적으로 막히는지 직접 확인하지 못했다.
- 보스방 클리어 후 입구/출구 방향 벽이 모두 제거되고 출구방 진입 및 층 이동이 정상 동작하는지 Play Mode 확인이 필요하다.
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

## 2026-05-26 월드 액터 View 재사용 및 이동 애니메이션 기본 구현

### 사용자의 요청 개요
- 현재 월드 표시가 입력 처리마다 액터 프레젠트를 삭제하고 다시 생성하는 방식이라 비효율적이며, 이동 애니메이션과 좌우 flip 처리를 넣기 위한 구조 변경 요청.

### 핵심 요구사항
- 매 입력마다 액터 표시 오브젝트를 삭제/재생성하지 않고 재사용한다.
- 플레이어 이동은 이동 방향으로 부드럽게 보간한다.
- 몬스터 이동은 이동 방향 보간에 더해 y 방향으로 약 0.5 정도 올라갔다 내려오는 기본 점프 느낌을 적용한다.
- 모든 액터 스프라이트는 기본 +x 방향을 바라본다고 보고, -x 이동 시 `SpriteRenderer.flipX`로 좌우를 구분한다.

### 이번 작업 범위
- 월드 표시 구조 중 액터 View 생명주기를 우선 분리했다.
- 바닥 타일은 맵이 바뀔 때만 재생성하도록 분리했다.
- 벽, 무기, 상점, 출구, 적 예고 마커는 상태 변화 반영을 위해 기존처럼 `Refresh()`마다 재생성하되, 액터는 재사용하도록 변경했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/ActorView.cs`
  - 액터 표시용 컴포넌트 추가.
  - 현재 grid 위치, 스프라이트 표시, 좌우 flip, 플레이어/몬스터 이동 애니메이션을 담당.
- `Assets/Arkeum/Scripts/Presentation/World/ActorView.cs.meta`
  - 신규 Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Presentation/World/ProductionViewFactory.cs`
  - 액터 생성 시 `ActorView`를 붙여 반환하도록 변경.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 기존 `spawnedViews` 일괄 삭제 구조를 `floorViews`, `markerViews`, `actorViews`로 분리.
  - 액터는 `ActorEntity.Id` 또는 허브 플레이어 고정 ID 기준으로 재사용하고, 사라진 액터만 제거하도록 변경.
  - 액터 위치 갱신 시 `ActorView.MoveTo()`를 호출해 이동 애니메이션이 실행되도록 변경.
- `Assembly-CSharp.csproj`
  - 신규 `ActorView.cs` 컴파일 항목 추가.
- `Docs/input.md`
  - 이번 작업 기록 추가.

### 실제 수행한 작업 요약
- `WorldPresenter.Refresh()`가 더 이상 모든 월드 표시 오브젝트를 한 번에 삭제하지 않도록 구조를 나눴다.
- 맵 바닥은 `renderedFloorMap` 기준으로 맵 변경 시에만 다시 그린다.
- 동적 마커는 매 refresh마다 재생성해 런타임 벽, 무기 획득, 상점 구매, 예고 마커 표시 변화가 유지되도록 했다.
- 액터 View는 딕셔너리에 보관하며, 살아있는 액터 목록에 없는 View만 제거한다.
- 액터 이동 시 x 방향 이동값을 기준으로 `flipX`를 갱신하고, 상하 이동에서는 기존 좌우 방향을 유지한다.

### 빌드/테스트 여부
- `$env:DOTNET_CLI_HOME='D:\Unity\Private\.dotnet'; $env:HOME='D:\Unity\Private\.dotnet'; dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `$env:DOTNET_CLI_HOME='D:\Unity\Private\.dotnet'; $env:HOME='D:\Unity\Private\.dotnet'; dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 두 빌드 모두 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 이동 애니메이션 체감, 카메라 추적 타이밍, 입력 연속 처리 시 애니메이션 중단/재시작 느낌은 아직 직접 확인하지 못했다.
- 현재 카메라는 액터 애니메이션 완료를 기다리지 않고 논리 위치 기준으로 즉시 이동한다. 필요하면 카메라에도 부드러운 추적 처리가 필요하다.
- 몬스터 타입별 고유 이동 애니메이션은 아직 분기하지 않았고, 모든 몬스터에 기본 점프 이동을 적용했다.
- PowerShell 환경에서 `git` 명령을 찾지 못해 git status/diff는 확인하지 못했다.

## 2026-05-26 플레이어 이동 애니메이션 기반 카메라 추적 수정

### 사용자의 요청 개요
- 플레이어가 이동 애니메이션으로 부드럽게 움직이는데 카메라는 별도로 즉시 움직여 부자연스러우므로, 카메라가 플레이어를 따라 움직이도록 수정 요청.

### 핵심 요구사항
- 카메라가 플레이어의 논리 좌표가 아니라 실제 화면상 플레이어 위치를 따라가야 한다.
- 플레이어 이동 보간 애니메이션 중에도 카메라가 같은 흐름으로 이동해야 한다.
- 기존 월드 표시 및 액터 이동 애니메이션 구조를 크게 바꾸지 않는다.

### 이번 작업 범위
- `WorldPresenter`의 카메라 위치 갱신 방식만 수정했다.
- 플레이어/허브 플레이어 `ActorView`를 카메라 추적 대상으로 지정하고, `LateUpdate()`에서 해당 `Transform` 위치를 따라가도록 변경했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 즉시 좌표 이동 대신 실행 중 플레이어 또는 허브 플레이어 `ActorView`를 카메라 추적 대상으로 설정하도록 변경.
  - `LateUpdate()`에서 추적 대상의 실제 `Transform.position`을 기준으로 카메라 위치를 갱신하도록 추가.
  - `EnsureCamera()`가 매 Refresh마다 카메라를 원점으로 되돌리지 않고, 새로 생성된 카메라 또는 z 위치만 보정하도록 수정.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항을 기록.

### 실제 수행한 작업 요약
- 런 상태에서는 `CurrentRun.Player.Id`에 해당하는 `ActorView`를 카메라 추적 대상으로 연결했다.
- 허브 상태에서는 `"HubPlayer"` `ActorView`를 카메라 추적 대상으로 연결했다.
- 플레이어 이동 애니메이션이 `ActorView`의 실제 `Transform`을 움직이면 카메라가 같은 프레임 흐름에서 해당 위치를 따라가게 했다.
- 추적 대상이 없는 경우에는 fallback 셀 위치로 카메라를 즉시 이동하도록 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 플레이어 이동 시 카메라 추적 감각과 화면 흔들림 여부는 직접 확인하지 못했다.
- 카메라가 플레이어를 정확히 고정 추적하므로, 추후 더 부드러운 지연 추적이 필요하면 damping 값을 둔 보간 방식으로 조정할 수 있다.

## 2026-05-26 공격 방향 기반 액터 flip 반영

### 사용자의 요청 개요
- 플레이어가 공격하는 방향에 맞춰 좌우 flip이 바뀌도록 수정 요청.

### 핵심 요구사항
- 공격 입력 방향이 좌우 방향이면 플레이어 스프라이트의 `flipX`가 해당 방향을 반영해야 한다.
- 이동이 없는 공격 행동에서도 flip이 갱신되어야 한다.
- 타이밍 챌린지 공격 시작 시에도 공격 방향이 즉시 화면에 반영되어야 한다.

### 이번 작업 범위
- 플레이어 공격 판정이 성공한 시점에 `ActorEntity.FacingDirection`을 공격 방향으로 갱신했다.
- `WorldPresenter`가 액터 view 갱신 시 `FacingDirection`을 함께 전달하도록 변경했다.
- `ActorView`가 위치 이동과 별개로 facing 방향만 받아 좌우 flip을 갱신할 수 있게 했다.
- 타이밍 챌린지 시작 직전에 월드 표시를 갱신해 공격 방향 flip이 팝업 전에도 반영되도록 했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 공격 대상이 확인되면 플레이어 `FacingDirection`을 공격 컨텍스트의 `FacingDirection`으로 갱신.
- `Assets/Arkeum/Scripts/Presentation/World/ActorView.cs`
  - `SetFacing()`을 추가해 이동하지 않는 상태에서도 x 방향에 따라 `SpriteRenderer.flipX`를 갱신.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 액터 refresh 시 `ActorEntity.FacingDirection`을 `ActorView`에 전달.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - 타이밍 챌린지 시작 전에 `WorldPresenter.Refresh()`를 호출해 공격 방향 flip을 즉시 반영.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항을 기록.

### 실제 수행한 작업 요약
- 공격 행동은 플레이어 위치가 바뀌지 않아 기존 이동 기반 flip만으로는 방향 전환이 발생하지 않았으므로, facing 기반 flip 갱신 경로를 분리했다.
- 좌우 공격 방향은 `ActorView.SetFacing()`을 통해 즉시 `flipX`에 반영된다.
- 상하 공격 방향은 기존 좌우 facing을 유지한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 공격 입력별 좌우 flip 변화와 타이밍 챌린지 팝업 직전 표시 상태는 직접 확인하지 못했다.

## 2026-05-27 플레이어 범위 공격 다중 대상 적용

### 사용자의 요청 개요
- 플레이어 공격 범위 안에 여러 공격 대상이 있을 때 모든 적이 공격을 받도록 수정 요청.

### 핵심 요구사항
- 무기 공격 범위에 포함된 살아있는 적을 하나만 선택하지 않고 모두 피해 처리한다.
- 기존 무기 오프셋, 방향 회전, 벽에 의한 공격 차단 규칙은 유지한다.
- 타이밍 공격 사용 시에도 같은 타이밍 결과가 범위 내 모든 공격 대상에게 적용되도록 한다.

### 이번 작업 범위
- 플레이어 공격 대상 탐색과 피해 적용 흐름을 단일 대상에서 다중 대상 목록 기반으로 변경.
- 타이밍 공격 세션이 여러 공격 컨텍스트를 보관하고 완료 시 모두 처리하도록 변경.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - `TryGetPlayerAttackTargets()`가 공격 범위 내 모든 적의 `WeaponAttackContext`를 수집하도록 변경.
  - `ResolvePlayerAttacks()`가 수집된 모든 대상에게 피해, 사망 처리, 골드 획득, 턴 소비를 한 번에 처리하도록 추가.
  - 타이밍 공격 완료 시 모든 대상 컨텍스트에 동일한 `TimingAttackResult`를 적용하도록 수정.
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingService.cs`
  - 단일 공격 컨텍스트뿐 아니라 다중 공격 컨텍스트 목록으로 타이밍 세션을 시작할 수 있도록 확장.
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingSession.cs`
  - 타이밍 세션이 `AttackContexts` 목록을 보관하도록 확장.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 무기 공격 오프셋을 끝까지 순회하면서 막히지 않은 각 칸의 적을 모두 공격 대상으로 수집하도록 변경.
- 동일 적이 중복 오프셋에 걸릴 가능성에 대비해 같은 적은 한 번만 추가하도록 방어 로직을 추가.
- 각 대상별로 별도 `WeaponAttackContext`를 사용해 창 같은 오프셋 기반 무기 효과가 기존 규칙대로 대상별 적용되도록 유지.
- 범위 공격 결과 메시지는 다중 명중, 다중 처치, 단일 처치 상황을 구분해 표시하도록 정리.

### 빌드/테스트 여부
- `$env:DOTNET_CLI_HOME='D:\Unity\Private\.dotnet'; $env:HOME='D:\Unity\Private\.dotnet'; dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 무기별 범위 내 다중 적 피격, 타이밍 공격 다중 피격, 처치/골드 메시지 표시를 직접 확인하지 못했다.
- `Assembly-CSharp-Editor.csproj` 빌드는 실행하지 않았다.
## 2026-05-27 플레이어 공격력 계산 구조 정리

### 사용자의 요청 개요
- 플레이어 공격력 변동을 `Stats.AttackPower`에 직접 덮어쓰는 현재 방식이 추후 배수 아이템, 조건부 보정, 버프 확장에 문제가 될 수 있어 구조 정리 요청.

### 핵심 요구사항
- 플레이어의 최종 공격력을 `ActorStats.AttackPower`에 직접 갱신하는 흐름을 줄인다.
- 공격 시점에 현재 런 상태, 무기, 보정값을 기준으로 공격력을 계산하도록 정리한다.
- 기존 공격력 계산 결과는 유지한다.

### 이번 작업 범위
- 플레이어 런 공격력 계산 전용 `RunStatCalculator` 추가.
- 런 시작, 층 이동, 무기 구매, 무기 획득 시 `Player.Stats.AttackPower`를 직접 덮어쓰던 코드 제거.
- 공격 컨텍스트 생성 시 계산기를 통해 공격력을 산출하도록 변경.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunStatCalculator.cs`
  - 플레이어 기본 공격력/방어력과 런 공격력 계산을 모으는 계산기 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunStatCalculator.cs.meta`
  - 신규 Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunState.cs`
  - `EffectiveAttack`이 하드코딩 합산 대신 `RunStatCalculator.CalculatePlayerAttack()`을 사용하도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 공격 컨텍스트의 `AttackPower`를 `RunStatCalculator`에서 계산하도록 변경.
  - 무기 구매/획득 시 `Player.Stats.AttackPower` 직접 갱신 제거.
- `Assets/Arkeum/Scripts/Gameplay/Combat/CombatSystem.cs`
  - 플레이어 공격 컨텍스트가 없는 경우에도 `RunStatCalculator`를 fallback 공격력 계산 경로로 사용하도록 변경.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - 런 시작/층 이동 시 `Player.Stats.AttackPower` 직접 갱신 제거.
  - 플레이어 기본 스탯 생성 시 `RunStatCalculator.CreatePlayerStats()`를 사용하도록 변경.
- `Assembly-CSharp.csproj`
  - 신규 `RunStatCalculator.cs` 컴파일 항목 추가.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 플레이어 최종 공격력은 더 이상 런 시작, 층 이동, 무기 변경 시점에 `Stats.AttackPower`에 캐싱하지 않는다.
- 공격 대상별 `WeaponAttackContext`를 만들 때 현재 `RunState` 기준으로 공격력을 계산한다.
- 기존 계산식인 기본 공격력 3 + 런 공격 보너스 + 장착 무기 공격 보너스는 유지했다.
- 플레이어 방어력 기본값은 `RunStatCalculator.CreatePlayerStats()`에서 1로 초기화하도록 보존했다.

### 빌드/테스트 여부
- `$env:DOTNET_CLI_HOME='D:\Unity\Private\.dotnet'; $env:HOME='D:\Unity\Private\.dotnet'; dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 런 시작, 층 이동, 무기 구매/획득 후 HUD 표시와 실제 피해량이 기존과 동일한지 직접 확인하지 못했다.
- 배수/퍼센트/조건부 스탯 보정 시스템은 아직 구현하지 않았고, 이번 작업은 계산 진입점을 분리하는 1차 정리다.
- `Assembly-CSharp-Editor.csproj` 빌드는 실행하지 않았다.
## 2026-05-27 층 이동 시 플레이어 엔티티 재사용

### 사용자의 요청 개요
- 다음 층으로 이동할 때 플레이어 엔티티를 새로 만들지 않고 기존 플레이어 엔티티를 재사용하는 방식으로 수정 요청.

### 핵심 요구사항
- 새 런 시작 시에는 플레이어 엔티티를 생성한다.
- 층 이동 시에는 기존 `runState.Player`를 새 층 액터 목록에 다시 넣고 위치만 새 층 스폰 지점으로 이동한다.
- 현재 HP 등 플레이어 엔티티가 가진 상태를 층 이동 중 유지한다.

### 이번 작업 범위
- `GameDirector.BuildRunActors()`가 기존 플레이어 엔티티를 선택적으로 받을 수 있도록 변경.
- `TryAdvanceToNextFloor()`에서 기존 `runState.Player`를 `BuildRunActors()`에 전달하도록 변경.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - `BuildRunActors(ActorEntity existingPlayer = null)` 시그니처로 변경.
  - 기존 플레이어가 전달되면 새 플레이어를 만들지 않고 `GridPosition`만 현재 맵의 `PlayerSpawn`으로 갱신한 뒤 액터 목록에 다시 등록.
  - 기존 플레이어가 없을 때만 새 플레이어 엔티티와 기본 스탯을 생성.
  - 층 이동 시 `BuildRunActors(runState.Player)`를 호출하도록 변경.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 새 런 시작 흐름은 기존처럼 `BuildRunActors()`를 호출해 새 플레이어를 만든다.
- 다음 층 이동 흐름은 기존 플레이어 엔티티를 재사용하며, 새 층의 스폰 위치로 이동시킨다.
- 기존 `ActorRepository.SetActors()` 구조는 유지해 현재 층의 적 목록은 새로 만들고, 플레이어만 기존 객체를 다시 등록한다.
- 층 이동 후 HP 복원 흐름은 기존처럼 유지했다.

### 빌드/테스트 여부
- `$env:DOTNET_CLI_HOME='D:\Unity\Private\.dotnet'; $env:HOME='D:\Unity\Private\.dotnet'; dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 층 이동 후 플레이어 HP, 위치, 카메라 추적, HUD 바인딩, 액터 뷰 재사용 상태를 직접 확인하지 못했다.
- `Assembly-CSharp-Editor.csproj` 빌드는 실행하지 않았다.

## 2026-06-09 몬스터 이동 충돌 고정 피해 처리

### 사용자의 요청 개요
- 몬스터가 이동하려는 자리에 플레이어가 있다면 플레이어에게 고정 데미지 1을 주도록 변경 요청.

### 핵심 요구사항
- 몬스터 이동 목적지가 플레이어 위치와 같으면 플레이어에게 피해를 준다.
- 피해량은 방어력 계산을 거치지 않는 고정 데미지 1로 처리한다.
- 몬스터는 플레이어 칸으로 겹쳐 이동하지 않는다.

### 이번 작업 범위
- 몬스터 이동 가능 여부와 이동 실행 로직을 조정했다.
- 전투 시스템에 고정 피해 적용 API를 추가했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 몬스터 이동 목적지가 플레이어 위치일 때 이동 대신 고정 피해 1을 적용하도록 변경.
  - 몬스터 이동 후보에서 플레이어 칸을 무조건 제외하던 조건을 제거해 이동 충돌 처리가 실행될 수 있도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Combat/CombatSystem.cs`
  - 방어력 계산을 거치지 않고 지정 피해량을 그대로 적용하는 `ApplyFixedDamage()` 추가.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 기존에는 `CanMoveTo()`가 플레이어 위치를 제외해 몬스터가 플레이어 칸으로 이동을 시도하는 상황이 실행부까지 도달하지 않았다.
- `CanMoveTo()`는 바닥 이동 가능 여부와 다른 적 점유 여부만 검사하도록 변경했다.
- `ExecutePreparedMove()`에서 목적지가 플레이어 위치인 경우 몬스터 방향만 갱신하고, 플레이어에게 고정 피해 1을 준 뒤 이동 준비 상태를 해제한다.
- 일반 적 공격 로직과 플레이어 공격 로직은 기존 흐름을 유지했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 몬스터가 플레이어 칸을 이동 목적지로 잡는 실제 상황과 HP 감소 표시를 직접 확인하지 못했다.
- `Assembly-CSharp-Editor.csproj` 빌드는 실행하지 않았다.

## 2026-06-09 맵 생성 방식 선택 및 셀룰러 오토마타 생성 경로 추가

### 사용자의 요청 개요
- 기본 맵 생성 기믹을 셀룰러 오토마타 알고리즘 기반으로 바꾸되, 튜토리얼이나 보스방처럼 직접 모양을 정해야 하는 맵을 위해 기존 맵 생성 로직도 남겨달라는 요청.

### 핵심 요구사항
- 일반 층은 셀룰러 오토마타 기반 동굴형 맵 생성 방식을 사용할 수 있어야 한다.
- 기존 `MapAsset` 방 템플릿 기반 방 그래프 생성 로직은 삭제하지 않고 선택 가능한 방식으로 유지한다.
- 튜토리얼 등 완전 수동 맵은 `MapAsset`을 그대로 런타임 맵으로 읽는 경로를 사용할 수 있어야 한다.
- 보스방이 있는 층은 기존 보스층 고정 배치 로직을 계속 사용할 수 있어야 한다.

### 이번 작업 범위
- 층 정의 데이터에 맵 생성 모드와 셀룰러 오토마타 파라미터를 추가했다.
- 런 맵 생성 진입점에서 생성 모드별 분기 경로를 추가했다.
- 셀룰러 오토마타 기반 맵 생성, smoothing, 가장 큰 연결 영역 추출, 플레이어 스폰/출구 위치 선택을 구현했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunFloorDefinition.cs`
  - `RunMapGenerationMode` enum 추가.
  - `RunFloorDefinition.GenerationMode` 추가.
  - `CellularAutomataMapSettings` 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - `CreateRunMap()`에서 생성 모드별로 `FixedMapAsset`, 기존 `RoomGraph`, 신규 `CellularAutomata` 경로를 선택하도록 변경.
  - 보스방 템플릿이 있는 층은 기존 `CreateBossDungeonMap()` 경로를 우선 사용하도록 유지.
  - 셀룰러 오토마타 초기 랜덤 벽 배치, 반복 smoothing, 가장 큰 열린 영역 flood fill, BFS 기반 원거리 출구 선택 로직 추가.

### 실제 수행한 작업 요약
- 기존 `CreateDungeonMap()`, `CreateBossDungeonMap()`, `CreateFallbackDungeonMap()` 로직은 삭제하지 않고 그대로 유지했다.
- `FixedMapAsset` 모드에서는 `RunFloorDefinition.MapAsset`을 직접 `MapDefinition`으로 변환한다.
- `CellularAutomata` 모드에서는 설정 크기 안에서 벽/바닥을 생성하고, 연결된 가장 큰 바닥 영역을 플레이 가능한 공간으로 삼는다.
- 셀룰러 맵은 `MapDefinition.WalkableCells`, `WallCells`, `PlayerSpawn`, `FloorExitPosition`, 단일 `Rooms` 항목을 채워 기존 런타임 조회 구조와 맞췄다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 두 빌드 모두 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 셀룰러 오토마타 맵의 실제 바닥/벽 표시, 이동 가능 영역, 출구 상호작용은 아직 직접 확인하지 못했다.
- 신규 `GenerationMode`와 `CellularAutomataSettings`가 기존 `RunDefinition.asset` 인스펙터에서 원하는 값으로 보이는지 확인이 필요하다.
- 셀룰러 오토마타 맵에는 아직 적/무기/상점 배치 후처리를 넣지 않았다. 일반 층에서 해당 배치를 원하면 별도 스폰 규칙이 필요하다.

## 2026-06-09 셀룰러 오토마타 맵 5x5 구역별 몬스터 랜덤 스폰 추가

### 사용자의 요청 개요
- 셀룰러 방식으로 던전을 생성했을 때 지형을 5x5칸 단위 구역으로 나누고, 구역당 몬스터 1마리를 랜덤 스폰하는 방식으로 변경 요청.

### 핵심 요구사항
- 셀룰러 오토마타 생성 맵에만 구역 단위 몬스터 스폰을 적용한다.
- 5x5 구역마다 유효한 바닥 칸을 찾아 몬스터 1마리를 랜덤 배치한다.
- 기존 방 그래프 기반 맵의 수동/방 템플릿 적 스폰 로직은 유지한다.

### 이번 작업 범위
- 셀룰러 오토마타 설정에 몬스터 스폰 구역 크기와 플레이어 시작점 안전거리 설정을 추가했다.
- 셀룰러 맵 생성 후 가장 큰 연결 영역 기준으로 5x5 구역을 순회하며 적 스폰을 생성하도록 구현했다.
- 적 종류는 기존 층의 `MapAsset` 방 템플릿에 등록된 `EnemySpawns`의 `EnemyDefinition` 목록을 풀로 재사용하도록 했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunFloorDefinition.cs`
  - `CellularAutomataMapSettings.EnemySpawnZoneSize` 추가. 기본값은 5.
  - `CellularAutomataMapSettings.EnemySpawnSafeDistanceFromPlayer` 추가. 기본값은 6.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 셀룰러 맵 생성 후 `AddCellularEnemySpawns()`를 호출하도록 추가.
  - 5x5 구역별 유효 바닥 후보 수집, 적 정의 풀 수집, 랜덤 스폰 생성 로직 추가.

### 실제 수행한 작업 요약
- 각 구역은 `EnemySpawnZoneSize` 기준으로 나누며, 기본값은 요청대로 5x5다.
- 구역 안에서 가장 큰 연결 바닥 영역에 속하고, 플레이어 스폰/출구가 아니며, 플레이어 시작점 안전거리 밖인 칸만 후보로 사용한다.
- 후보가 없는 구역은 스폰하지 않고 건너뛴다.
- 적 정의가 하나도 없으면 셀룰러 적 스폰을 건너뛰고 경고 로그를 남긴다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 두 빌드 모두 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 5x5 구역별 실제 몬스터 배치 밀도와 성능은 아직 직접 확인하지 못했다.
- 200x200 맵에서 5x5 구역마다 1마리를 배치하면 이론상 최대 1600마리까지 생성될 수 있어 실제 플레이 밀도 조정이 필요할 수 있다.
- 현재 적 종류 풀은 방 템플릿에 배치된 기존 적 스폰에서 가져오므로, 해당 층의 `RoomAssets` 또는 시작 `MapAsset`에 적 스폰 샘플이 필요하다.

## 2026-06-09 전장의 안개 표시 시스템 추가

### 사용자의 요청 개요
- 플레이어가 탐사하지 않은 공간은 검은색으로 가리고, 한 번 가본 곳은 회색으로 지형만 보이며 몬스터는 숨기고, 플레이어 주변 5칸은 보이게 하는 전장의 안개 시스템 도입 요청.

### 핵심 요구사항
- 미탐사 셀은 검은색으로 표시한다.
- 탐사했지만 현재 시야 밖인 셀은 회색 안개로 표시하고 지형만 보이게 한다.
- 현재 플레이어 주변 5칸 안은 정상 표시한다.
- 현재 시야 밖 몬스터는 보이지 않아야 한다.

### 이번 작업 범위
- 런 화면 전용 탐사/현재 시야 상태를 `WorldPresenter`에 추가했다.
- 플레이어 위치 기준 맨해튼 거리 5칸을 현재 시야로 계산한다.
- 현재 시야 밖 적, 적 예고 마커, 무기/상점/출구 마커를 숨기도록 표시 필터를 추가했다.
- 200x200 맵에서도 매 refresh마다 안개 오브젝트를 재생성하지 않도록 안개 타일을 맵당 캐싱하도록 구현했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - `exploredCells`, `visibleCells` 상태 추가.
  - 런 refresh 시 현재 시야 갱신, 안개 오버레이 갱신, 시야 밖 액터/마커 숨김 처리 추가.
  - 안개 표시용 `Fog` 루트와 캐시된 fog view/renderer 관리 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldVisualSet.cs`
  - 미탐사/탐사 완료 안개 색상 설정 필드 추가.

### 실제 수행한 작업 요약
- 런 맵에 진입하거나 층이 바뀌면 탐사 상태와 안개 뷰를 초기화한다.
- 플레이어 위치에서 맨해튼 거리 5 이하인 셀을 현재 시야로 보고, 해당 셀을 탐사 완료로 누적한다.
- 미탐사 셀에는 검은 안개, 탐사 완료지만 현재 시야 밖인 셀에는 반투명 회색 안개를 올린다.
- 적은 현재 시야 안에 있을 때만 `ActorView`를 유지하고, 시야 밖이면 제거된다.
- 벽은 지형으로 취급해 안개 아래에 계속 그려지며, 아이템/출구/상점/적 예고 마커는 현재 시야 밖이면 표시하지 않는다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 두 빌드 모두 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 이동에 따라 검은/회색/현재 시야 표시가 자연스럽게 갱신되는지는 아직 직접 확인하지 못했다.
- 현재 시야는 벽에 의한 시야 차단 없이 거리 5칸 기준으로 계산한다. 벽 뒤를 가려야 한다면 별도 line-of-sight 처리가 필요하다.
- 탐사 상태는 현재 런 화면 세션용이며 저장/로드까지 지속되지는 않는다.

## 2026-06-10 타이밍 챌린지 2단계 판정 및 아무 키 입력 처리

### 사용자의 요청 개요
- 타이밍 챌린지 판정을 기존 실패/Good/Perfect 3단계에서 실패/성공 2단계로 단순화하고, 입력 방식을 엔터 전용에서 아무 키 입력으로 변경 요청.

### 핵심 요구사항
- 타이밍 챌린지는 실패와 성공 2가지 결과만 가진다.
- 실패 시 데미지는 0, 성공 시 데미지는 2배로 처리한다.
- 타이밍 판정 중에는 엔터키뿐 아니라 키보드의 아무 키를 눌러도 판정이 완료되어야 한다.

### 이번 작업 범위
- 타이밍 결과 enum, 챌린지 정의, 단일 입력 런타임, UI Presenter, 입력 판정, 타이밍 에셋 값을 수정했다.
- 기존 단검에 연결된 `SinglePressTimingChallenge` 에셋의 데미지 배율과 판정 구간 필드명을 새 구조에 맞게 정리했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingResultGrade.cs`
  - `Good`, `Perfect` 결과를 제거하고 `Success` 결과를 추가.
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingChallengeDefinition.cs`
  - 성공 배율을 2배로 처리하고, 실패 배율을 0으로 처리하도록 변경.
  - 기존 serialized 필드 호환을 위해 `FormerlySerializedAs`를 추가.
- `Assets/Arkeum/Scripts/Gameplay/Timing/ITimingChallengeRuntime.cs`
  - Good/Perfect 구간 대신 Success 구간만 노출하도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Timing/SinglePressTimingChallengeDefinition.cs`
  - 단일 성공 구간 안에서 입력하면 `Success`, 그 외에는 `Failed`를 반환하도록 변경.
- `Assets/Arkeum/Scripts/Presentation/UI/SinglePressTimingChallengePresenter.cs`
  - 성공 구간만 표시하고 기존 Perfect 구간 UI는 숨기도록 변경.
- `Assets/Arkeum/Scripts/Infrastructure/Input/InputReader.cs`
  - 타이밍 판정 중 `Keyboard.current.anyKey.wasPressedThisFrame`도 완료 입력으로 인정하도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 타이밍 결과 메시지 분기를 `Success` 기준으로 변경.
- `Assets/Arkeum/ScriptableObjects/Timing/SinglePressTimingChallenge.asset`
  - 성공 배율 2, 성공 구간 필드로 에셋 값을 갱신.

### 실제 수행한 작업 요약
- 타이밍 판정 모델을 `None / Failed / Success`로 정리했다.
- 실패 결과는 공격력을 0배로 만들어 최종 데미지가 0이 되도록 했다.
- 성공 결과는 공격력을 2배로 만들어 기존 공격 처리 흐름에 전달되도록 했다.
- 기존 Good 구간을 Success 구간으로 사용하고 Perfect 구간은 제거했다.
- 타이밍 판정 상태에서 키보드 아무 키 입력을 판정 완료 입력으로 처리하도록 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 타이밍 UI 표시, 아무 키 입력 판정, 실패 시 0 데미지/성공 시 2배 데미지 체감은 직접 확인하지 못했다.
- 기존 프리팹에는 Perfect 구간 RectTransform 참조가 남아 있을 수 있으나, 런타임에서 해당 오브젝트를 숨기도록 처리했다. 필요하면 프리팹 구조 자체도 추후 정리할 수 있다.

## 2026-06-10 타이밍 실패 데미지 최소 1 적용 문제 수정

### 사용자의 요청 개요
- 타이밍 실패 시 데미지가 0이어야 하는데 실제로 데미지가 들어가는 것 같아, 실패 데미지 0 처리 위치와 원인 확인 요청.

### 핵심 요구사항
- 타이밍 실패 시 실제 최종 데미지가 0이어야 한다.
- 실패 결과가 일반 데미지 계산기의 최소 피해 보정에 의해 1 이상으로 바뀌면 안 된다.

### 이번 작업 범위
- 타이밍 실패 결과가 전투 데미지 계산 흐름에서 어떻게 처리되는지 확인했다.
- 타이밍 실패 공격에 한해 `DamageResolver`의 최소 1 데미지 보정을 우회하도록 수정했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Combat/CombatSystem.cs`
  - `TimingResultGrade.Failed`인 타이밍 공격은 `DamageResolver.ResolveDamage()`로 넘기지 않고 즉시 0 데미지를 반환하도록 변경.

### 실제 수행한 작업 요약
- 실패 판정 자체는 `TimingChallengeDefinition.BuildResult()`에서 `DamageMultiplier = 0f`로 생성되고 있었다.
- 하지만 `CombatSystem.ResolvePlayerAttack()`에서 이 공격력을 다시 `DamageResolver.ResolveDamage()`에 넘기고 있었다.
- `DamageResolver.ResolveDamage()`는 `Mathf.Max(1, attackPower - defense)` 구조라 공격력이 0이어도 최종 피해가 1로 보정되는 상태였다.
- 타이밍 실패 공격은 이 최소 피해 보정을 타지 않도록 별도 분기했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 타이밍 실패 입력 후 적 HP가 줄지 않는지는 직접 확인하지 못했다.

## 2026-06-11 원형 수축 마커 타이밍 챌린지 추가

### 사용자의 요청 개요
- 원형 판과 중심이 같은 도넛 모양 성공 구간이 있고, 마커가 원 바깥에서 중심 방향으로 점점 줄어들 때 성공 구간 안에서 버튼을 누르면 성공하는 새 타이밍 챌린지 추가 요청.

### 핵심 요구사항
- 타이밍 UI는 원형 판을 기준으로 표시한다.
- 성공 구간은 원형 판과 중심이 같은 도넛 모양 영역이다.
- 마커는 원 바깥쪽에서 시작해 중심 방향으로 수축한다.
- 마커 반지름이 성공 구간 안에 있을 때 입력하면 성공, 그 외에는 실패한다.

### 이번 작업 범위
- 원형 수축 마커용 `TimingChallengeDefinition`과 Runtime을 추가했다.
- 원형 판, 도넛형 SuccessZone, 수축 마커를 표시하는 Presenter를 추가했다.
- 바로 연결해 사용할 수 있도록 ScriptableObject 에셋과 Presenter 프리팹을 추가했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Timing/RadialShrinkTimingChallengeDefinition.cs`
  - 마커 반지름이 바깥에서 안쪽으로 줄어드는 타이밍 규칙 추가.
  - `IRadialShrinkTimingChallengeRuntime`을 통해 Presenter가 마커 반지름과 도넛 성공 구간을 읽을 수 있게 했다.
- `Assets/Arkeum/Scripts/Presentation/UI/RadialShrinkTimingChallengePresenter.cs`
  - 런타임에 원형 판, 도넛 성공 구간, 마커 UI를 생성하고 갱신하는 Presenter 추가.
- `Assets/Arkeum/ScriptableObjects/Timing/RadialShrinkTimingChallenge.asset`
  - 새 타이밍 챌린지 에셋 추가.
- `Assets/Arkeum/Prefabs/Timing/RadialShrinkTiming.prefab`
  - 새 Presenter 프리팹 추가.
- `Assembly-CSharp.csproj`
  - 새 스크립트 파일을 컴파일 목록에 추가.
- 각 신규 파일의 `.meta`
  - Unity 에셋 참조를 위한 메타 파일 추가.

### 실제 수행한 작업 요약
- 기본 설정은 `durationSeconds = 1.2`, 마커 시작 반지름 `1.2`, 종료 반지름 `0`, 성공 도넛 반지름 `0.48~0.62`로 구성했다.
- 판정은 `MarkerRadiusNormalized`가 성공 도넛의 inner/outer 반지름 사이에 있으면 `Success`, 아니면 `Failed`를 반환한다.
- Presenter는 별도 UI 참조를 요구하지 않고 프리팹 루트에 붙은 컴포넌트가 런타임에 필요한 UI 오브젝트를 생성한다.
- 새 챌린지는 아직 특정 무기에 연결하지 않았다. 사용하려면 무기 에셋의 `timingChallenge` 필드에 `RadialShrinkTimingChallenge.asset`을 연결해야 한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 원형 UI 표시, 마커 수축 위치, 입력 성공/실패 판정은 직접 확인하지 못했다.
- Unity Editor에서 새 프리팹과 ScriptableObject 참조가 정상 로드되는지 확인이 필요하다.

## 2026-06-11 원형 타이밍 UI 미표시 가능 원인 보완

### 사용자의 요청 개요
- 새 원형 수축 마커 타이밍 챌린지 UI가 보이지 않는다고 문의.

### 핵심 요구사항
- 별도 추가 작업이 필요한지 확인한다.
- UI가 보이지 않을 수 있는 구현상 원인을 보완한다.

### 이번 작업 범위
- 현재 `Dagger.asset`이 `RadialShrinkTimingChallenge.asset`을 바라보고 있어 무기 연결 자체는 되어 있음을 확인했다.
- 원형 UI를 그리는 Graphic 컴포넌트를 private nested 클래스에서 Unity가 안정적으로 인식할 수 있는 독립 컴포넌트 파일로 분리했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/RadialRingGraphic.cs`
  - 도넛/원형 UI 메시를 그리는 독립 `MaskableGraphic` 컴포넌트 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/RadialShrinkTimingChallengePresenter.cs`
  - nested `RingGraphic` 대신 독립 `RadialRingGraphic`을 사용하도록 변경.
- `Assembly-CSharp.csproj`
  - 새 `RadialRingGraphic.cs`를 컴파일 목록에 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/RadialRingGraphic.cs.meta`
  - Unity 에셋 참조를 위한 메타 파일 추가.

### 실제 수행한 작업 요약
- UI 미표시는 별도 무기 연결 누락보다는 런타임에 private nested `MonoBehaviour` 그래픽을 추가하는 구조가 Unity에서 안정적으로 표시되지 않을 가능성이 컸다.
- `RadialRingGraphic`을 별도 파일의 public 컴포넌트로 분리해 Unity 컴포넌트 생성/렌더링 경로를 명확하게 만들었다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 UI 표시 여부는 직접 확인하지 못했다.
- 그래도 보이지 않는다면 `TimingPopupPresenter.presenterRoot`가 활성 Canvas 아래인지, 플레이 중 Console에 Presenter 프리팹/스크립트 참조 누락 경고가 있는지 확인해야 한다.

## 2026-06-11 원형 타이밍 Presenter 수동 UI 연결 방식 변경

### 사용자의 요청 개요
- 원형 타이밍 UI가 여전히 보이지 않아, 런타임 자동 생성 방식 대신 사용자가 직접 UI를 만들고 연결하는 방식으로 스크립트 변경 요청.

### 핵심 요구사항
- Presenter가 UI 오브젝트를 자동 생성하지 않아야 한다.
- Unity에서 직접 만든 원형 판, 성공 도넛, 마커 UI를 Inspector로 연결할 수 있어야 한다.
- 런타임에는 연결된 UI의 성공 구간과 마커 위치만 갱신해야 한다.

### 이번 작업 범위
- `RadialShrinkTimingChallengePresenter`를 수동 UI 참조 기반으로 변경했다.
- 기존 `RadialShrinkTiming.prefab`의 Presenter serialized 필드를 새 구조에 맞게 정리했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/RadialShrinkTimingChallengePresenter.cs`
  - 런타임 UI 생성 코드를 제거.
  - `popupPanel`, `boardRect`, `successZoneRing`, `markerRect`를 Inspector에서 직접 연결하는 방식으로 변경.
  - 연결 누락 시 Console 경고를 출력하도록 변경.
- `Assets/Arkeum/Prefabs/Timing/RadialShrinkTiming.prefab`
  - Presenter 필드를 새 수동 연결 방식에 맞게 갱신.
- `Docs/input.md`
  - 이번 변경과 수동 UI 구성 방식 기록.

### 실제 수행한 작업 요약
- `successZoneRing.SetRadii()`로 도넛 성공 구간만 갱신한다.
- `boardRect.rect`의 짧은 축을 기준으로 반지름을 계산하고, `markerRect`는 중심에 고정한 채 `sizeDelta`로 지름을 갱신한다.
- 프리팹에는 `popupPanel`만 루트로 연결되어 있고, `boardRect`, `successZoneRing`, `markerRect`는 사용자가 UI를 만든 뒤 직접 연결해야 한다.

### 수동 UI 구성 방법
- `RadialShrinkTiming` 프리팹을 연다.
- 루트 아래에 원형 판 역할의 UI 오브젝트를 만든다.
  - 이 오브젝트의 `RectTransform`을 Presenter의 `boardRect`에 연결한다.
- 같은 중심을 갖는 성공 구간 오브젝트를 만든다.
  - 이 오브젝트에 `RadialRingGraphic` 컴포넌트를 붙인다.
  - 해당 컴포넌트를 Presenter의 `successZoneRing`에 연결한다.
- 마커 UI 오브젝트를 만든다.
  - 이 오브젝트의 `RectTransform`을 Presenter의 `markerRect`에 연결한다.
  - 마커는 Presenter가 `boardRect` 중심 기준 위쪽 축에서 반지름만큼 이동시킨다.
- 루트 또는 전체 패널 오브젝트는 Presenter의 `popupPanel`에 연결한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 사용자가 직접 만든 UI 연결 후 실제 표시되는지는 직접 확인하지 못했다.
- `boardRect`, `successZoneRing`, `markerRect` 중 하나라도 비어 있으면 UI는 갱신되지 않고 Console 경고가 출력된다.

## 2026-06-11 원형 타이밍 마커 수축 방식 수정

### 사용자의 요청 개요
- `SuccessZoneRing`이 위에서 아래로 움직이는 것처럼 보이며, 실제 의도는 `MarkerRect`가 원의 중심을 향해 반지름이 줄어드는 방식이라고 피드백.

### 핵심 요구사항
- 성공 구간 도넛은 고정되어야 한다.
- 마커는 위치가 이동하는 것이 아니라 원 중심을 기준으로 반지름이 줄어들어야 한다.

### 이번 작업 범위
- `RadialShrinkTimingChallengePresenter`의 마커 갱신 방식을 위치 이동에서 크기 축소 방식으로 변경했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/RadialShrinkTimingChallengePresenter.cs`
  - `markerRect.anchoredPosition`을 반지름 값으로 이동시키던 방식을 제거.
  - `markerRect.anchoredPosition`은 `Vector2.zero`로 고정하고, `markerRect.sizeDelta`를 반지름에 맞는 지름 크기로 갱신하도록 변경.

### 실제 수행한 작업 요약
- `boardRect`의 짧은 축을 기준으로 최대 반지름을 계산한다.
- 현재 normalized 반지름에 따라 `markerDiameter = normalizedRadius * boardRadius * 2`를 계산한다.
- `markerRect`는 항상 중심에 고정하고, `sizeDelta`만 줄어들게 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 마커가 중심 기준으로 수축하는지는 직접 확인하지 못했다.
- 마커 UI는 중심 기준 크기 변경이 자연스럽게 보이도록 `RectTransform` pivot과 anchors를 중앙 `(0.5, 0.5)`로 두는 것이 좋다.

## 2026-06-11 초침 회전형 부채꼴 타이밍 챌린지 추가

### 사용자의 요청 개요
- 기존 타이밍 기능을 참고해 원형 중심에서 시계 초침처럼 선이 회전하고, 초록 부채꼴 구간에 들어왔을 때 입력하면 성공하는 새 타이밍을 만들고 싶다는 요청.

### 핵심 요구사항
- 원형 중심을 기준으로 회전하는 선형 마커가 있어야 한다.
- 성공 구간은 초록색 부채꼴 영역으로 표시되어야 한다.
- 회전 선의 각도가 성공 부채꼴 구간 안에 있을 때 입력하면 `Success`, 그 외에는 `Failed`로 판정해야 한다.
- 기존 타이밍 챌린지/Presenter/ScriptableObject 구조를 재사용해야 한다.

### 이번 작업 범위
- 각도 기반 타이밍 판정 Runtime과 `TimingChallengeDefinition`을 추가했다.
- 부채꼴 UI를 그리는 전용 `MaskableGraphic`을 추가했다.
- 초침 회전형 Presenter와 기본 프리팹, ScriptableObject 에셋을 추가했다.
- 특정 무기 에셋에는 새 타이밍을 자동 연결하지 않았다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Timing/ClockHandTimingChallengeDefinition.cs`
  - 초침 회전형 타이밍 규칙과 각도 기반 성공 판정 Runtime 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/ClockHandTimingChallengePresenter.cs`
  - 초록 부채꼴 구간과 회전 선 UI 갱신 Presenter 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/RadialSectorGraphic.cs`
  - 중심각/각도 폭/반지름 범위로 부채꼴 또는 원호형 UI 메시를 생성하는 그래픽 컴포넌트 추가.
- `Assets/Arkeum/ScriptableObjects/Timing/ClockHandTimingChallenge.asset`
  - 새 초침 회전형 타이밍 챌린지 기본 에셋 추가.
- `Assets/Arkeum/Prefabs/Timing/ClockHandTiming.prefab`
  - 원형 판, 성공 부채꼴, 회전 선이 연결된 기본 Presenter 프리팹 추가.
- `Assembly-CSharp.csproj`
  - 새 C# 스크립트 3개를 컴파일 항목에 추가.
- 각 신규 파일의 `.meta`
  - Unity 에셋 참조를 위한 GUID 메타 파일 추가.
- `Docs/input.md`
  - 이번 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- `ClockHandTimingChallengeDefinition`은 `startAngleDegrees`, `clockwise`, `rotations`, `successCenterAngleDegrees`, `successSweepAngleDegrees` 설정으로 회전 선의 현재 각도와 성공 구간을 계산한다.
- 입력 시 `Mathf.DeltaAngle()`로 회전 선이 성공 중심각 기준 반각 안에 있는지 판정한다.
- `RadialSectorGraphic`은 초록 부채꼴 성공 영역을 직접 UI 메시로 그린다.
- `ClockHandTimingChallengePresenter`는 성공 부채꼴을 갱신하고, `handRect`를 원 중심에 고정한 채 각도만 회전시킨다.
- 새 `ClockHandTimingChallenge.asset`은 새 `ClockHandTiming.prefab`을 Presenter로 참조한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 회전 방향, 부채꼴 표시 위치, 입력 성공/실패 체감은 직접 확인하지 못했다.
- 새 타이밍을 실제 무기에 사용하려면 원하는 `WeaponDefinition` 에셋의 `timingChallenge` 필드에 `ClockHandTimingChallenge.asset`을 연결해야 한다.
- Unity Editor에서 `ClockHandTiming.prefab`의 원형 판/부채꼴/초침 UI 레이어 순서와 색상은 필요에 따라 조정할 수 있다.

## 2026-06-11 타이밍 성공 구간 랜덤화

### 사용자의 요청 개요
- `SinglePress`, `RadialShrink`, `ClockHand` 타이밍의 고정 성공 구간을 제거하고, 성공 구간 길이와 성공 구간이 등장할 수 있는 범위를 기준으로 매 타이밍마다 랜덤 성공 구간을 만들도록 변경 요청.

### 핵심 요구사항
- `SinglePress`는 기존 `SuccessZoneMin`, `SuccessZoneMax` 설정 대신 성공 구간 길이와 등장 가능 범위를 사용해야 한다.
- `RadialShrink`도 성공 반지름 min/max 대신 성공 구간 길이와 등장 가능 반지름 범위를 사용해야 한다.
- `ClockHand`도 성공 중심각/각도 폭 직접 지정 대신 성공 구간 길이와 등장 가능 각도 범위를 사용해야 한다.
- 성공 구간 판정과 UI 표시는 런타임에 랜덤 결정된 최종 성공 구간을 기준으로 동작해야 한다.

### 이번 작업 범위
- 세 타이밍 `TimingChallengeDefinition`의 serialized 설정 필드를 새 구조로 변경했다.
- 각 Runtime 생성 시 성공 구간 전체가 등장 가능 범위 안에 들어가도록 시작 위치를 랜덤 샘플링하도록 했다.
- 기본 타이밍 ScriptableObject 에셋 3개를 새 필드명과 기본값으로 갱신했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Timing/SinglePressTimingChallengeDefinition.cs`
  - `successZoneMin/Max` 필드를 제거하고 `successZoneLength`, `successZoneSpawnRangeMin/Max` 기반 랜덤 구간 계산 추가.
- `Assets/Arkeum/Scripts/Gameplay/Timing/RadialShrinkTimingChallengeDefinition.cs`
  - `successInner/OuterRadiusNormalized` 필드를 제거하고 `successZoneLengthNormalized`, `successZoneSpawnRangeMin/MaxNormalized` 기반 랜덤 반지름 구간 계산 추가.
- `Assets/Arkeum/Scripts/Gameplay/Timing/ClockHandTimingChallengeDefinition.cs`
  - `successCenterAngleDegrees`, `successSweepAngleDegrees` 직접 설정을 제거하고 `successZoneLengthDegrees`, `successZoneSpawnRangeMin/MaxDegrees` 기반 랜덤 각도 구간 계산 추가.
- `Assets/Arkeum/ScriptableObjects/Timing/SinglePressTimingChallenge.asset`
  - 새 SinglePress 성공 구간 길이/등장 범위 기본값 반영.
- `Assets/Arkeum/ScriptableObjects/Timing/RadialShrinkTimingChallenge.asset`
  - 새 RadialShrink 성공 구간 길이/등장 범위 기본값 반영.
- `Assets/Arkeum/ScriptableObjects/Timing/ClockHandTimingChallenge.asset`
  - 새 ClockHand 성공 구간 길이/등장 범위 기본값 반영.
- `Docs/input.md`
  - 이번 변경 내용, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 성공 구간은 런타임 생성 시 한 번 결정되며, 해당 타이밍 세션 동안 고정된다.
- 등장 가능 범위가 성공 구간 길이보다 좁으면 성공 구간 길이를 등장 가능 범위에 맞게 줄여 비정상 구간이 생기지 않도록 했다.
- Presenter는 기존처럼 Runtime의 최종 `SuccessZoneMin/Max`, 반지름, 각도 값을 읽어 표시하므로 별도 UI 변경 없이 랜덤 구간을 표시한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 여러 번 타이밍을 실행했을 때 성공 구간 위치가 매번 달라지는지는 직접 확인하지 못했다.
- ClockHand의 등장 가능 각도 범위는 현재 0~360 내부의 일반 구간으로 처리하며, 330~30처럼 0도를 가로지르는 wrap 범위는 별도 규칙으로 지원하지 않는다.
## 2026-06-11 셀룰러 맵 상점 마커 텔레포트 기능 추가

### 사용자의 요청 개요
- 셀룰러 방식 맵으로 변경되면서 사라진 상점 기능을 다시 사용할 수 있도록 요청.
- 던전 맵의 랜덤 위치에 상점 마커를 생성하고, 해당 마커를 밟으면 맵 밖에 미리 생성된 상점으로 텔레포트하는 구조를 요구.
- 상점 내부에도 복귀 마커를 두고, 해당 마커를 밟으면 기존 던전 맵의 상점 마커 위치로 돌아오도록 요구.

### 핵심 요구사항
- 셀룰러 런 맵에 상점 입구 마커를 랜덤한 유효 위치에 배치한다.
- 기존 `RunSpecialRoomType.Shop` 상점 에셋을 셀룰러 맵 바깥에 생성한다.
- 던전 상점 마커와 상점 내부 복귀 마커를 서로 텔레포트로 연결한다.
- 상점 내부 진열대 구매 기능은 기존 `ShopOfferDefinition` 기반 흐름을 유지한다.

### 이번 작업 범위
- 셀룰러 맵 생성 경로에만 상점방 배치 및 텔레포트 마커 생성을 추가.
- 플레이어가 마커 칸을 밟은 뒤 텔레포트되도록 런 이동 처리에 연결.
- 월드 표시에서 상점 입구/복귀 마커를 별도 색상으로 렌더링.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Map/MapDefinition.cs`
  - 상점 입구, 상점 내부 입장 위치, 상점 복귀 마커 위치를 저장하는 필드 추가.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 셀룰러 맵 생성 후 `Shop` 특수방 템플릿을 맵 바깥에 배치.
  - 셀룰러 맵의 열린 칸 중 플레이어 시작점, 층 출구, 적 스폰과 겹치지 않는 위치를 상점 입구로 랜덤 선택.
  - 상점 에셋의 `FloorExitPosition`이 있으면 복귀 마커로 사용하고, 없으면 첫 번째 문 위치를 복귀 마커로 사용.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 플레이어 이동 후 상점 입구/복귀 마커 위에 있으면 각각 상점 내부 또는 던전 입구 마커로 텔레포트하도록 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 상점 입구/복귀 마커를 청록색 마커로 표시하도록 추가.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 셀룰러 맵에 상점 입구 마커를 랜덤 배치하고, 상점방은 생성된 셀룰러 맵 오른쪽 바깥에 배치하도록 구현.
- 상점 진열대와 상점 영역(`ShopCells`)은 기존 상점방 배치 흐름을 재사용하도록 연결.
- 던전 상점 입구를 밟으면 상점 기준 위치로 이동하고, 상점 내부 복귀 마커를 밟으면 던전의 기존 상점 입구 마커 위치로 돌아오게 구현.
- 상점 내부 복귀 마커는 상점 에셋에 `FloorExitPosition`이 지정되어 있으면 해당 위치, 없으면 첫 번째 문 위치를 기본값으로 사용.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 셀룰러 맵 생성 후 상점 입구 마커 표시, 상점 입장/퇴장 텔레포트, 상점 구매 UI/메시지 흐름은 직접 확인하지 못했다.
- 현재 `Floor1/Shop.asset`은 `FloorExitPosition`이 `{x: 0, y: 0}`이라 복귀 마커 기본값으로 첫 번째 문 위치가 사용된다. 원하는 상점 내부 마커 위치가 있다면 에셋의 `FloorExitPosition`을 명시적으로 지정하는 것이 좋다.

## 2026-06-23 BGM/SFX 관리 스크립트 추가

### 사용자의 요청 개요
- BGM 및 SFX를 관리할 수 있는 Unity 스크립트 작성 요청.

### 핵심 요구사항
- BGM과 SFX를 구분해서 관리할 수 있어야 한다.
- Inspector에서 오디오 클립을 등록하고 코드에서 재생할 수 있어야 한다.
- BGM 전환, 정지, 일시정지/재개, SFX 재생, 볼륨/뮤트 제어를 제공해야 한다.

### 이번 작업 범위
- 런타임에서 사용할 독립형 오디오 매니저 컴포넌트를 추가했다.
- Unity 스크립트 인식을 위한 `.meta` 파일을 추가했다.
- 로컬 IDE/빌드 확인을 위해 생성형 `Assembly-CSharp.csproj`의 Compile 목록에도 새 스크립트를 포함했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/Audio/AudioManager.cs`
  - BGM/SFX 클립 목록, BGM 페이드 전환, SFX 풀링 재생, 2D/3D SFX 재생, 볼륨/뮤트 제어 API를 제공하는 `AudioManager` 추가.
- `Assets/Arkeum/Scripts/Presentation/Audio.meta`
  - Unity 폴더 메타 파일 추가.
- `Assets/Arkeum/Scripts/Presentation/Audio/AudioManager.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Assembly-CSharp.csproj`
  - `AudioManager.cs`를 로컬 `dotnet build` Compile 목록에 포함. 단, 이 파일은 `.gitignore`의 `*.csproj` 규칙에 의해 저장소 추적 대상이 아니다.
- `Docs/input.md`
  - 이번 작업 요청, 변경 범위, 빌드 결과, 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- `AudioManager`를 `Arkeum.Production.Presentation.Audio` 네임스페이스에 추가했다.
- BGM은 `PlayBgm(string id)`, `PlayBgm(AudioClip)`, `StopBgm()`, `PauseBgm()`, `ResumeBgm()`로 제어할 수 있게 했다.
- SFX는 `PlaySfx(string id)`, `PlaySfxAt(string id, Vector3 position)`, `PlaySfx(AudioClip)`로 재생할 수 있게 했다.
- `SetMasterVolume`, `SetBgmVolume`, `SetSfxVolume`, `SetMuted`, `StopAllSfx`를 제공했다.
- SFX는 지정한 풀 크기만큼 `AudioSource`를 생성해 효과음이 겹쳐 재생될 수 있게 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 오디오 클립 등록, BGM 페이드 체감, SFX 동시 재생, 3D 위치 기반 SFX 동작은 직접 확인하지 못했다.
- 씬에 `AudioManager` GameObject를 배치하고 Inspector에서 BGM/SFX id와 clip을 등록해야 실제 재생에 사용할 수 있다.

## 2026-06-24 오디오 호출 위치 정리 및 액션 피드백 분리

### 사용자의 요청 개요
- 오디오 검토에서 제안했던 "다른 위치가 더 좋아 보이는 부분"에 맞춰 현재 적용된 오디오 호출 구조를 수정해 달라는 요청.

### 핵심 요구사항
- gameplay 계층(`RunController`, `CombatSystem`, `EnemyBehaviorActions`)에서 `AudioManager`를 직접 호출하지 않도록 정리한다.
- 이동음은 실제 플레이어 위치 변경이 발생한 액션 피드백으로 일관되게 처리한다.
- 버튼 사운드 수신기 네임스페이스를 기존 프로젝트 네임스페이스 규칙에 맞춘다.

### 이번 작업 범위
- 오디오 재생을 presentation/core 흐름으로 이동하기 위한 `AudioCueService` 추가.
- 런 액션 결과를 오디오 ID가 아닌 gameplay 피드백 플래그로 전달하기 위한 `RunActionFeedback` 추가.
- `GameDirector`가 액션 피드백과 플레이어 HP 변화를 보고 SFX를 재생하도록 변경.
- 버튼 prefab의 직렬화된 `ButtonSoundReceiver` 타입명을 새 네임스페이스에 맞게 변경.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/Audio/AudioCueService.cs`
  - BGM/SFX ID 매핑을 한 곳에 모아 `GameDirector`가 사용할 오디오 큐 서비스 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunActionFeedback.cs`
  - 플레이어 이동, 공격, 텔레포트 같은 런 액션 피드백 플래그 추가.
- `Assets/Arkeum/Scripts/Core/ServiceRegistry.cs`
  - `AudioCueService`를 서비스로 보관하도록 추가.
- `Assets/Arkeum/Scripts/Core/GameBootstrap.cs`
  - `AudioCueService`를 생성해 `ServiceRegistry`에 주입.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - BGM, 허브 이동음, 런 액션 SFX, 플레이어 피격음을 `AudioCueService`를 통해 재생하도록 변경.
  - 런 액션 전후 플레이어 HP를 비교해 적 공격/충돌 피격음을 상위 흐름에서 재생하도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - `AudioManager` 직접 호출 제거.
  - 일반 공격과 타이밍 공격 모두 `PlayerAttacked` 피드백을 남기도록 변경.
  - 이동 성공 시 `PlayerMoved`, 상점 텔레포트 시 `PlayerTeleported` 피드백을 남기도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Combat/CombatSystem.cs`
  - 적 공격 시 `AudioManager` 직접 호출 제거.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 적 이동 충돌 시 `AudioManager` 직접 호출 제거.
- `Assets/Arkeum/Scripts/Presentation/Audio/ButtonSoundReceiver.cs`
  - 네임스페이스를 `Arkeum.Production.Presentation.Audio`로 변경.
- `Assets/Arkeum/Prefabs/UI/Button.prefab`
  - `ButtonSoundReceiver` 직렬화 타입명을 변경된 네임스페이스로 갱신.
- `Assembly-CSharp.csproj`
  - 로컬 `dotnet build` 검증을 위해 새 C# 파일 2개를 Compile 목록에 추가. 이 파일은 Unity/Rider 생성 파일이며 저장소 추적 대상이 아닐 수 있다.

### 실제 수행한 작업 요약
- 오디오 ID와 실제 재생 호출은 `AudioCueService`에 모으고, gameplay 코드는 `RunActionFeedback`만 남기도록 분리했다.
- 일반 공격에만 있던 공격 피드백을 `ResolvePlayerAttacks()` 내부로 옮겨 타이밍 공격 완료 시에도 같은 공격음이 나도록 했다.
- 플레이어 이동 후 무기 줍기나 상점 정보 표시가 이어져도 이동 피드백은 남도록 위치 변경 직후 `PlayerMoved`를 기록하게 했다.
- 적 공격과 적 이동 충돌 피격음은 `GameDirector`가 액션 전후 HP 감소를 감지해 재생하도록 변경했다.
- 버튼 사운드 수신기 네임스페이스와 prefab 직렬화 참조를 정리했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp.csproj -nologo`는 병렬 실행 중 `obj/Debug/Assembly-CSharp.dll` 파일 잠금으로 1회 실패했으나, 단독 재실행 성공.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 청감, 이동+텔레포트 동시 재생 체감, 타이밍 공격 완료 사운드, 피격음 중복 여부는 직접 확인하지 못했다.
- 현재 새로 추가한 액션 피드백은 기존에 씬에 등록된 `PlayerMove`, `PlayerAttack`, `PlayerHit`, `Teleport` SFX만 사용한다. 무기 줍기, 구매, 보스방 봉인/개방 같은 추가 SFX는 별도 클립 ID 등록 후 확장하면 된다.

## 2026-06-29 StartScene 메인 메뉴 구현

### 사용자의 요청 개요
- `StartScene`을 사용하는 메인 메뉴의 추천 구조를 실제 프로젝트에 구현해 달라는 요청.

### 핵심 요구사항
- 메인 메뉴 UI 표시와 메뉴 동작 제어 책임을 분리한다.
- 새 게임 선택 시 `GameScene`으로 전환한다.
- 설정 화면에서 오디오 볼륨을 조절하고 설정값을 유지한다.
- 실제 저장 기능이 없는 현재 상태에서는 이어하기를 사용할 수 없도록 명확히 표시한다.
- `StartScene`이 빌드의 첫 진입 씬이 되도록 등록한다.

### 이번 작업 범위
- 기존 `StartScene`에 배치된 Start, Load, Setting, Quit 버튼을 재사용했다.
- 메인 메뉴 Presenter와 Controller를 추가하고 Canvas에 연결했다.
- 기존 네 버튼을 설정 화면에서 Master/BGM/SFX 볼륨 조절 및 뒤로 가기 버튼으로 전환하도록 구현했다.
- 씬 전환 페이드와 오디오 설정의 `PlayerPrefs` 저장을 추가했다.
- Build Settings에서 `StartScene`을 `GameScene`보다 앞에 등록했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/MainMenuPresenter.cs`
  - 버튼 검색·라벨·선택 상태, 메인/설정 화면 표시, 씬 전환 페이드를 담당하는 UI Presenter 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/MainMenuPresenter.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Core/MainMenuController.cs`
  - 새 게임, 설정, 종료 흐름과 오디오 볼륨 설정 저장을 담당하는 메뉴 Controller 추가.
- `Assets/Arkeum/Scripts/Core/MainMenuController.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scenes/StartScene.unity`
  - Canvas에 Presenter와 Controller를 연결하고 해상도 대응을 위해 Canvas Scaler를 Scale With Screen Size 방식으로 변경.
- `ProjectSettings/EditorBuildSettings.asset`
  - `StartScene`을 첫 번째 빌드 씬으로 등록.
- `Assembly-CSharp.csproj`
  - 로컬 `dotnet build` 검증을 위해 새 스크립트 2개를 Compile 목록에 추가. 이 파일은 Unity/Rider 생성 파일이며 저장소 추적 대상이 아닐 수 있다.
- `Docs/input.md`
  - 이번 요청, 변경 범위, 빌드 결과와 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 메뉴 UI 책임을 `MainMenuPresenter`, 흐름 제어를 `MainMenuController`로 분리했다.
- New Game 버튼은 짧은 페이드 후 `GameScene`을 비동기로 로드한다.
- Continue 버튼은 저장 프로필 영속화 기능이 아직 없어 `Continue (No Save)`로 표시하고 비활성화했다.
- Settings 버튼을 누르면 동일한 버튼 영역이 Master/BGM/SFX 볼륨 조절 화면으로 전환된다.
- 볼륨은 클릭할 때마다 100%, 75%, 50%, 25%, 0% 순서로 변경되고 `PlayerPrefs`에 보관된다.
- Quit 버튼은 플레이어 빌드에서 애플리케이션을 종료하고 Unity Editor Play Mode에서도 정지하도록 처리했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Editor가 실행 중이어서 별도 배치 모드 씬 로딩 검증은 수행하지 못했다.
- Unity Play Mode에서 버튼 배치, 키보드/게임패드 UI 이동, 페이드, 실제 씬 전환 및 볼륨 청감은 직접 확인하지 못했다.
- 이어하기를 활성화하려면 `SaveProfile` 파일 저장·불러오기 기능과 `GameBootstrap` 프로필 주입 구조를 추가해야 한다.

## 2026-06-29 저장 데이터가 없을 때 Continue 버튼 표시 수정

### 사용자의 요청 개요
- 저장 데이터가 없을 때 Continue 버튼에 `(No Save)` 문구를 붙이지 않고 버튼만 비활성화하도록 요청.

### 핵심 요구사항
- 저장 여부와 관계없이 버튼 라벨은 `Continue`로 유지한다.
- 저장 데이터가 없으면 기존처럼 버튼 입력은 비활성화한다.

### 이번 작업 범위
- 메인 메뉴 Presenter의 Continue 버튼 라벨 결정 로직만 수정했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/MainMenuPresenter.cs`
  - 저장 데이터가 없어도 Continue 버튼 라벨을 변경하지 않도록 수정.
- `Docs/input.md`
  - 이번 변경 내용과 검증 결과 기록.

### 실제 수행한 작업 요약
- `ShowMainMenu(false)` 호출 시 Continue 버튼의 텍스트는 `Continue`로 표시되고 `interactable`만 `false`가 되도록 변경했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 비활성화 색상과 버튼 입력 차단 상태는 직접 확인하지 못했다.

## 2026-06-29 설정 시스템 및 UI 연결 스크립트 구현

### 사용자의 요청 개요
- 사용자가 제작할 설정 UI에 연결할 런타임 스크립트 구현 요청.
- 사운드, 모바일 조작, 그래픽·프레임·화면 효과·배터리 절약·PC 해상도 설정을 요구.

### 핵심 요구사항
- Master/BGM/SFX 볼륨과 모바일 버튼 투명도를 Slider로 제어한다.
- On/Off 및 라디오 버튼 UI는 Unity `Toggle`로 연결한다.
- 모바일 전용 항목과 PC 전용 항목의 표시를 실행 플랫폼에 맞게 전환한다.
- 설정값을 저장하고 다음 실행 및 씬 전환 뒤에도 적용한다.
- UI 표현과 실제 설정 저장·적용 책임을 분리한다.

### 이번 작업 범위
- 전체 설정값을 보관·저장·적용하는 정적 설정 서비스를 추가했다.
- 사용자가 만든 Slider/Toggle UI를 Inspector에서 연결할 설정 메뉴 Binder를 추가했다.
- 모바일 이동 버튼 크기·투명도·좌우 위치를 실제 UI에 적용하는 선택형 Target 컴포넌트를 추가했다.
- 메인 메뉴와 게임 직접 실행 경로에서 저장 설정을 초기화하도록 기존 Bootstrap을 연결했다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Infrastructure/Settings/GameSettingsService.cs`
  - 모든 설정의 `PlayerPrefs` 저장, 조회, 변경 이벤트 및 런타임 적용 API 추가.
- `Assets/Arkeum/Scripts/Infrastructure/Settings/GameSettingsService.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Infrastructure/Settings.meta`
  - Unity 폴더 메타 파일 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/SettingsMenuBinder.cs`
  - Slider, 버튼형 Toggle, 해상도 선택 항목과 설정 서비스를 연결하는 UI Binder 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/SettingsMenuBinder.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/MobileControlSettingsTarget.cs`
  - 모바일 이동 버튼의 크기, 투명도, 좌우 위치를 설정 변경에 맞춰 적용하는 컴포넌트 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/MobileControlSettingsTarget.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Core/MainMenuController.cs`
  - 기존 오디오 설정 저장 코드를 공용 설정 서비스로 통합하고, 선택적으로 `SettingsMenuBinder` 패널을 열 수 있도록 연결.
- `Assets/Arkeum/Scripts/Core/GameBootstrap.cs`
  - GameScene 직접 실행 시에도 저장된 설정이 초기화·적용되도록 연결.
- `Assembly-CSharp.csproj`
  - 로컬 빌드 검증을 위해 새 C# 파일을 Compile 목록에 포함. Unity/Rider 생성 파일이므로 저장소 추적 대상이 아닐 수 있음.
- `Docs/input.md`
  - 이번 요청, 구현 범위, 검증 결과 및 후속 연결 사항 기록.

### 실제 수행한 작업 요약
- 오디오 볼륨, 진동, 모바일 버튼 크기·투명도·위치, 그래픽 품질, 프레임 제한, 화면 흔들림, 배터리 절약, 해상도를 저장하도록 구현했다.
- 그래픽 품질은 프로젝트의 품질 단계 중 첫 단계/중간 단계/마지막 단계를 낮음/보통/높음으로 매핑한다.
- 배터리 절약 모드 활성화 중에는 선택값을 덮어쓰지 않고 실제 적용 품질과 프레임만 낮음/30fps로 제한한다.
- 프레임 제한 적용을 위해 VSync를 비활성화하고 `Application.targetFrameRate`를 설정한다.
- 화면 흔들림은 현재 효과 구현체가 없어 설정값과 변경 이벤트만 제공하며, 흔들림 실행부에서 `GameSettingsService.ScreenShakeEnabled`를 확인하도록 확장할 수 있다.
- 진동은 `GameSettingsService.TryVibrate()` 호출 시 모바일 플랫폼이고 설정이 활성화된 경우에만 실행된다.
- 해상도는 Binder의 각 Resolution Option에 Toggle, width, height를 등록하는 방식으로 구성했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- 사용자가 제작할 설정 UI가 아직 없어 Inspector 참조 연결 및 Play Mode UI 상호작용은 확인하지 못했다.
- Android/iOS 실기기에서 진동, 버튼 배치, 배터리 절약 모드 동작은 확인하지 못했다.
- PC 해상도별 레이아웃과 전체 화면 모드 조합은 확인하지 못했다.
- 실제 화면 흔들림 실행 코드는 현재 프로젝트에서 확인되지 않아 설정값 소비 연결은 후속 작업이 필요하다.

## 2026-07-08 설정 시스템 모바일 우선 범위 축소

### 사용자의 요청 개요
- 설정 기능을 휴대폰 대응부터 구현하고 PC 설정은 추후 확장할 수 있도록 현재 범위를 축소해 달라는 요청.

### 핵심 요구사항
- 모바일에서 필요한 사운드, 진동, 조작 버튼, 그래픽·프레임·화면 효과·배터리 설정은 유지한다.
- PC 전용 해상도 설정과 모바일/PC UI 분기 코드는 현재 구현에서 제거한다.
- 이후 PC 대응 시 설정 서비스를 확장할 수 있는 구조는 유지한다.

### 이번 작업 범위
- 설정 서비스에서 해상도 저장 및 적용 기능 제거.
- 설정 UI Binder에서 해상도 라디오 버튼과 플랫폼 전용 Root 참조 제거.
- 모바일 조작 설정을 Unity Editor에서도 미리 볼 수 있도록 적용 조건 조정.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Infrastructure/Settings/GameSettingsService.cs`
  - PC 해상도 키, 상태, 저장 API와 `Screen.SetResolution` 적용 로직 제거.
- `Assets/Arkeum/Scripts/Presentation/UI/SettingsMenuBinder.cs`
  - 해상도 옵션 직렬화 구조와 모바일/PC 표시 전환 필드 및 로직 제거.
- `Assets/Arkeum/Scripts/Presentation/UI/MobileControlSettingsTarget.cs`
  - 모바일 빌드 외에 Unity Editor에서도 버튼 크기·투명도·좌우 배치를 확인할 수 있도록 변경.
- `Docs/input.md`
  - 이번 범위 축소 내용과 검증 결과 기록.

### 실제 수행한 작업 요약
- 현재 설정 UI에는 Master/BGM/SFX, 진동, 버튼 크기, 버튼 투명도, 이동 버튼 위치, 그래픽 품질, 프레임 제한, 화면 흔들림, 배터리 절약만 연결하면 된다.
- PC 해상도 옵션과 `mobileOnlyRoot`, `pcOnlyRoot` Inspector 필드는 제거했다.
- 기존 설정 저장·변경 이벤트 구조는 유지해 추후 PC 전용 옵션을 별도 확장할 수 있다.

### 빌드/테스트 여부
- 샌드박스 내부 첫 빌드는 로컬 Microsoft SDK 경로 접근 권한 부족으로 실패했다.
- 승인된 `dotnet build Assembly-CSharp.csproj -nologo` 재실행은 성공했다.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- 설정 UI가 아직 연결되지 않아 Unity Play Mode에서 Slider/Toggle 상호작용은 확인하지 못했다.
- Android/iOS 실기기에서 진동, 모바일 조작 버튼 배치, 배터리 절약 적용은 확인하지 못했다.
- 화면 흔들림 실행부의 설정값 소비 연결은 기존과 동일하게 후속 작업이 필요하다.
## 2026-07-11 MainMenuPresenter 설정 버튼 공유 레거시 제거 및 설정 Back 버튼 분리

### 사용자 요청 개요
- `StartScene`에서 별도 설정 패널을 사용하므로 `MainMenuPresenter`에 남아 있던 `showingSettings` 기반 레거시 설정 모드 코드를 제거해달라는 요청.
- 기존 `backRequested` 기능은 메인 메뉴 버튼 재사용 방식이 아니라 설정 패널이 별도 Back 버튼을 받아 사용할 수 있도록 분리 요청.

### 핵심 요구사항
- 메인 메뉴 버튼은 New Game, Continue, Settings, Quit 역할만 담당한다.
- 설정 UI는 별도 패널의 Slider/Toggle/Back Button으로 동작한다.
- 설정 Back 동작은 `SettingsMenuBinder`가 자체 버튼을 받아 콜백으로 처리한다.

### 이번 작업 범위
- `MainMenuPresenter`의 설정 모드 상태와 설정용 버튼 재사용 콜백 제거.
- `MainMenuController`의 메인 메뉴 액션 바인딩 단순화 및 설정 패널 Back 콜백 연결.
- `SettingsMenuBinder`에 Back 버튼 참조와 `BindBack()` 기능 추가.
- `StartScene`의 기존 `Back-Button`을 `SettingsMenuBinder.backButton`에 직렬화 연결.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/MainMenuPresenter.cs`
  - `showingSettings`, `ShowSettings()`, 볼륨 순환 콜백, `backRequested` 등 버튼 공유 레거시 제거.
  - `Bind()`를 순수 메인 메뉴 액션 4개만 받도록 정리.
- `Assets/Arkeum/Scripts/Core/MainMenuController.cs`
  - `MainMenuPresenter.Bind()` 호출 인자 정리.
  - `SettingsMenuBinder.BindBack(ShowMainMenu)` 연결 추가.
  - 설정 패널이 없을 때 경고만 출력하도록 정리.
- `Assets/Arkeum/Scripts/Presentation/UI/SettingsMenuBinder.cs`
  - `backButton` 직렬화 필드, `BindBack(Action)`, Back 버튼 클릭 핸들러 추가.
- `Assets/Arkeum/Scenes/StartScene.unity`
  - 기존 `Back-Button` 프리팹 인스턴스의 `Button` 컴포넌트를 `SettingsMenuBinder.backButton`에 연결.
- `Docs/input.md`
  - 이번 요청, 변경 범위, 검증 결과 기록.

### 실제 수행한 작업 요약
- 메인 메뉴 Presenter에서 설정 화면을 버튼 라벨 전환으로 표현하던 레거시 흐름을 제거했다.
- 설정 패널 Back 동작을 `SettingsMenuBinder` 소유로 분리했다.
- `StartScene`에 이미 존재하는 `Back-Button`을 새 필드에 연결해 Inspector 수동 연결 없이 동작하도록 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 `StartScene` 설정 패널 열기/닫기와 Back 버튼 클릭 동작은 직접 확인하지 못했다.

## 2026-07-11 몬스터 피격 이펙트 출력 시스템 구현

### 사용자의 요청 개요
- 몬스터가 데미지를 입었을 때 화면에 이펙트를 출력할 수 있도록 시스템 구현 요청.

### 핵심 요구사항
- 플레이어 공격으로 몬스터에게 실제 데미지가 들어간 경우에만 피격 이펙트를 출력한다.
- 일반 공격과 타이밍 공격 모두 같은 방식으로 처리한다.
- 이펙트 연출값은 월드 비주얼 설정에서 조정 가능해야 한다.

### 이번 작업 범위
- 런 컨트롤러에서 이번 액션 중 피해를 입은 몬스터 위치를 기록한다.
- 액션 완료 후 월드 프레젠터가 해당 위치에 일회성 피격 이펙트를 생성한다.
- 이펙트는 별도 뷰 컴포넌트가 크기 확대 및 페이드아웃 후 자동 제거한다.
- 기본 스프라이트가 지정되지 않아도 fallback 스프라이트로 출력되도록 처리한다.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 플레이어 공격으로 실제 데미지가 발생한 몬스터 좌표를 `DamagedEnemyCells`로 기록.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - 액션 처리 후 월드 갱신 다음 피격 이펙트를 출력하도록 연결.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 피격 좌표 목록을 받아 화면에 이펙트를 생성하는 API 추가.
- `Assets/Arkeum/Scripts/Presentation/World/ProductionViewFactory.cs`
  - 데미지 이펙트 GameObject 생성 기능 추가.
- `Assets/Arkeum/Scripts/Presentation/World/DamageEffectView.cs`
  - 이펙트 확대/페이드아웃/자동 제거 컴포넌트 추가.
- `Assets/Arkeum/Scripts/Presentation/World/DamageEffectView.cs.meta`
  - Unity 스크립트 메타 파일 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldVisualSet.cs`
  - 몬스터 피격 이펙트 스프라이트, 색상, 지속시간, 시작/종료 스케일 설정 추가.
- `Assets/Arkeum/ScriptableObjects/WorldVisualSet.asset`
  - 새 이펙트 설정값 기본값 명시.
- `Assembly-CSharp.csproj`
  - 로컬 `dotnet build` 검증을 위해 새 스크립트 compile 항목 반영.
- `Docs/input.md`
  - 이번 작업 내용 기록.

### 실제 수행한 작업 요약
- 일반 공격과 타이밍 공격이 공통으로 호출하는 `ResolvePlayerAttacks()`에서 데미지 결과가 0보다 큰 경우 몬스터 위치를 기록하도록 했다.
- `CompleteHandledRunAction()`에서 월드 상태를 먼저 갱신한 뒤 기록된 몬스터 위치에 피격 이펙트를 출력하도록 했다.
- 이펙트 오브젝트는 생성 후 설정된 시간 동안 확대 및 투명도 감소 애니메이션을 수행하고 자동 삭제된다.
- `WorldVisualSet`에 새 설정 필드를 추가해 추후 인스펙터에서 전용 피격 스프라이트를 지정할 수 있도록 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 공격 시 피격 이펙트의 위치, 크기, 지속시간, 시야 밖 몬스터 처리 체감은 직접 확인하지 못했다.
- 현재 `WorldVisualSet.asset`의 `enemyDamageEffectSprite`는 비워 두었으므로, 원하는 전용 히트 스프라이트가 있으면 인스펙터에서 지정해야 한다.

## 2026-07-11 몬스터 피격 이펙트 파티클 시스템 전환

### 사용자의 요청 개요
- 기존 스프라이트 직접 애니메이션 방식이 아니라 Unity `ParticleSystem`을 사용해 몬스터 피격 이펙트를 애니메이션으로 출력하도록 수정 요청.

### 핵심 요구사항
- 몬스터가 실제 데미지를 입었을 때 피격 위치에 파티클 이펙트를 출력한다.
- 이펙트는 런타임에 `ParticleSystem`으로 생성되고 burst 방식으로 재생된다.
- 파티클 수, 수명, 속도, 반경, 크기, 색상, 스프라이트를 `WorldVisualSet`에서 조정할 수 있어야 한다.

### 이번 작업 범위
- 기존 `SpriteRenderer` 확대/페이드아웃 방식의 `DamageEffectView`를 `ParticleSystem` 재생 및 자동 제거 방식으로 변경.
- `ProductionViewFactory.CreateDamageEffect()`가 파티클 시스템을 구성하도록 수정.
- `WorldVisualSet`의 피격 이펙트 설정을 파티클 전용 설정으로 교체.
- `WorldPresenter`가 새 파티클 설정값을 전달하도록 수정.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/DamageEffectView.cs`
  - 파티클 시스템을 재생하고 수명 종료 후 GameObject를 제거하도록 변경.
- `Assets/Arkeum/Scripts/Presentation/World/ProductionViewFactory.cs`
  - `ParticleSystem`, emission burst, circle shape, color over lifetime, size over lifetime, texture sheet sprite 설정을 런타임 구성.
- `Assets/Arkeum/Scripts/Presentation/World/WorldVisualSet.cs`
  - `enemyDamageParticle*` 설정 필드와 프로퍼티 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 파티클 이펙트 생성 시 count/lifetime/speed/radius/size 값을 전달하도록 변경.
- `Assets/Arkeum/ScriptableObjects/WorldVisualSet.asset`
  - 파티클 피격 이펙트 기본 설정값 반영.
- `Docs/input.md`
  - 이번 작업 내용 기록.

### 실제 수행한 작업 요약
- 피격 이펙트 오브젝트가 `SpriteRenderer` 대신 `ParticleSystem`을 가지도록 변경했다.
- 파티클은 일회성 burst로 방출되고 색상 알파가 수명 동안 0으로 줄어든다.
- 크기는 `enemyDamageParticleStartSize`에서 `enemyDamageParticleEndSize`로 변화한다.
- `enemyDamageParticleSprite`가 지정되면 해당 스프라이트를 파티클 텍스처 시트로 사용한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 파티클 크기, 속도, 정렬 순서, 스프라이트 적용 결과는 직접 확인하지 못했다.
- `WorldVisualSet.asset`의 `enemyDamageParticleSprite`는 현재 비워져 있으므로, 전용 파티클 스프라이트가 필요하면 Inspector에서 지정해야 한다.

## 2026-07-11 몬스터 피격 이펙트 스프라이트 프레임 애니메이션 전환

### 사용자의 요청 개요
- 파티클 여러 개를 방출하는 방식이 아니라, 이펙트 오브젝트 1개가 여러 스프라이트 프레임을 순서대로 실행하는 방식으로 다시 제작 요청.

### 핵심 요구사항
- 기존 파티클 시스템 기반 피격 이펙트를 제거한다.
- 피격 이펙트는 `SpriteRenderer` 1개로 표현한다.
- 여러 개의 스프라이트 프레임을 순서대로 재생하고, 재생 완료 후 자동 제거한다.
- 프레임 목록, 재생 속도, 색상, 스케일은 `WorldVisualSet`에서 설정할 수 있어야 한다.

### 이번 작업 범위
- `DamageEffectView`를 `ParticleSystem` 재생 방식에서 스프라이트 프레임 코루틴 재생 방식으로 변경.
- `ProductionViewFactory.CreateDamageEffect()`에서 `ParticleSystem` 구성 코드를 제거하고 `SpriteRenderer` 기반 생성으로 변경.
- `WorldVisualSet`의 `enemyDamageParticle*` 설정을 `enemyDamageEffectFrames`, `enemyDamageEffectFrameRate`, `enemyDamageEffectScale`, `enemyDamageEffectTint`로 교체.
- `WorldPresenter`가 새 프레임 애니메이션 설정값을 전달하도록 변경.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/DamageEffectView.cs`
  - 스프라이트 프레임 배열을 순서대로 재생하고 끝나면 GameObject를 제거하도록 변경.
- `Assets/Arkeum/Scripts/Presentation/World/ProductionViewFactory.cs`
  - 피격 이펙트를 `SpriteRenderer` 1개로 생성하도록 변경하고 파티클 구성 코드 제거.
- `Assets/Arkeum/Scripts/Presentation/World/WorldVisualSet.cs`
  - 피격 이펙트 프레임 배열, 틴트, 프레임레이트, 스케일 설정 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 피격 이펙트 생성 시 프레임 애니메이션 설정을 넘기도록 변경.
- `Assets/Arkeum/ScriptableObjects/WorldVisualSet.asset`
  - 파티클 설정값을 제거하고 프레임 애니메이션 기본 설정값 반영.
- `Docs/input.md`
  - 이번 작업 내용 기록.

### 실제 수행한 작업 요약
- 파티클 시스템 기반 burst, shape, color over lifetime, size over lifetime 설정 코드를 제거했다.
- `DamageEffectView`가 `enemyDamageEffectFrames` 배열을 `enemyDamageEffectFrameRate` 간격으로 순차 표시하도록 했다.
- 프레임 배열이 비어 있으면 fallback 스프라이트를 1프레임만 표시한 뒤 제거하도록 유지했다.
- 기존 데미지 발생 좌표 기록 및 `WorldPresenter.PlayEnemyDamageEffects()` 호출 흐름은 그대로 유지했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 프레임 애니메이션 재생 속도, 위치, 스케일, 정렬 순서는 직접 확인하지 못했다.
- `WorldVisualSet.asset`의 `enemyDamageEffectFrames`는 현재 비어 있으므로, 실제 사용하려면 Inspector에서 hit 애니메이션 스프라이트들을 순서대로 등록해야 한다.

## 2026-07-12 몬스터 피격 이펙트 화면 진동 추가

### 사용자의 요청 개요
- 타격감을 위해 몬스터 피격 이펙트가 출력될 때 화면 진동도 함께 발생하도록 추가 요청.

### 핵심 요구사항
- 몬스터가 실제 데미지를 입어 피격 이펙트가 출력되는 시점에만 화면 진동을 실행한다.
- 기존 설정 시스템의 화면 진동 On/Off 값을 존중한다.
- 진동 시간과 세기는 월드 비주얼 설정에서 조정 가능해야 한다.

### 이번 작업 범위
- `WorldPresenter.PlayEnemyDamageEffects()`에서 실제 이펙트가 하나 이상 생성된 경우 화면 진동을 시작하도록 연결.
- 카메라 추적 로직과 충돌하지 않도록 기본 카메라 위치와 shake offset을 분리.
- `WorldVisualSet`에 피격 화면 진동 지속시간과 세기 설정을 추가.
- `WorldVisualSet.asset`에 기본 화면 진동 값을 반영.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - `GameSettingsService.ScreenShakeEnabled`를 확인해 화면 진동 실행 여부를 결정.
  - 카메라 기본 위치에 `cameraShakeOffset`을 합성하도록 `MoveCameraTo()`를 변경.
  - 피격 화면 진동 코루틴 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldVisualSet.cs`
  - `enemyDamageScreenShakeDuration`, `enemyDamageScreenShakeMagnitude` 설정 추가.
- `Assets/Arkeum/ScriptableObjects/WorldVisualSet.asset`
  - 피격 화면 진동 기본값 `0.12`초, 세기 `0.12` 반영.
- `Docs/input.md`
  - 이번 작업 내용 기록.

### 실제 수행한 작업 요약
- 피격 이펙트가 시야 안에서 실제 생성된 경우에만 화면 진동을 실행하도록 했다.
- 화면 진동 설정이 꺼져 있거나 지속시간/세기가 0 이하이면 실행하지 않도록 했다.
- 카메라 Follow 갱신과 흔들림이 서로 덮어쓰지 않도록 `lastCameraBasePosition + cameraShakeOffset` 방식으로 처리했다.
- 새 피격이 들어오면 기존 흔들림 코루틴을 중단하고 새 흔들림으로 갱신한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 화면 진동 체감, 카메라 추적 중 흔들림 강도, 설정 Toggle 연동은 직접 확인하지 못했다.
- `WorldVisualSet.asset`에서 `Enemy Damage Screen Shake Duration/Magnitude` 값을 플레이 감각에 맞게 조정해야 할 수 있다.

## 2026-07-13 GameScene PauseMenu 및 Settings 연결

### 사용자의 요청 개요
- HUD의 `PauseMenu-Button`을 누르면 게임을 일시정지하고 `PauseMenu-Canvas`를 표시하며, PauseMenu의 `Settings-Button`을 누르면 Settings의 `OptionPanel`을 표시하도록 연결 요청.

### 핵심 요구사항
- Pause 버튼 입력 시 게임 진행을 정지하고 PauseMenu를 연다.
- PauseMenu의 Settings 버튼 입력 시 OptionPanel을 연다.
- 메뉴를 닫거나 Continue를 선택하면 일시정지를 정상적으로 해제한다.

### 이번 작업 범위
- PauseMenu와 Settings 화면 전환을 담당하는 런타임 컨트롤러 추가.
- GameScene의 `GameRoot`에 컨트롤러를 연결하고 Pause/Settings Canvas 참조 지정.
- 기존 `SettingsMenuBinder`의 OptionPanel 열기, 저장, Back 동작 재사용.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/PauseMenuController.cs`
  - Pause/Continue/Settings 버튼 연결, `Time.timeScale` 기반 일시정지, OptionPanel 전환, ESC 입력 처리 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/PauseMenuController.cs.meta`
  - Unity 스크립트 메타데이터 추가.
- `Assets/Arkeum/Scenes/GameScene.unity`
  - `GameRoot`에 `PauseMenuController`를 추가하고 PauseMenu 및 Settings Canvas 참조 연결.
- `Assembly-CSharp.csproj`
  - 로컬 `dotnet build` 검증 대상에 새 스크립트 포함.
- `Docs/input.md`
  - 이번 작업 요청, 구현 내용, 검증 결과 기록.

### 실제 수행한 작업 요약
- 게임 시작 시 PauseMenu와 Settings Canvas를 숨기도록 초기화했다.
- `PauseMenu-Button` 클릭 시 기존 `Time.timeScale`을 보관하고 0으로 설정한 뒤 PauseMenu를 표시한다.
- `Continue-Button` 또는 PauseMenu가 열린 상태의 ESC 입력 시 Canvas를 닫고 기존 `Time.timeScale`을 복원한다.
- `Settings-Button` 클릭 시 PauseMenu를 숨기고 Settings Canvas 및 OptionPanel을 표시한다.
- OptionPanel의 기존 Back 버튼 또는 ESC 입력 시 설정을 저장하고 PauseMenu로 돌아가도록 연결했다.
- 씬 버튼은 기존 오브젝트 이름을 기준으로 탐색하며, 필수 참조가 누락되면 콘솔 오류를 출력하도록 했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 버튼 클릭, 일시정지 중 애니메이션 및 입력 정지, Canvas 표시 순서는 직접 확인하지 못했다.
- PauseMenu의 `Exit-Button`과 `QuitGame-Button` 동작은 이번 요청 범위에 포함되지 않아 연결하지 않았다.

## 2026-07-13 PauseMenu Exit 및 QuitGame 동작 연결

### 사용자의 요청 개요
- PauseMenu의 `Exit-Button`을 누르면 `StartScene`으로 이동하고, `QuitGame-Button`을 누르면 게임을 종료하도록 연결 요청.

### 핵심 요구사항
- Exit 입력 시 일시정지를 해제하고 StartScene을 로드한다.
- QuitGame 입력 시 일시정지를 해제하고 애플리케이션을 종료한다.
- Unity Editor Play Mode에서도 QuitGame 동작을 확인할 수 있어야 한다.

### 이번 작업 범위
- 기존 `PauseMenuController`에서 Exit 및 QuitGame 버튼을 이름으로 찾아 클릭 이벤트 연결.
- 씬 전환 또는 종료 전에 메뉴를 닫고 기존 `Time.timeScale`을 복원하는 공통 처리 추가.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/PauseMenuController.cs`
  - `Exit-Button`, `QuitGame-Button` 참조와 이벤트 연결 추가.
  - `StartScene` 로드 및 애플리케이션 종료 처리 추가.
  - 메뉴 종료와 배속 복원 공통 로직 추가.
- `Docs/input.md`
  - 이번 작업 내용과 검증 결과 기록.

### 실제 수행한 작업 요약
- `Exit-Button` 클릭 시 설정을 저장하고 Pause/Settings Canvas를 닫은 다음 기존 배속을 복원하고 `StartScene`을 로드한다.
- `QuitGame-Button` 클릭 시 같은 정리 작업 후 빌드에서는 `Application.Quit()`을 호출한다.
- Unity Editor에서는 `QuitGame-Button` 클릭 시 Play Mode를 종료하도록 조건부 처리했다.
- 필수 참조 누락 검사에 Exit 및 QuitGame 버튼을 포함했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 최종 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.
- 두 빌드를 처음 병렬 실행했을 때 출력 DLL 파일 잠금으로 런타임 빌드 1회가 실패했으며, 에디터 빌드 완료 후 순차 재실행하여 성공을 확인했다.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 Exit 버튼의 실제 StartScene 전환과 QuitGame 버튼의 Play Mode 종료는 직접 클릭해 확인하지 못했다.
- 플랫폼 빌드에서 `Application.Quit()`의 실제 종료 동작은 확인하지 못했다.

## 2026-07-14 보스 몬스터 3종 패턴 구현

### 사용자의 요청 개요
- 1턴의 공격 준비와 위험 타일 표시를 공통 규칙으로 사용하는 보스 몬스터 구현 요청.
- 공간 절단, 근거리 범위 공격, 직선 돌진의 3개 패턴과 우선순위, 지속시간, 기절 규칙 반영 요청.

### 핵심 요구사항
- 공간 절단은 7턴마다 보스 기준 가로 또는 세로 방향의 보스방 전체에 벽을 만들고 2턴 동안 유지한다.
- 플레이어가 보스 기준 맨해튼 거리 2 이내이면 해당 범위 공격을 1턴 준비한 뒤 실행한다.
- 플레이어가 근거리 밖에서 같은 행 또는 열을 1턴 이상 유지하면 경로를 예고한 뒤 벽 앞까지 돌진한다.
- 돌진 경로에 플레이어가 남아 있으면 플레이어 앞에서 멈춰 피해를 주고, 돌진 후 3턴 동안 기절한다.
- 기절, 준비 중인 패턴, 돌진 등 특수 상태에서는 다른 패턴을 새로 선택하지 않는다.

### 이번 작업 범위
- 기존 일반 몬스터 행동 트리와 분리된 보스 행동 트리 및 보스 패턴 실행 로직 추가.
- 보스 전용 준비 상태, 영향 셀, 임시 벽, 정렬 유지, 기절 상태 추가.
- 보스 패턴 위험 타일 표시와 런타임 벽 생성/해제 연결.
- 보스 정의 에셋 추가 및 2층 보스방의 기존 일반 몬스터 3마리를 보스 1마리로 교체.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyActionType.cs`
  - 공간 절단, 근거리 범위 공격, 직선 돌진 행동 타입 추가.
- `Assets/Arkeum/Scripts/Gameplay/Actors/ActorEntity.cs`
  - 보스 준비 셀, 활성 임시 벽, 턴 주기, 정렬 유지, 기절 지속시간 런타임 상태 추가.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyDefinition.cs`
  - 보스 여부와 공간 절단 주기/지속시간, 근거리 범위, 돌진 기절시간 설정 추가.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 3종 패턴의 선택, 준비, 실행, 피해, 이동, 임시 벽 및 기절 처리 구현.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorTreeFactory.cs`
  - 보스 전용 행동 트리 구성.
- `Assets/Arkeum/Scripts/Gameplay/Combat/EnemyTurnSystem.cs`
  - 보스 정의 여부에 따라 일반/보스 행동 트리를 선택하도록 변경.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 보스 사망 시 남아 있는 공간 절단 임시 벽을 즉시 제거하도록 처리.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 보스 패턴별 영향 셀 전체를 공격 예정 타일로 표시.
- `Assets/Arkeum/ScriptableObjects/Enemies/Boss/BossDefinition.asset`
  - 보스 기본 능력치와 패턴 설정을 가진 신규 보스 정의 추가.
- `Assets/Arkeum/ScriptableObjects/Enemies/Boss.meta`, `Assets/Arkeum/ScriptableObjects/Enemies/Boss/BossDefinition.asset.meta`
  - 신규 Unity 에셋 메타데이터 추가.
- `Assets/Arkeum/ScriptableObjects/MapAssets/Floor2/Boss.asset`
  - 기존 오크/박쥐/스켈레톤 배치를 신규 보스 1마리 배치로 교체.
- `Docs/input.md`
  - 이번 작업 요청, 구현 범위, 검증 결과 및 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 공간 절단은 보스 턴 누적값 기준 7턴마다 우선 선택하고, 보스가 속한 방의 같은 X 또는 Y 좌표 셀을 무작위 방향으로 예고한다.
- 다음 보스 턴에 예고 셀을 런타임 벽으로 전환하며, 생성에 성공한 벽만 추적해 2턴 후 안전하게 제거한다.
- 근거리 공격은 보스방 안에서 맨해튼 거리 2 이하인 셀을 예고하고, 실행 시 플레이어가 예고 범위에 남아 있을 때 보스 공격력으로 피해를 적용한다.
- 직선 돌진은 근거리 밖의 같은 행/열 상태가 연속 확인되면 벽까지의 경로를 예고하고, 실행 시 현재 벽을 다시 검사해 이동한다.
- 돌진 경로에 플레이어가 있으면 해당 셀 직전에서 멈추고 피해를 적용하며, 이후 3번의 보스 턴 동안 이동·공격·패턴 선택을 중단한다.
- 보스 패턴이 준비 중이면 새 패턴을 선택하지 않고 기존 예고 패턴을 먼저 실행하도록 우선순위를 구성했다.
- 신규 보스는 체력 20, 공격력 2, 방어력 1, 처치 보상 10으로 설정하고 기존 오크 스프라이트에 붉은 틴트를 적용했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.
- `git diff --check` 실행 결과 공백 오류 없음. 기존 파일의 LF/CRLF 변환 안내만 출력됨.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 패턴 선택 순서, 위험 타일 표시, 임시 벽 충돌 및 제거, 돌진 위치와 3턴 기절 체감은 직접 확인하지 못했다.
- 보스 전용 스프라이트가 없어 현재는 오크 스프라이트와 붉은 틴트를 사용한다. 전용 아트가 준비되면 `BossDefinition.asset`의 Sprite를 교체해야 한다.
- 체력 20, 공격력 2, 방어력 1, 보상 10은 초기값이므로 실제 플레이 난이도에 맞춘 밸런스 조정이 필요할 수 있다.

## 2026-07-14 보스방 진입 전 보스 행동 정지

### 사용자의 요청 개요
- 플레이어가 보스방에 들어가 임시 봉쇄벽이 생성되기 전까지 보스가 행동하지 않도록 수정 요청.

### 핵심 요구사항
- 보스방 진입 전에는 보스가 이동, 공격, 패턴 준비를 하지 않는다.
- 보스 턴 카운트와 직선 정렬 유지 카운트도 진입 전에는 증가하지 않는다.
- 플레이어 진입으로 임시 봉쇄벽이 생성된 뒤부터 기존 보스 행동을 시작한다.

### 이번 작업 범위
- 적 턴 처리에서 보스방 진입 상태를 확인하는 보스 전용 행동 게이트 추가.
- 일반 몬스터의 기존 행동에는 영향을 주지 않도록 보스 정의에만 조건 적용.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Combat/EnemyTurnSystem.cs`
  - 보스이면서 `RunState.BossRoomEntered`가 false인 경우 행동 트리를 실행하지 않도록 변경.
- `Docs/input.md`
  - 이번 후속 요청과 변경 및 검증 결과 기록.

### 실제 수행한 작업 요약
- 기존 보스방 진입 흐름이 `플레이어 이동 → 임시 봉쇄벽 생성 → BossRoomEntered 설정 → 적 턴` 순서임을 확인했다.
- 보스방 진입 전에는 보스 행동 트리 자체를 호출하지 않으므로 이동, 공격, 위험 타일 준비, 턴 주기 및 기절/벽 지속시간 갱신이 모두 발생하지 않는다.
- 임시벽 생성과 함께 `BossRoomEntered`가 true가 된 첫 적 턴부터 보스가 정상적으로 행동한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 보스방 밖에서 여러 턴을 소비해도 보스가 정지 상태를 유지하는지 직접 확인하지 못했다.
- 보스방 첫 진입 시 임시벽 표시와 같은 턴에 보스의 첫 행동 예고 또는 추적 이동이 시작되는 화면 흐름은 Play Mode 점검이 필요하다.

## 2026-07-14 보스 근거리 공격 범위 및 돌진 조건 변경

### 사용자의 요청 개요
- 보스 근거리 공격을 맨해튼 거리 2 범위에서 보스와 인접한 상하좌우 및 대각선 8칸 공격으로 변경 요청.
- 돌진 패턴의 거리 3 이상 조건을 제거하고 같은 행 또는 열 조건을 중심으로 발동하도록 변경 요청.

### 핵심 요구사항
- 근거리 공격 범위는 보스 기준 8방향 인접 셀로 고정한다.
- 맨해튼 거리 2에 해당하지만 인접하지 않은 셀은 근거리 공격 범위에서 제외한다.
- 돌진 정렬 조건에서 플레이어와 보스 사이의 거리 비교를 제거한다.
- 기존 패턴 우선순위를 유지해 상하좌우 인접 상태에서는 근거리 공격을 먼저 선택한다.

### 이번 작업 범위
- 근거리 공격 선택 조건과 공격 예정 셀 생성 방식을 8방향 인접 판정으로 변경.
- 돌진 정렬 조건에서 기존 `distance > CloseAttackRange` 비교 제거.
- 더 이상 필요하지 않은 근거리 거리 설정을 보스 정의 코드와 에셋에서 제거.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 근거리 공격 조건과 범위를 8방향 인접 판정으로 변경하고 돌진 거리 조건 제거.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyDefinition.cs`
  - 고정된 8칸 범위에서 사용하지 않는 `closeAttackRange` 설정과 프로퍼티 제거.
- `Assets/Arkeum/ScriptableObjects/Enemies/Boss/BossDefinition.asset`
  - 사용하지 않는 `closeAttackRange` 직렬화 값 제거.
- `Docs/input.md`
  - 이번 후속 요청, 변경 범위, 빌드 결과 및 미확인 사항 기록.

### 실제 수행한 작업 요약
- 두 좌표의 X/Y 차이가 각각 1 이하이고 같은 좌표가 아닌지를 검사하는 8방향 인접 판정을 추가했다.
- 보스 주변 상하좌우 4칸과 대각선 4칸만 근거리 공격 예정 타일로 등록한다.
- 돌진 조건은 근거리 공격 우선 범위가 아니면서 플레이어와 보스가 같은 행 또는 열인지로 판정한다.
- 이에 따라 거리 2 이상의 같은 행/열에서도 기존 정렬 유지 규칙을 만족하면 돌진을 준비한다.
- 첫 정렬 감지 턴에는 보스가 추적 이동으로 거리를 좁히지 않고 제자리에서 직선 유지 여부를 확인해, 거리 2에서도 근거리 공격으로 전환되지 않고 돌진을 준비할 수 있게 했다.
- 상하좌우 바로 옆은 같은 행/열이지만 근거리 공격 우선순위가 적용되며, 대각선 바로 옆도 근거리 공격 대상이 된다.

### 빌드/테스트 여부
- 최초 샌드박스 빌드는 로컬 Microsoft SDK 경로 접근 제한으로 실행되지 못했다.
- 승인된 환경에서 `dotnet build Assembly-CSharp.csproj -nologo` 재실행 성공.
- 승인된 환경에서 `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 8개 인접 타일의 위험 표시와 실제 피격 범위는 직접 확인하지 못했다.
- 거리 2인 같은 행/열에서 돌진 준비가 시작되는 타이밍과 근거리 공격 우선순위는 Play Mode 점검이 필요하다.

## 2026-07-14 보스 패턴 선택 우선순위 변경

### 사용자의 요청 개요
- 보스가 새 패턴을 선택할 때 `8칸 근거리 공격 → 돌진 → 공간 절단` 순서로 조건을 확인하도록 변경 요청.

### 핵심 요구사항
- 인접 8칸 공격 조건이 가장 높은 우선순위를 가진다.
- 돌진 조건은 인접 8칸 공격 다음으로 확인한다.
- 공간 절단 주기가 도달했더라도 두 공격 패턴 조건이 충족되면 공간 절단보다 해당 공격을 우선한다.

### 이번 작업 범위
- 보스 행동 선택 분기의 조건 확인 순서 변경.
- 기본 추적 이동 준비 중에도 상위 보스 패턴 조건을 먼저 재평가하도록 순서 조정.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 보스 패턴 조건을 근거리 공격, 돌진, 공간 절단 순으로 재배치.
- `Docs/input.md`
  - 이번 후속 요청과 변경 및 검증 결과 기록.

### 실제 수행한 작업 요약
- 이미 준비된 보스 패턴과 기절 상태는 기존처럼 우선 처리한다.
- 새 패턴 선택 단계에서는 인접 8칸 공격 조건을 가장 먼저 확인한다.
- 직선 정렬이 요구 턴 수를 충족하면 돌진을 두 번째로 준비한다.
- 두 공격 조건이 모두 충족되지 않을 때만 공간 절단의 7턴 주기를 확인한다.
- 첫 직선 정렬 감지 상태에서는 돌진 조건이 아직 완성되지 않았으므로, 공간 절단 주기가 도달했다면 공간 절단을 선택하고 그렇지 않으면 제자리에서 정렬 유지를 기다린다.
- 준비 중인 기본 추적 이동이 있어도 상위 보스 패턴 조건이 충족되면 해당 이동을 덮어쓰고 패턴을 준비한다.

### 빌드/테스트 여부
- 승인된 환경에서 `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 승인된 환경에서 `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 공간 절단 주기와 근거리/돌진 조건이 동시에 충족될 때 실제로 요청한 우선순위대로 표시되는지는 직접 확인하지 못했다.

## 2026-07-14 보스 이동 준비 상태 우선 처리

### 사용자의 요청 개요
- 보스가 이동 준비 상태일 때 새 패턴 조건이 충족되더라도 무시하고 다음 턴에 준비된 이동을 실행하도록 변경 요청.

### 핵심 요구사항
- 이동 준비가 시작된 뒤에는 근거리 공격, 돌진, 공간 절단 조건이 이동을 취소하지 않는다.
- 다음 보스 턴에 준비된 목표 위치로 이동한다.
- 이동 완료 후 다음 턴부터 기존 패턴 우선순위를 다시 확인한다.

### 이번 작업 범위
- 보스 행동 처리에서 준비된 이동 실행을 새 패턴 조건 검사보다 앞에 배치.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 이동 준비 상태를 먼저 처리하여 새 패턴이 준비된 이동을 덮어쓰지 못하도록 변경.
- `Docs/input.md`
  - 이번 후속 요청과 변경 및 검증 결과 기록.

### 실제 수행한 작업 요약
- 보스 처리 순서를 `기절 → 준비된 보스 패턴 실행 → 준비된 이동 실행 → 새 패턴 선택`으로 구성했다.
- `WanderMove` 또는 `ChaseMove` 준비 상태이면 플레이어 위치로 인해 새 패턴 조건이 생겨도 조건 계산 전에 `MoveToPreparedTarget()`을 실행한다.
- 이동 실행 과정에서 기존 준비 상태가 해제되며, 그다음 보스 턴부터 `8칸 공격 → 돌진 → 공간 절단` 순서로 새 패턴을 선택한다.

### 빌드/테스트 여부
- 승인된 환경에서 `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 승인된 환경에서 `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 이동 예고 중 플레이어가 인접하거나 직선상에 진입했을 때 실제로 이동 예고가 유지되고 다음 턴 이동하는지는 직접 확인하지 못했다.

## 2026-07-22 GameScene Result-Canvas 결과창 연결

### 사용자의 요청 개요
- `GameScene`의 `Result-Canvas`를 플레이어 사망 또는 최종 층 클리어 시 결과창으로 사용하도록 연결 스크립트 구현 요청.

### 핵심 요구사항
- 일반 플레이 중에는 `Result-Canvas`를 숨긴다.
- 플레이어 사망 시 패배 결과를, 마지막 층 클리어 시 클리어 결과를 표시한다.
- 기존 결과창의 제목, 턴 수, 최고 도달 층, 총 골드 UI를 실제 런 데이터와 연결한다.
- 결과창 버튼 또는 확인 입력으로 허브에 복귀할 수 있게 한다.

### 이번 작업 범위
- 결과 화면 프레젠터 추가 및 `GameBootstrap`/`ServiceRegistry`/`GameDirector` 연결.
- 실제 플레이어 행동 턴 수 누적 데이터 추가.
- `GameScene`의 기존 `Result-Canvas`와 자식 UI 참조 연결.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/ResultScreenPresenter.cs`
  - 결과창 표시/숨김, 결과 텍스트 갱신, 허브 복귀 버튼 처리.
- `Assets/Arkeum/Scripts/Presentation/UI/ResultScreenPresenter.cs.meta`
  - 신규 Unity 스크립트 에셋 메타데이터.
- `Assets/Arkeum/Scripts/Core/GameBootstrap.cs`
  - 결과 화면 프레젠터 초기화 및 서비스 등록.
- `Assets/Arkeum/Scripts/Core/ServiceRegistry.cs`
  - 결과 화면 프레젠터 서비스 제공.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - 허브/런 시작 시 결과창 숨김, 사망/최종 클리어 시 결과창 표시.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunState.cs`
  - 런 단위 플레이어 행동 턴 수 저장 필드 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/TurnSystem.cs`
  - 플레이어 행동 소비 시 턴 수 누적.
- `Assets/Arkeum/Scenes/GameScene.unity`
  - `GameRoot`에 결과 화면 프레젠터를 추가하고 기존 `Result-Canvas` UI 참조 연결.
  - 결과창 루트 스케일을 정상화하고 HUD 위에 표시되도록 캔버스 정렬 순서 조정.
- `Docs/input.md`
  - 이번 요청, 구현 범위, 검증 결과 및 후속 점검 사항 기록.

### 실제 수행한 작업 요약
- 사망 결과는 `DEFEAT`, 마지막 층 클리어 결과는 `CLEAR`로 표시한다.
- 결과창에 플레이어 행동 턴 수, 프로필 최고 도달 층, 현재 총 골드를 표시한다.
- 기존 결과 버튼을 누르면 허브로 복귀하며, 기존 확인 키 복귀 동작도 유지한다.
- 다음 층이 존재하는 중간 층 클리어에서는 결과창을 띄우지 않고 기존처럼 다음 층으로 진행한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 사망 및 마지막 층 클리어 시 결과창의 시각적 배치와 버튼 클릭 동작은 직접 확인하지 못했다.
- 기존 UI의 `TotalScore` 오브젝트에는 프로젝트에 별도 점수 시스템이 없어 임의 점수 대신 `Total Gold`를 표시하도록 연결했다.

## 2026-07-22 Result-Canvas 현재 층 표시 변경

### 사용자의 요청 개요
- 결과창의 `HighFloor` 항목에 프로필 최고 기록이 아닌 플레이어가 죽은 현재 층을 표시하도록 수정 요청.

### 핵심 요구사항
- 런 종료 시 `RunState.CurrentFloor` 값을 결과창에 표시한다.

### 이번 작업 범위
- 결과 화면의 층 표시 데이터와 라벨 변경.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/ResultScreenPresenter.cs`
  - `HighFloor` UI가 프로필 최고 층 대신 현재 런 종료 층을 표시하도록 변경.
- `Docs/input.md`
  - 이번 후속 요청과 변경 및 검증 결과 기록.

### 실제 수행한 작업 요약
- 기존 `Highest Floor: {profile.HighestFloor}` 표시를 `Current Floor: {runState.CurrentFloor}`로 변경했다.
- 사망 시 플레이어가 사망한 층이 그대로 표시되며, 최종 클리어 시에도 런이 종료된 현재 층을 표시한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 사망 결과창에 현재 층 숫자가 표시되는지는 직접 확인하지 못했다.

## 2026-07-22 보스방 진입 시 전장 안개 제거

### 사용자의 요청 개요
- 플레이어가 보스방에 들어가면 전장의 안개를 모두 제거하도록 수정 요청.

### 핵심 요구사항
- `BossRoomEntered` 상태가 된 순간 현재 층의 모든 이동 가능 타일을 공개한다.
- 보스방 진입 후 해당 층이 끝날 때까지 안개가 다시 표시되지 않게 한다.

### 이번 작업 범위
- 런 안개 가시성 계산에 보스방 진입 상태 처리 추가.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 보스방 진입 후 현재 맵 전체를 가시·탐색 완료 상태로 처리.
- `Docs/input.md`
  - 이번 요청과 변경 및 검증 결과 기록.

### 실제 수행한 작업 요약
- `UpdateRunFog()`에서 `CurrentRun.BossRoomEntered`를 확인한다.
- 보스방에 진입했다면 `CurrentMap.WalkableCells` 전체를 `visibleCells`와 `exploredCells`에 추가하고 일반 시야 거리 계산을 생략한다.
- 기존 보스방 진입 처리 직후 호출되는 `WorldPresenter.Refresh()`에서 전장 안개가 제거된다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 보스방 첫 진입 직후 전장 전체 안개가 실제로 사라지는지는 직접 확인하지 못했다.

## 2026-07-22 3슬롯 세이브/로드 기능 구현

### 사용자의 요청 개요
- 게임 진행 상태를 저장하고 불러오는 기능을 구현하고, 저장 슬롯은 3개로 구성 요청.
- 아직 로드 슬롯 버튼 3개는 없으므로 추후 Unity Inspector에서 연결할 수 있는 공개 스크립트 API 제공 요청.

### 핵심 요구사항
- 1~3번 슬롯을 각각 독립된 저장 파일로 관리한다.
- 프로필 진행도와 현재 런의 맵, 액터, 장비 및 전투 상태를 함께 복원한다.
- 추후 저장/로드 버튼의 `OnClick(int)`에 슬롯 번호를 전달해 연결할 수 있어야 한다.
- 빈 슬롯 여부와 슬롯 표시용 메타데이터를 조회할 수 있어야 한다.

### 이번 작업 범위
- JSON 기반 3슬롯 저장소와 저장 데이터 모델 추가.
- 게임 런타임 상태 캡처 및 복원 로직 추가.
- 메인 메뉴에서 최근 슬롯 계속하기 및 지정 슬롯 로드 진입 흐름 연결.
- `GameDirector`에 슬롯 저장/로드 공개 API 추가.
- 실제 슬롯 버튼 및 슬롯 선택 UI 생성은 제외.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Infrastructure/Persistence/SaveGameData.cs`
  - 프로필, 런, 맵, 액터, 무기/상점/적 배치와 슬롯 메타데이터 DTO 정의.
- `Assets/Arkeum/Scripts/Infrastructure/Persistence/SaveGameService.cs`
  - 3슬롯 JSON 저장/로드/삭제/조회, 런 상태 캡처 및 복원 구현.
- `Assets/Arkeum/Scripts/Infrastructure/Persistence.meta`
- `Assets/Arkeum/Scripts/Infrastructure/Persistence/SaveGameData.cs.meta`
- `Assets/Arkeum/Scripts/Infrastructure/Persistence/SaveGameService.cs.meta`
  - 신규 Unity 폴더 및 스크립트 에셋 메타데이터.
- `Assets/Arkeum/Scripts/Core/ServiceRegistry.cs`
  - `SaveGameService` 등록 및 제공.
- `Assets/Arkeum/Scripts/Core/GameBootstrap.cs`
  - 메인 메뉴의 슬롯 로드 요청을 소비하고 저장 프로필/런을 복원하여 게임 시작.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - `SaveToSlot(int)`, `TrySaveToSlot(int, out string)`, `LoadFromSlot(int)` 공개 API 및 런 복원 흐름 추가.
- `Assets/Arkeum/Scripts/Core/MainMenuController.cs`
  - `LoadGameFromSlot(int)`, `HasSaveSlot(int)`, `GetSaveSlotMetadata(int)` 공개 API와 최근 슬롯 Continue 연결.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapService.cs`
  - 저장된 맵을 현재 런 맵으로 주입하는 복원 API 추가.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 저장 파일은 `Application.persistentDataPath/Saves/slot_1.json`부터 `slot_3.json`까지 생성한다.
- 임시 파일에 JSON을 먼저 기록한 후 슬롯 파일로 교체해 기록 도중 손상 가능성을 줄였다.
- 프로필 골드/퀘스트 진행도, 현재 층/턴/장비, 생성된 맵 구조와 런타임 벽, 남은 무기/상점 상품, 플레이어와 적의 HP·위치·행동 준비·보스 상태를 저장한다.
- 로드 시 저장된 에셋 ID로 무기와 적 정의를 다시 연결하고 월드/HUD/상호작용을 갱신한다.
- 허브 또는 런의 액션 사이에서만 저장할 수 있고, 타이밍 액션/결과 처리 같은 중간 상태에서는 저장을 거부한다.
- 추후 저장 버튼에는 `GameDirector.SaveToSlot(1~3)`, 로드 버튼에는 `MainMenuController.LoadGameFromSlot(1~3)`을 연결할 수 있다.
- 기존 Continue 버튼은 저장된 슬롯 중 가장 최근 슬롯을 불러오며, 저장 파일이 없으면 비활성화된다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 슬롯 저장 후 앱 재실행, 같은 맵/액터/장비 상태 복원까지의 통합 동작은 직접 확인하지 못했다.
- 저장/로드 슬롯 버튼 3개와 슬롯 정보 텍스트 UI는 아직 생성·연결하지 않았다.
- 저장 데이터는 현재 버전 1이며, 이후 데이터 구조 변경 시 버전 마이그레이션 로직을 추가해야 한다.

## 2026-07-22 GameScene 모바일 이동/타이밍 버튼 연결

### 사용자의 요청 개요
- `GameScene`에 새로 만든 `Timing-Button`과 `MoveButton` 방향 버튼들을 각각 기존 게임 기능에 연결 요청.

### 핵심 요구사항
- 상/하/좌/우 버튼으로 키보드 이동과 동일한 허브 및 런 이동을 수행한다.
- `Timing-Button`은 런 진행 중 타이밍 모드를 전환한다.
- 타이밍 챌린지 진행 중에는 같은 `Timing-Button`을 타이밍 판정 입력으로 사용한다.
- 씬의 버튼 참조를 추후 별도로 Inspector에서 연결하지 않아도 동작하게 한다.

### 이번 작업 범위
- 기존 `InputReader`에 UI 입력 큐 추가.
- 버튼 이름을 기준으로 자동 탐색하고 입력 큐에 전달하는 모바일 컨트롤 브리지 추가.
- `GameBootstrap`에서 모바일 컨트롤 브리지를 자동 생성·초기화하도록 연결.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Infrastructure/Input/InputReader.cs`
  - UI 방향 이동, 타이밍 모드 전환, 타이밍 판정 입력 큐 추가.
- `Assets/Arkeum/Scripts/Presentation/UI/MobileGameplayControls.cs`
  - `Timing-Button`, `Up-Button`, `Down-Button`, `Left-Button`, `Right-Button` 자동 탐색 및 클릭 이벤트 연결.
- `Assets/Arkeum/Scripts/Presentation/UI/MobileGameplayControls.cs.meta`
  - 신규 Unity 스크립트 에셋 메타데이터.
- `Assets/Arkeum/Scripts/Core/GameBootstrap.cs`
  - 모바일 컨트롤 컴포넌트를 자동 생성하고 `GameDirector`와 `InputReader` 주입.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 방향 버튼 클릭을 기존 키보드 이동과 같은 `InputReader.TryGetMoveDirection()` 흐름으로 전달한다.
- 이동 버튼은 허브와 일반 런 상태에서만 활성화되고, 타이밍 챌린지 및 결과 처리 중에는 비활성화된다.
- `Timing-Button`은 일반 런에서 타이밍 모드 ON/OFF를 요청하고, 타이밍 챌린지에서는 현재 판정 입력을 요청한다.
- 기존 `Timing-Button`의 비활성화된 루트 이미지는 화면에 표시되지 않는 투명 레이캐스트 영역으로 활성화해 클릭을 받을 수 있게 했다.
- 버튼은 이름으로 자동 탐색하므로 씬의 `OnClick` 수동 등록 없이 동작한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 각 버튼을 직접 클릭해 이동 방향, 타이밍 모드 전환, 타이밍 챌린지 판정까지의 실제 UI 입력은 확인하지 못했다.
- 버튼 이름이 변경되면 `MobileGameplayControls`의 자동 탐색 이름도 함께 변경하거나 Inspector 참조를 직접 지정해야 한다.

## 2026-07-22 타이밍 챌린지 전체 게임 버튼 판정 입력

### 사용자의 요청 개요
- 타이밍 챌린지 중에는 특정 타이밍 버튼뿐 아니라 어떤 게임 조작 버튼을 눌러도 판정 입력이 되도록 수정 요청.

### 핵심 요구사항
- 타이밍 챌린지 중 상/하/좌/우 방향 버튼과 `Timing-Button`을 모두 판정 입력으로 처리한다.
- 방향 버튼을 눌렀을 때 플레이어가 이동하지 않아야 한다.
- 기존 키보드 아무 키 판정 입력은 유지한다.

### 이번 작업 범위
- 모바일 방향 버튼의 타이밍 챌린지 상태 처리와 활성 조건 변경.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/MobileGameplayControls.cs`
  - 타이밍 챌린지 중 방향 버튼 입력을 이동 대신 타이밍 판정으로 전달하고 버튼 활성 상태 유지.
- `Docs/input.md`
  - 이번 후속 요청과 변경 및 검증 결과 기록.

### 실제 수행한 작업 요약
- `QueueMove()`에서 현재 상태가 `TimingChallenge`이면 방향을 사용하지 않고 `QueueTimingAction()`을 호출한다.
- 타이밍 챌린지 중에도 네 방향 버튼을 누를 수 있도록 활성 조건을 확장했다.
- 따라서 `Timing-Button`과 네 방향 버튼 중 어느 것을 눌러도 동일한 타이밍 판정이 한 번 실행된다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 각 방향 버튼과 타이밍 버튼을 눌러 판정이 한 번씩 실행되는지는 직접 확인하지 못했다.

## 2026-07-22 타이밍 판정 후 챌린지 재시작 중복 입력 수정

### 사용자의 요청 개요
- 타이밍 챌린지에서 우측 UI 버튼으로 판정하면 기존 챌린지가 완료되는 동시에 같은 방향 공격이 다시 실행되어 새 챌린지가 시작되는 문제 수정 요청.

### 핵심 요구사항
- UI 방향 버튼 클릭을 타이밍 판정과 일반 공격 입력으로 중복 처리하지 않는다.
- 타이밍 챌린지 중 UI 버튼 입력은 판정 한 번만 실행한다.
- UI 밖 마우스 입력과 키보드·게임패드 입력은 기존 동작을 유지한다.

### 이번 작업 범위
- 타이밍 판정 입력에서 UI 위 마우스 왼쪽 클릭과 `Player/Attack` 바인딩의 중복 처리 차단.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Infrastructure/Input/InputReader.cs`
  - 포인터가 UI 위에 있을 때 마우스 왼쪽 `Attack` 입력을 타이밍 판정에서 제외.
- `Docs/input.md`
  - 문제 원인, 수정 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 기존에는 마우스 버튼을 누르는 순간 `Player/Attack`이 챌린지를 먼저 완료하고, 버튼을 놓을 때 UI `onClick`이 일반 이동으로 다시 큐에 들어갔다.
- `WasTimingActionPressed()`에서 마우스 왼쪽 버튼이 눌렸고 현재 포인터가 UI 위라면 `Attack` 판정 입력을 무시하도록 변경했다.
- UI 버튼의 `onClick`에서 생성되는 `QueueTimingAction()`만 남으므로 챌린지가 한 번만 완료되고 방향 이동 큐가 추가되지 않는다.
- UI 밖의 마우스 클릭, 키보드 아무 키, 게임패드 판정 입력은 기존처럼 사용할 수 있다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 실행 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 실행 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 방향 UI 버튼을 눌러 기존 챌린지만 완료되고 새 챌린지가 시작되지 않는지는 직접 확인하지 못했다.
## 2026-07-22 이동 버튼 패널 좌우 배치 설정 연결

### 사용자의 요청 개요
- 설정 패널의 `LeftButton-Toggle`, `RightButton-Toggle` 선택에 맞춰 `MoveButtonPanel` 위치가 좌우로 변경되도록 연결 요청.

### 핵심 요구사항
- 현재 `MoveButtonPanel` 위치를 우측 배치 기준으로 유지한다.
- 왼쪽 토글 선택 시 패널을 화면 좌측의 대칭 위치로 옮긴다.
- 오른쪽 토글 선택 시 기존 우측 위치로 되돌린다.

### 이번 작업 범위
- 기존 이동 버튼 방향 설정 이벤트를 실제 `GameScene`의 `MoveButtonPanel`에 연결.
- 현재 씬 배치와 이동 버튼 패널의 가로 여백을 일치시킴.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/UI/MobileControlSettingsTarget.cs`
  - 기본 가로 여백을 현재 우측 배치값인 50으로 조정.
- `Assets/Arkeum/Scenes/GameScene.unity`
  - `MoveButtonPanel`에 `MobileControlSettingsTarget`을 추가하고 패널의 `RectTransform`을 연결.
- `Docs/input.md`
  - 이번 요청, 변경 범위와 검증 결과를 기록.

### 실제 수행한 작업 요약
- `GameSettingsService.MovementSide` 변경 이벤트가 발생하면 이동 버튼 패널의 앵커와 피벗을 좌/우 하단으로 전환하도록 기존 적용 컴포넌트를 씬에 연결했다.
- 왼쪽 배치는 `(x: 50, y: 50)`, 오른쪽 배치는 `(x: -50, y: 50)`의 하단 여백을 사용한다.
- 저장된 설정도 게임 씬 활성화 시 즉시 적용된다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 두 토글을 직접 눌렀을 때의 실제 화면 전환은 확인하지 못했다.

## 2026-07-24 플레이어 Idle 애니메이션 추가

### 사용자의 요청 개요
- 플레이어 캐릭터가 정지 상태에서도 Idle 스프라이트 애니메이션을 반복 재생하도록 구현 요청.

### 핵심 요구사항
- 기존 런타임 액터 생성 구조를 유지한다.
- 허브와 던전의 플레이어 캐릭터에 같은 Idle 애니메이션을 적용한다.
- 기존 이동 보간과 좌우 방향 전환을 유지한다.
- 적 캐릭터의 기존 표시 동작에는 영향을 주지 않는다.

### 이번 작업 범위
- `ActorView`에 스프라이트 프레임 기반 Idle 반복 재생 기능 추가.
- `WorldVisualSet`에 플레이어 Idle 프레임과 재생 속도 설정 추가.
- `WorldPresenter`에서 플레이어 View에만 Idle 설정 연결.
- 기존 Soldier Idle 스프라이트 6개를 `WorldVisualSet.asset`에 등록.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/ActorView.cs`
  - Idle 프레임을 지정된 속도로 반복 재생하는 코루틴과 설정 API 추가.
  - Idle 재생 중 일반 새로고침이 현재 애니메이션 프레임을 덮어쓰지 않도록 처리.
- `Assets/Arkeum/Scripts/Presentation/World/WorldVisualSet.cs`
  - 플레이어 Idle 프레임 배열과 초당 프레임 수 설정 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 허브 및 런 플레이어 View 생성·새로고침 시 Idle 설정 전달.
- `Assets/Arkeum/ScriptableObjects/WorldVisualSet.asset`
  - `Soldier-Idle_0`부터 `Soldier-Idle_5`까지 6프레임과 8 FPS 설정 등록.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 플레이어 액터가 생성되면 첫 유효 Idle 프레임을 즉시 표시하고 6개 프레임을 8 FPS로 반복 재생한다.
- 같은 Idle 설정이 반복 전달될 때 코루틴을 재시작하지 않아 액터 새로고침 중 애니메이션이 매번 처음으로 돌아가지 않게 했다.
- 기존 `SpriteRenderer.flipX` 기반 좌우 방향 전환과 위치 이동 코루틴은 그대로 유지된다.
- Idle 프레임이 없거나 모두 비어 있으면 기존 정지 스프라이트 표시 방식으로 동작한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 허브와 던전 플레이어의 실제 Idle 반복 재생 모습은 직접 확인하지 못했다.
- 프레임별 잘린 영역 차이로 캐릭터가 미세하게 흔들려 보일 경우 Sprite Editor에서 여섯 프레임의 Pivot을 같은 위치로 맞출 필요가 있다.

## 2026-07-24 적 이동 충돌 데미지 왕복 애니메이션

### 사용자의 요청 개요
- 적이 이동하려는 타일에 플레이어가 있어 이동 충돌 데미지를 줄 때, 일반 이동과 구분되는 왕복 점프 애니메이션 추가 요청.

### 핵심 요구사항
- 적이 플레이어 타일로 이동 충돌 데미지를 준 경우에만 실행한다.
- 적의 실제 격자 위치는 기존 타일에 유지한다.
- 플레이어 방향으로 타일 간 거리의 절반만 이동한다.
- 전진 중 높이 `0.5`까지 상승하고, 원래 타일로 돌아오면서 하강한다.
- 기존 일반 적 이동 애니메이션에는 영향을 주지 않는다.

### 이번 작업 범위
- 이동 충돌 데미지 발생 적과 목표 칸을 일회성 피드백으로 기록.
- 월드 화면 갱신 시 해당 피드백을 소비해 충돌 적 View에 왕복 애니메이션 실행.
- `ActorView`에 논리 위치를 변경하지 않는 충돌 이동 애니메이션 추가.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Actors/ActorEntity.cs`
  - 저장 대상에서 제외되는 이동 충돌 화면 피드백 필드 추가.
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyBehaviorActions.cs`
  - 적의 준비된 이동 목표가 플레이어 위치일 때 충돌 피드백 기록.
- `Assets/Arkeum/Scripts/Gameplay/Combat/EnemyTurnSystem.cs`
  - 적 턴 시작 시 이전 이동 충돌 피드백 초기화.
- `Assets/Arkeum/Scripts/Presentation/World/ActorView.cs`
  - 목표 타일까지 절반 전진 후 원위치로 돌아오는 점프 코루틴 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 보이는 적의 이동 충돌 피드백을 한 번 소비해 해당 View 애니메이션 실행.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 이동 충돌 데미지가 발생하면 적 ID에 해당하는 View만 왕복 점프한다.
- 애니메이션 전체 시간은 기존 적 이동과 같은 `0.16초`를 사용한다.
- 전반부에는 원래 위치에서 목표 타일의 50% 지점까지 이동하며 높이 `+0.5`까지 상승한다.
- 후반부에는 같은 경로를 되돌아오며 높이가 감소하고 원래 월드 위치에 정확히 복귀한다.
- 애니메이션 중에도 적의 `GridPosition`은 변경하지 않아 게임 판정과 후속 턴에는 영향을 주지 않는다.
- 피드백은 화면 갱신 시 한 번만 소비되므로 이후 새로고침에서 반복 재생되지 않는다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 이동 충돌 데미지 발생 시 전진 거리, 최고 높이와 복귀 타이밍은 직접 확인하지 못했다.
- 체감상 너무 빠르거나 느리면 `ActorView.EnemyMoveDuration`, 충돌 높이가 과하면 `EnemyJumpHeight` 값을 조정할 수 있다.

## 2026-07-24 플레이어 피격 화면 흔들림 및 모바일 진동 연결

### 사용자의 요청 개요
- 플레이어가 데미지를 받으면 화면 흔들림과 모바일 진동을 실행하고 Settings의 각 토글에 연결 요청.

### 핵심 요구사항
- 플레이어 HP가 실제로 감소한 경우에만 피격 피드백을 실행한다.
- ScreenShake 설정이 켜져 있을 때만 카메라를 흔든다.
- Vibration 설정이 켜져 있고 모바일 플랫폼일 때만 진동을 실행한다.
- 기존 Settings 저장 및 UI 갱신 흐름을 유지한다.

### 이번 작업 범위
- 플레이어 데미지 감지 결과를 한 번 계산해 피격 사운드와 시각·진동 피드백에 공통 사용.
- 기존 카메라 흔들림 기능을 플레이어 피격에서도 호출할 수 있도록 일반화.
- 기존 `GameSettingsService.TryVibrate()`를 플레이어 피격 흐름에 연결.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - 처리 전후 플레이어 HP 비교 결과를 저장하고, HP 감소 시 플레이어 피격 피드백 호출.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 화면 흔들림과 모바일 진동을 함께 실행하는 `PlayPlayerDamageFeedback()` 추가.
  - 기존 적 데미지 화면 흔들림 메서드를 공용 데미지 화면 흔들림 메서드로 이름 변경.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 플레이어 행동과 이어지는 적 턴 처리가 끝난 뒤 HP가 행동 시작 전보다 낮아졌는지 확인한다.
- HP가 감소하면 기존 피격 사운드에 더해 월드 새로고침 후 플레이어 피격 피드백을 실행한다.
- 화면 흔들림은 `GameSettingsService.ScreenShakeEnabled`가 켜진 경우에만 실행된다.
- 화면 흔들림 시간과 강도는 기존 `WorldVisualSet` 설정인 `0.12초`, `0.12`를 사용한다.
- 진동은 `GameSettingsService.TryVibrate()`를 통해 `Application.isMobilePlatform`과 `VibrationEnabled`가 모두 참일 때만 `Handheld.Vibrate()`를 호출한다.
- `OptionPanel.prefab`의 `Vibration-Toggle`과 `ScreenShake-Toggle`은 기존 `SettingsMenuBinder`에 이미 연결되어 있어 씬 및 프리팹 수정 없이 저장된 설정이 즉시 반영된다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 플레이어 피격 시 실제 카메라 흔들림은 직접 확인하지 못했다.
- 모바일 실기기에서 진동 작동과 Settings 토글 OFF 시 진동이 차단되는지는 확인하지 못했다.
- Unity Editor와 일반 데스크톱 빌드에서는 `Application.isMobilePlatform`이 거짓이므로 진동이 실행되지 않는다.

## 2026-07-24 적 캐릭터 Idle 애니메이션 추가

### 사용자의 요청 개요
- 플레이어와 동일한 스프라이트 프레임 방식의 Idle 애니메이션을 적 캐릭터에도 추가 요청.

### 핵심 요구사항
- 적 종류마다 서로 다른 Idle 프레임과 재생 속도를 설정할 수 있어야 한다.
- 기존 플레이어 Idle 애니메이션을 유지한다.
- 적 이동, 좌우 방향 전환과 이동 충돌 왕복 애니메이션을 유지한다.
- Idle 프레임이 없는 적은 기존 정지 스프라이트로 표시한다.

### 이번 작업 범위
- `EnemyDefinition`에 적별 Idle 프레임 배열과 FPS 설정 추가.
- `WorldPresenter`가 플레이어와 적의 Idle 설정을 각각 `ActorView`에 전달하도록 확장.
- Orc, Skeleton, Boss 정의 에셋에 기존 Idle 리소스 등록.
- Bat 정의에는 추후 프레임 등록이 가능한 빈 설정 추가.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Actors/EnemyDefinition.cs`
  - 적별 `idleFrames`, `idleFrameRate` 직렬화 필드와 읽기 프로퍼티 추가.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 액터 View 생성·새로고침 시 액터 종류에 맞는 Idle 프레임과 FPS 전달.
- `Assets/Arkeum/ScriptableObjects/Enemies/Orc/OrcDefinition.asset`
  - Orc Idle 6프레임과 8 FPS 등록.
- `Assets/Arkeum/ScriptableObjects/Enemies/Skeleton/SkeletonDefinition.asset`
  - Skeleton Idle 6프레임과 8 FPS 등록.
- `Assets/Arkeum/ScriptableObjects/Enemies/Boss/BossDefinition.asset`
  - 기존 붉은 Orc 계열 외형에 맞춰 Orc Idle 6프레임과 8 FPS 등록.
- `Assets/Arkeum/ScriptableObjects/Enemies/Bat/BatDefinition.asset`
  - 빈 Idle 프레임 배열과 8 FPS 기본 설정 추가.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 적 View도 기존 `ActorView.SetIdleAnimation()`을 사용해 프레임을 반복 재생한다.
- Orc와 Boss는 `Orc-Idle_0`부터 `Orc-Idle_5`까지, Skeleton은 `enemies-skeleton1_idle_0`부터 `_5`까지 재생한다.
- 적별 프레임 속도는 현재 모두 8 FPS로 설정했으며 각 `EnemyDefinition` Inspector에서 독립적으로 변경할 수 있다.
- 같은 설정이 반복 전달될 때 기존 코루틴을 유지하므로 월드 새로고침마다 애니메이션이 첫 프레임으로 돌아가지 않는다.
- 프레임 배열이 비어 있거나 유효한 프레임이 없으면 기존 `sprite`를 계속 사용한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 Orc, Skeleton, Boss의 실제 Idle 재생 크기, Pivot과 색상 적용 결과는 직접 확인하지 못했다.
- 현재 Bat에는 `Bat.png` 단일 이미지밖에 없어 실제 Idle 애니메이션은 적용되지 않았다. Bat용 분할 프레임을 추가한 뒤 `BatDefinition.Idle Frames`에 등록해야 한다.
- 프레임별 Pivot 차이로 적이 흔들려 보이면 해당 스프라이트 시트의 Pivot을 동일하게 맞출 필요가 있다.

## 2026-07-27 상점 랜덤 상품·진열대 프리팹·근접 설명 팝업 연결

### 사용자의 요청 개요
- 사용자가 제작한 `ShopShelfView` 프리팹을 상점 진열대 월드 표시에 사용하도록 요청.
- 별도 상품 목록에서 중복 없이 무작위 상품 3개를 골라 진열대에 배치하도록 요청.
- `GameScene/HUD-Canvas`에 배치한 `ShopOfferPopup`을 플레이어가 진열대에 접근했을 때 표시하고 멀어지면 숨기도록 요청.
- 팝업의 `ItemNameText`, `ItemDescImage`, `DescriptionText`, `PriceText`에 상품 정보를 연결하도록 요청.

### 핵심 요구사항
- 기존 `Shop.asset`의 진열대 좌표와 구매·저장 흐름을 유지한다.
- 새 런의 맵 생성 시 상품을 한 번만 무작위 선정하고 저장 데이터를 불러올 때는 재추첨하지 않는다.
- 판매 상품은 중복 없이 최대 3개를 선정한다.
- 여러 진열대와 인접하면 플레이어가 바라보는 방향의 진열대를 우선 표시한다.
- 구매 완료, 거리 이탈, 상점 퇴장 또는 런 종료 시 팝업을 숨긴다.
- 골드가 부족하면 가격을 다른 색으로 표시한다.

### 이번 작업 범위
- 상점 상품 카탈로그 ScriptableObject와 1층용 기본 카탈로그 에셋 추가.
- 기존 상점 진열대 데이터를 위치 슬롯으로 사용하고 카탈로그 상품을 런타임에 배정.
- `ShopShelfView.prefab`을 `WorldPresenter`에 연결해 진열대와 상품 아이콘을 함께 표시.
- `ShopOfferPopupPresenter`를 팝업 프리팹과 `GameScene` 오브젝트에 연결.
- 무기별 상점 설명 이미지 설정 필드 추가.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/ShopCatalogDefinition.cs`
  - 판매 무기, 가격, 설명 목록을 보관하는 상점 카탈로그 데이터 추가.
- `Assets/Arkeum/ScriptableObjects/ShopCatalog.asset`
  - 창, 단검, 대검 기본 상품 3개 등록.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunFloorDefinition.cs`
  - 층별 `ShopCatalog`, `ShopOfferCount` 설정 추가.
- `Assets/Arkeum/ScriptableObjects/RunDefinition.asset`
  - 1층에 기본 카탈로그와 상품 수 3개 설정.
- `Assets/Arkeum/Scripts/Gameplay/Map/MapGenerator.cs`
  - 맵 생성 완료 후 유효 카탈로그 상품을 섞고, 진열대 위치에 중복 없이 최대 지정 개수 배정.
- `Assets/Arkeum/Scripts/Gameplay/Run/WeaponDefinition.cs`
  - 팝업 `ItemDescImage`용 `shopDescriptionSprite` 설정 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 인접 상품 조회 API 추가, 바라보는 방향 우선 처리, 기존 인접 상품 HUD 로그 제거.
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - `ShopShelfView.prefab` 인스턴스 생성 및 `ShelfRenderer`/`ItemIconRenderer` 설정.
- `Assets/Arkeum/Scripts/Presentation/UI/ShopOfferPopupPresenter.cs`
  - 팝업 자식 UI 자동 탐색, 상품 정보 표시, 접근·이탈 및 가격 색상 처리.
- `Assets/Arkeum/Scripts/Presentation/UI/HudPresenter.cs`
  - 현재 런과 플레이어 위치를 기준으로 상점 팝업을 지속 갱신.
- `Assets/Arkeum/Prefabs/ShopOfferPopup.prefab`
  - `ShopOfferPopupPresenter` 컴포넌트 추가.
- `Assets/Arkeum/Scenes/GameScene.unity`
  - 씬 팝업에 Presenter를 연결하고 `WorldPresenter`에 `ShopShelfView.prefab` 참조 설정.
- `Assembly-CSharp.csproj`
  - 신규 런타임 스크립트 컴파일 항목 추가.
- 신규 스크립트와 에셋의 `.meta`
  - Unity GUID 및 임포터 정보 추가.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- `Shop.asset.ShopOffers`에 등록된 세 좌표는 진열대 위치 슬롯으로 유지하고, 새 런 맵이 생성된 뒤 해당 위치의 상품 데이터만 카탈로그 결과로 교체한다.
- 카탈로그의 유효한 상품을 섞어 중복 없이 최대 `ShopOfferCount`개를 선택한다.
- 선정 결과는 기존 런타임 `MapDefinition.ShopOffers`에 들어가므로 기존 저장/불러오기에서 무기, 가격, 설명이 그대로 유지된다.
- 현재 기본 카탈로그에는 창 3 Gold, 단검 1 Gold, 대검 3 Gold를 등록했다.
- 월드 진열대는 사용자가 만든 `ShopShelfView.prefab`을 생성하고 `ItemIconRenderer`에 실제 선정 무기 스프라이트와 색상을 적용한다.
- 팝업은 자식 이름으로 네 UI 요소를 찾아 이름, 설명 이미지, 설명, 가격을 설정한다.
- `ItemDescImage`는 무기의 `Shop Description Sprite`를 우선 사용하고, 미설정 시 기존 무기 스프라이트를 임시 이미지로 사용한다.
- 플레이어가 상하좌우 한 칸 안에 있는 동안 팝업을 표시하고, 여러 상품이 인접하면 `FacingDirection`의 상품을 먼저 선택한다.
- 현재 골드가 가격보다 적으면 가격 글자를 붉은색으로 표시한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 랜덤 상품 표시, 프리팹 크기·정렬, 팝업 접근·이탈과 구매 후 숨김은 직접 확인하지 못했다.
- 현재 카탈로그 상품이 정확히 3개이므로 구성은 항상 세 무기이며 배치 순서만 무작위다. 서로 다른 조합을 만들려면 `ShopCatalog.asset`에 네 번째 이상의 상품을 추가해야 한다.
- 전용 작동 설명 이미지는 아직 지정되지 않았다. 각 `WeaponDefinition`의 `Shop Description Sprite`에 이미지를 지정하기 전까지 `ItemDescImage`에는 무기 아이콘이 표시된다.
- `ShopOfferPopup`의 실제 모바일 화면 배치와 텍스트 넘침은 Unity Game View에서 확인 후 RectTransform 및 폰트 크기 조정이 필요할 수 있다.

## 2026-07-27 상점 설명 팝업을 진열대 PopupAnchor에 연결

### 사용자의 요청 개요
- `ShopOfferPopup`이 고정된 HUD 위치가 아니라 현재 선택된 `ShopShelfView`의 `PopupAnchor` 위치에 나타나도록 요청.

### 핵심 요구사항
- 인접 상품 선택 규칙과 팝업 표시·숨김 조건은 기존대로 유지한다.
- 실제 생성된 진열대 프리팹의 `PopupAnchor` 위치를 사용한다.
- 월드 좌표를 `HUD-Canvas` 좌표로 변환해 화면 공간 팝업을 배치한다.
- 카메라 이동 시 팝업 위치도 계속 갱신한다.

### 이번 작업 범위
- `WorldPresenter`가 상품 좌표별 진열대 `PopupAnchor`를 추적하도록 확장.
- `HudPresenter`가 현재 `WorldPresenter`를 상점 팝업 갱신에 전달.
- `ShopOfferPopupPresenter`가 앵커 월드 좌표를 화면 및 Canvas 로컬 좌표로 변환하도록 확장.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Presentation/World/WorldPresenter.cs`
  - 생성된 `ShopShelfView`의 `PopupAnchor`를 상품 좌표별로 저장하고 월드 위치 조회 API 제공.
  - 마커 재생성 시 이전 앵커 참조 제거.
- `Assets/Arkeum/Scripts/Presentation/UI/HudPresenter.cs`
  - 팝업 갱신 시 `WorldPresenter` 전달.
- `Assets/Arkeum/Scripts/Presentation/UI/ShopOfferPopupPresenter.cs`
  - `Camera.WorldToScreenPoint()`와 `RectTransformUtility.ScreenPointToLocalPointInRectangle()`을 사용한 HUD 좌표 변환 추가.
- `Docs/input.md`
  - 이번 요청, 변경 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 진열대 프리팹 생성 시 `PopupAnchor` 자식을 찾아 해당 상품의 격자 좌표와 연결한다.
- 플레이어와 인접한 상품이 선택되면 연결된 앵커의 실제 월드 위치를 조회한다.
- 현재 월드 카메라로 앵커를 화면 좌표로 변환하고, `HUD-Canvas`가 Screen Space Overlay인지 Camera 방식인지에 맞춰 팝업 `RectTransform.anchoredPosition`을 설정한다.
- 앵커가 없거나 카메라 뒤에 있으면 팝업을 숨긴다.
- 기존 접근, 이탈, 구매, 런 종료 및 바라보는 방향 우선 규칙은 유지한다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공.
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공.
- 최종 빌드 결과: 경고 0개, 오류 0개.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 `PopupAnchor`와 팝업 중심점의 시각적 일치, 화면 가장자리에서의 잘림 여부는 직접 확인하지 못했다.
- 팝업은 자신의 Pivot을 기준으로 앵커 위치에 정렬된다. 앵커를 팝업의 모서리에 맞추려면 `ShopOfferPopup`의 Pivot 또는 `ShopShelfView/PopupAnchor` 위치를 Inspector에서 조정해야 한다.
- 화면 가장자리에서 팝업 전체를 화면 안으로 제한하는 Clamp 처리는 현재 포함하지 않았다.

## 2026-07-29 타이밍 팝업 준비 시간 및 시작·성공·실패 효과음 추가

### 사용자의 요청 개요
- 타이밍 팝업이 표시되자마자 판정이 진행되는 부담을 줄이기 위해 시작 전 대기 시간을 추가하고, 시작·성공·실패 상황별 효과음을 연결해 달라는 요청.

### 핵심 요구사항
- 팝업을 먼저 표시한 뒤 0.8초 동안 타이밍 런타임을 정지한다.
- 준비 시간이 끝나는 시점에 시작 효과음을 재생한다.
- 준비 시간 중 입력은 판정에 사용하거나 이후 프레임으로 넘기지 않는다.
- 타이밍 판정 결과에 따라 서로 다른 성공·실패 효과음을 재생한다.
- 기존 SFX 볼륨 및 음소거 설정을 그대로 적용한다.

### 이번 작업 범위
- 타이밍 정의별로 조절 가능한 시작 대기 시간 설정 추가.
- 타이밍 세션에 준비 상태와 남은 준비 시간 관리 추가.
- 게임 진행 루프에서 준비 중 입력 폐기, 시작음 및 결과음 호출 연결.
- 기존 프로젝트 오디오 클립을 활용한 타이밍 SFX 세 종류 등록.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingChallengeDefinition.cs`
  - 타이밍 종류별 `Start Delay Seconds` 설정과 읽기 프로퍼티 추가.
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingSession.cs`
  - 준비 시간, 시작 상태와 준비 시간 갱신 로직 추가.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - 준비 중 런타임 정지 및 입력 폐기, 시작·결과 효과음 호출 연결.
- `Assets/Arkeum/Scripts/Presentation/Audio/AudioCueService.cs`
  - 타이밍 시작·성공·실패 SFX 호출 API 추가.
- `Assets/Arkeum/ScriptableObjects/Timing/ClockHandTimingChallenge.asset`
  - 시작 대기 시간을 0.8초로 설정.
- `Assets/Arkeum/ScriptableObjects/Timing/RadialShrinkTimingChallenge.asset`
  - 시작 대기 시간을 0.8초로 설정.
- `Assets/Arkeum/ScriptableObjects/Timing/SinglePressTimingChallenge.asset`
  - 시작 대기 시간을 0.8초로 설정.
- `Assets/Arkeum/Prefabs/AudioManager.prefab`
  - `TimingStart`, `TimingSuccess`, `TimingFailed` SFX 항목과 기존 오디오 클립 연결.
- `Docs/input.md`
  - 이번 요청, 변경 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 타이밍 세션 생성 시 정의에 설정된 준비 시간을 저장하고, 준비 시간이 끝나기 전에는 기존 타이밍 런타임의 `Tick()`을 호출하지 않는다.
- 준비 시간 동안 키보드 및 모바일 타이밍 입력을 매 프레임 소비하여 시작 직후 의도하지 않은 판정으로 이어지지 않게 했다.
- 0.8초가 지나 세션이 시작 상태로 바뀌는 순간 `TimingStart` 효과음을 재생한다.
- 성공 판정에는 `TimingSuccess`, 실패 입력 및 시간 초과에는 `TimingFailed` 효과음을 재생한다.
- 시작음에는 `013_Confirm_03`, 성공음에는 `16_Atk_buff_04`, 실패음에는 `029_Decline_09` 기존 클립을 사용했다.
- 세 효과음은 `AudioManager`의 기존 SFX 경로를 사용하므로 저장된 SFX 볼륨과 음소거 설정을 따른다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 0.8초 준비 시간의 실제 체감과 시작음·움직임의 시청각 동기화는 직접 확인하지 못했다.
- 시작·성공·실패 효과음의 음량과 음색이 전투 BGM 및 공격음과 잘 구분되는지는 실제 재생 후 조정할 수 있다.

## 2026-07-29 게임 진행 주요 상황 효과음 확장

### 사용자의 요청 개요
- 사운드 점검 결과에서 제안한 적 명중·처치, 장비·상점, 보스방, 층 전환·런 결과, 막힘 및 일시정지 효과음을 실제 게임 흐름에 적용해 달라는 요청.

### 핵심 요구사항
- 플레이어 공격음과 별도로 적 명중 및 처치 결과를 소리로 구분한다.
- 무기 획득·교체, 상점 구매 성공 및 구매 불가 상황에 맞는 효과음을 재생한다.
- 보스방 진입·봉인·개방을 소리로 강조한다.
- 층 클리어·다음 층 이동, 플레이어 사망·최종 클리어 결과를 구분한다.
- 벽 충돌 및 상점 거절음의 과도한 반복 재생을 제한한다.
- 기존에 3으로 설정된 SFX 피치를 정상 범위로 조정한다.

### 이번 작업 범위
- 런 액션 피드백 플래그를 주요 전투·아이템·진행 이벤트까지 확장.
- 게임 진행 결과에 따라 중앙 오디오 큐 서비스를 통해 SFX 호출.
- 프리팹에 기존 프로젝트 오디오 클립 기반 신규 SFX 항목 등록.
- SFX별 재생 간격과 지연 재생 기능 추가.
- 일시정지·재개 및 허브 벽 충돌 피드백 연결.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Run/RunActionFeedback.cs`
  - 적 명중·처치, 장비 획득·해제, 구매, 거절, 보스방 및 층 클리어 피드백 플래그 추가.
- `Assets/Arkeum/Scripts/Gameplay/Run/RunController.cs`
  - 실제 전투, 획득, 구매, 보스방 봉인·개방 및 출구 사용 결과에 피드백 플래그 설정.
  - 막힌 길도 턴을 소비하지 않는 처리 결과로 반환해 거절음을 재생하도록 연결.
- `Assets/Arkeum/Scripts/Presentation/Audio/AudioCueService.cs`
  - 확장된 액션 피드백을 SFX ID로 변환.
  - 층 이동, 런 사망·클리어 및 허브 거절음 호출 API 추가.
  - 동시에 발생하는 보스 봉인·층 이동·결과음을 짧게 지연해 겹침 완화.
- `Assets/Arkeum/Scripts/Presentation/Audio/AudioManager.cs`
  - SFX 항목별 재생 간격 설정과 unscaled time 기반 지연 재생 지원.
- `Assets/Arkeum/Scripts/Core/GameDirector.cs`
  - 허브 벽 충돌, 다음 층 이동, 런 결과 표시 시 해당 효과음 호출.
- `Assets/Arkeum/Scripts/Presentation/UI/PauseMenuController.cs`
  - 일시정지 및 재개 효과음 연결.
- `Assets/Arkeum/Prefabs/AudioManager.prefab`
  - 적 명중·처치, 장비, 상점, 거절, 보스방, 층 전환, 결과, 일시정지 SFX 등록.
  - 기존 SFX 피치를 0.8~1.15 범위로 조정하고 상황별 음량·랜덤 피치·재생 간격 설정.
- `Docs/input.md`
  - 이번 요청, 구현 범위와 검증 결과 기록.

### 실제 수행한 작업 요약
- 공격 시 기존 휘두르기 소리 뒤에 명중 여부와 처치 여부에 따라 `EnemyHit`, `EnemyDefeated`가 추가로 재생된다.
- 다중 타격은 한 액션당 명중음과 처치음을 각각 한 번만 재생해 과도한 중첩을 방지한다.
- 바닥 무기 획득은 장착음, 기존 무기를 내려놓으면 해제음, 상점 구매는 구매음으로 구분했다.
- 골드 부족, 빈 진열대와 벽 충돌에는 `ActionDenied`를 사용하고 0.15초 재생 제한을 적용했다.
- 보스방 진입 시 조우음 후 0.18초 뒤 봉인음을 재생하며, 전멸 후에는 개방음을 재생한다.
- 출구 사용 시 층 클리어음, 다음 층 생성 후 0.25초 뒤 착지 계열 이동음을 재생한다.
- 최종 결과는 피격·층 클리어음과 겹치지 않도록 사망은 0.2초, 클리어는 0.3초 뒤 별도 결과음을 재생한다.
- Escape 키와 UI 버튼 양쪽의 일시정지·재개 흐름에 전용 효과음을 연결했다.
- 기존 `PlayerHit`가 거절음과 같은 클립을 사용하던 설정을 실제 피격 클립 `61_Hit_03`으로 교체했다.
- 신규 클립은 모두 기존 `Assets/Arkeum/Audio/SFX` 리소스를 재사용했다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `AudioManager.prefab`에서 참조하는 오디오 GUID가 모두 프로젝트 `.meta`에 존재하는지 확인했다.

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 BGM과 함께 들었을 때의 실제 음량, 피치와 지연 간격은 직접 확인하지 못했다.
- 보스방 진입, 마지막 적 처치, 다음 층 이동과 최종 결과처럼 여러 큐가 이어지는 구간은 실제 재생 후 간격 조정이 필요할 수 있다.
- 이번 효과음은 화면 전체에 일관되게 들리는 2D SFX로 연결했다. 적 위치 기반 공간 음향은 현재 카메라·AudioListener 거리 감쇠 설정을 먼저 확인한 뒤 별도로 적용하는 것이 안전하다.

## 2026-07-29 타이밍 판정 후행 여유 범위 추가

### 사용자의 요청 개요
- 화면에 보이는 타이밍 마커보다 실제 판정이 앞서 진행되는 것처럼 느껴져, 초록색 성공 범위 안에서 조금 늦게 입력했을 때 실패하는 현상을 완화해 달라는 요청.

### 핵심 요구사항
- 화면에 표시되는 성공 범위와 기존 난이도 인상은 유지한다.
- 타이밍 진행 방향의 뒤쪽에만 실제 성공 판정 범위를 조금 더 제공한다.
- 아직 성공 범위에 도달하지 않은 빠른 입력에는 추가 보정을 적용하지 않는다.
- 선형, 시계 회전, 원형 축소 타이밍 모두 같은 시간 기준의 체감 여유를 사용한다.

### 이번 작업 범위
- 모든 타이밍 정의에 조절 가능한 늦은 입력 허용 시간 추가.
- 타이밍 방식별 이동 방향에 맞춘 후행 판정 범위 계산 적용.
- 세 타이밍 에셋에 기본 0.06초 허용 시간 설정.

### 변경된 파일과 변경 목적
- `Assets/Arkeum/Scripts/Gameplay/Timing/TimingChallengeDefinition.cs`
  - 공통 `Late Input Grace Seconds` 설정과 읽기 프로퍼티 추가.
- `Assets/Arkeum/Scripts/Gameplay/Timing/SinglePressTimingChallengeDefinition.cs`
  - 오른쪽으로 이동하는 마커의 성공 구간 뒤쪽에 시간 기준 판정 여유 적용.
- `Assets/Arkeum/Scripts/Gameplay/Timing/ClockHandTimingChallengeDefinition.cs`
  - 시계바늘의 실제 회전 방향 뒤쪽으로 성공 각도 확장.
- `Assets/Arkeum/Scripts/Gameplay/Timing/RadialShrinkTimingChallengeDefinition.cs`
  - 축소 마커가 최근 허용 시간 동안 성공 링을 통과했는지 판정.
- `Assets/Arkeum/ScriptableObjects/Timing/ClockHandTimingChallenge.asset`
  - 늦은 입력 허용 시간을 0.06초로 설정.
- `Assets/Arkeum/ScriptableObjects/Timing/RadialShrinkTimingChallenge.asset`
  - 늦은 입력 허용 시간을 0.06초로 설정.
- `Assets/Arkeum/ScriptableObjects/Timing/SinglePressTimingChallenge.asset`
  - 늦은 입력 허용 시간을 0.06초로 설정.
- `Docs/input.md`
  - 이번 요청, 구현 방식과 검증 결과 기록.

### 실제 수행한 작업 요약
- 성공 영역을 시각적으로 넓히지 않고 판정에만 0.06초의 후행 여유를 추가했다.
- 선형 타이밍은 현재 마커 위치부터 0.06초 전 위치까지의 구간이 성공 영역과 겹치면 성공으로 판정한다.
- 시계 타이밍은 회전 속도를 기준으로 0.06초에 해당하는 각도를 계산해 시계방향 또는 반시계방향의 뒤쪽 경계만 확장한다.
- 원형 축소 타이밍은 현재 반지름과 0.06초 전 반지름 사이가 성공 링을 통과하거나 겹치면 성공으로 판정한다.
- 성공 영역에 도달하기 전의 빠른 입력에는 여유 범위가 추가되지 않는다.

### 빌드/테스트 여부
- `dotnet build Assembly-CSharp.csproj -nologo` 성공 (경고 0개, 오류 0개).
- `dotnet build Assembly-CSharp-Editor.csproj -nologo` 성공 (경고 0개, 오류 0개).

### 확인하지 못한 사항 또는 후속 점검 사항
- Unity Play Mode에서 실제 표시 위치와 입력 판정의 체감 일치는 직접 확인하지 못했다.
- 0.06초가 여전히 짧거나 지나치게 관대하면 각 타이밍 에셋의 `Late Input Grace Seconds`를 조정할 수 있다.
