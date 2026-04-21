# Dungeon Map Refactor Requirements

## 목적

런 던전 맵을 단일 고정 맵에서 여러 방과 통로로 구성된 층 구조로 확장한다. 기존 `MapAsset`은 개별 방 템플릿으로 활용하며, 생성된 층은 `MapDefinition`에 방, 문, 통로 메타데이터와 walkable cell 정보를 함께 제공한다.

## 확정 요구사항

### 던전과 방

- 던전은 여러 개의 방으로 구성된다.
- 방은 기존 `MapAsset` 데이터를 활용한다.
- 하나의 층에는 최소 6개의 방이 존재해야 한다.
- 플레이어 시작 방은 월드 좌표 `(0, 0)`을 기준으로 생성한다.
- 시작 방을 제외한 방은 랜덤하게 배치한다.
- 방끼리는 서로 겹치면 안 된다.

### 문

- 문은 `MapAsset`에서 직접 찍는 편집 데이터로 관리한다.
- 문 데이터는 좌표와 방향을 가진다.
- 문 방향은 방을 기준으로 하며 `DoorDirection` enum으로 관리한다.
- `MapAssetEditorWindow`는 `Door` 브러시와 `DoorErase` 브러시를 제공한다.
- `Door` 브러시는 현재 선택한 방향의 문을 해당 cell에 추가한다.
- 지형 cell을 지우면 해당 위치의 문도 함께 제거한다.
- 문은 walkable cell 위에 있어야 한다.
- 던전 생성기는 연결 방향에 맞는 명시적 문 후보만 사용한다.
- 기존/비어 있는 에셋 호환을 위해 문 데이터가 전혀 없는 템플릿은 임시 기본 문 후보를 생성한다. 새 방 템플릿은 직접 문을 찍는 것을 기준으로 한다.

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
  - `RoomGap`: 방 배치 grid 간격에 추가되는 여백.
  - `PlacementAttempts`: 랜덤 방 배치 시도 횟수.
  - `RandomSeed`: 층별 deterministic 생성에 사용하는 기본 seed.
- `RunFloorDefinition`이 없을 때는 기본값 `6 / 5 / 300 / 173`을 사용한다.

## 구현 요약

- `MapAsset`에 `Doors: List<MapDoorData>`를 추가한다.
- `MapDoorData`는 `Position`, `Direction`을 가진다.
- `MapAssetEditorWindow`에 `Door`, `DoorErase` 툴과 문 방향 선택 UI를 추가한다.
- `MapDefinition`에 `Rooms`, `Corridors` 메타데이터를 추가한다.
- `DungeonRoomDefinition`은 방 id, origin, bounds, cells, 실제 연결된 doors를 가진다.
- `DungeonCorridorDefinition`은 연결 방 id, 양쪽 문 좌표/방향, corridor cells를 가진다.
- `MapGenerator.CreateRunMap()`은 `MapAsset`을 방 템플릿으로 사용해 런 던전을 생성한다.
- `MapGenerator`는 방 연결 방향에 맞는 `MapAsset.Doors` 후보를 선택한다.
- `MapGenerator.CreateHubMap()`은 기존처럼 `hubMapAsset`을 완성 맵으로 사용할 수 있다.

## 남은 의문점

- 방 연결을 현재처럼 인접 grid 기반으로 유지할지, 더 자유로운 그래프/스패닝 트리 방식으로 확장할지 결정이 필요하다.
- 하나의 문이 여러 통로에 재사용될 수 있는지, 한 문당 하나의 연결만 허용할지 결정이 필요하다.
- 통로 depth를 층 번호 기반에서 방 depth 또는 별도 corridor depth 규칙으로 바꿀 때 어떤 데이터 에셋에 노출할지 결정이 필요하다.
