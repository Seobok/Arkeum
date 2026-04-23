# Dungeon Map Refactor Requirements

## 목적

런 던전 맵을 단일 고정 맵에서 여러 방과 통로로 구성된 층 구조로 확장한다. 기존 `MapAsset`은 개별 방 템플릿으로 활용하며, 생성된 층은 `MapDefinition`에 방, 문, 통로 메타데이터와 walkable cell 정보를 함께 제공한다.

## 확정 요구사항

### 던전과 방

- 던전은 여러 개의 방으로 구성된다.
- 방은 기존 `MapAsset` 데이터를 활용한다.
- 하나의 층에는 최소 6개의 방이 존재해야 한다.
- 한 층은 시작 방용 `MapAsset` 1개와 일반 방용 `RoomAssets` 여러 개를 사용할 수 있다.
- 플레이어 시작 방은 월드 좌표 `(0, 0)`을 기준으로 생성한다.
- 플레이어는 시작 방 `MapAsset`의 원점, 즉 `PlayerSpawn`을 월드 좌표 `(0, 0)`으로 정규화한 위치에서 시작한다.
- 시작 방을 제외한 방은 `RoomAssets`에서 랜덤하게 선택해 배치한다.
- 방끼리는 서로 겹치면 안 된다.

### 층별 방 템플릿

- `RunFloorDefinition.MapAsset`은 해당 층의 시작 방 템플릿이다.
- `RunFloorDefinition.RoomAssets`는 시작 방 이후에 생성되는 일반 방 템플릿 목록이다.
- 던전 생성 시 0번 방은 항상 `MapAsset`을 사용한다.
- 1번 방부터는 `RoomAssets` 목록에서 랜덤하게 선택한다.
- `RoomAssets`가 비어 있으면 기존 호환을 위해 시작 방 템플릿을 일반 방 후보로도 사용한다.
- `MapAsset`이 비어 있으면 기본 직사각형 fallback 방을 시작 방으로 사용한다.

### 문

- 문은 `MapAsset`에서 직접 찍는 편집 데이터로 관리한다.
- 문 데이터는 좌표와 방향을 가진다.
- 문 방향은 방을 기준으로 하며 `DoorDirection` enum으로 관리한다.
- `MapAssetEditorWindow`는 `Door` 브러시와 `DoorErase` 브러시를 제공한다.
- `Door` 브러시는 현재 선택한 방향의 문을 해당 cell에 추가한다.
- 지형 cell을 지우면 해당 위치의 문도 함께 제거한다.
- 문은 walkable cell 위에 있어야 한다.
- 던전 생성기는 연결 방향에 맞는 명시적 문 후보만 사용한다.
- 기존/비어 있는 에셋 호환을 위해 문 데이터가 전혀 없는 템플릿은 임시 기본 문 후보를 생성한다.

### 통로

- 방과 방은 통로로 연결된다.
- 통로는 문과 문을 이어준다.
- 통로는 방 내부를 뚫고 지나갈 수 없다.
- 방과 통로는 문을 통해서만 접해야 한다.
- 같은 방향의 문끼리는 연결하지 않는다.
- 서로 수직 방향의 문끼리는 1회 꺾어서 연결한다.
  - 오른쪽/왼쪽 방향 문 좌표를 `(x1, y1)`, 위쪽/아래쪽 방향 문 좌표를 `(x2, y2)`라고 하면 꺾이는 좌표는 `(x2, y1)`이다.
- 서로 반대 방향의 문끼리는 일직선 또는 2회 꺾어서 연결한다.
  - 오른쪽/왼쪽 연결에서 2회 꺾는 경우 꺾이는 좌표는 `((x1 + x2) / 2, y1)`, `((x1 + x2) / 2, y2)`이다.
  - 위쪽/아래쪽 연결에서 2회 꺾는 경우 꺾이는 좌표는 `(x1, (y1 + y2) / 2)`, `(x2, (y1 + y2) / 2)`이다.
- 통로 depth는 현재처럼 층 번호 기반으로 둔다.
- 통로 depth 규칙은 추후 변경 가능성이 있으므로 생성 코드에서 별도 적용 지점으로 유지한다.

### 층별 생성 프로퍼티

- 층 생성 관련 값은 데이터 에셋에서 조정할 수 있어야 한다.
- `RunFloorDefinition`은 다음 프로퍼티를 가진다.
  - `MinimumRoomCount`: 생성할 최소 방 개수. 현재 최소 보정값은 6이다.
  - `RoomGap`: 방 배치 간격에 추가되는 여백.
  - `PlacementAttempts`: 랜덤 방 배치 시도 횟수.
  - `RandomSeed`: 층별 deterministic 생성에 사용하는 기본 seed.
- `RunFloorDefinition`이 없을 때는 기본값 `6 / 5 / 300 / 173`을 사용한다.

## 구현 요약

- `RunFloorDefinition.MapAsset`을 시작 방 템플릿으로 사용한다.
- `RunFloorDefinition.RoomAssets`를 일반 방 템플릿 후보 목록으로 사용한다.
- `MapGenerator.CreateRunMap()`은 시작 방 템플릿과 일반 방 템플릿 후보를 분리한 `RoomTemplateSet`을 만든다.
- `MapGenerator`는 0번 방을 시작 방 템플릿으로 생성하고, 이후 방마다 `RoomAssets` 기반 후보 중 하나를 랜덤 선택한다.
- 서로 다른 크기의 방 템플릿이 섞일 수 있으므로, 새 방 origin은 부모 방 bounds와 후보 방 bounds, `RoomGap`을 기준으로 계산한다.
- 방 겹침 검사는 실제 cell set 기준으로 유지한다.
- `MapAsset`에 `Doors: List<MapDoorData>`를 추가한다.
- `MapDoorData`는 `Position`, `Direction`을 가진다.
- `MapAssetEditorWindow`에 `Door`, `DoorErase` 툴과 문 방향 선택 UI를 추가한다.
- `MapDefinition`에 `Rooms`, `Corridors` 메타데이터를 추가한다.
- `MapGenerator.CreateHubMap()`은 기존처럼 `hubMapAsset`을 완성 맵으로 사용할 수 있다.

## 남은 의문점

- 일반 방도 타입별 후보군, 예를 들어 전투방/보상방/상점방/보스방으로 나눌지 결정이 필요하다.
- 방 연결을 현재처럼 인접 grid 기반으로 유지할지, 더 자유로운 그래프/스패닝 트리 방식으로 확장할지 결정이 필요하다.
- 하나의 문이 여러 통로에 재사용될 수 있는지, 한 문당 하나의 연결만 허용할지 결정이 필요하다.
- 통로 depth를 층 번호 기반에서 방 depth 또는 별도 corridor depth 규칙으로 바꿀 때 어떤 데이터 에셋에 노출할지 결정이 필요하다.
