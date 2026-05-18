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
