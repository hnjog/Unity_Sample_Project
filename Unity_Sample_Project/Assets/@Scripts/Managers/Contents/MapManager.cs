using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;

public class MapManager
{
    // Map : 그리드 방식
    // - 클라이언트에서라면 물리법칙을 이용할 수 있으나
    // 서버에서는 매우 부하가 큰 작업이기에 보통 MMO 등에서는 이러한 Grid 방식을 통해
    // 이동과 길찾기 등을 고려한다 
    // 그렇다고 Collider 등은 안쓰는 것은 아니다 (스킬 판정 체크 등에 유용)

    public GameObject Map { get; private set; }
    public string MapName { get; private set; }
    public Grid CellGrid { get; private set; }

    // Grid 내부의 좌표와 월드 좌표를 매핑
    // 여러 개의 오브젝트를 넣을 생각이라면, 별도의 클래스를 파는 것이 좋을 수 있다
    Dictionary<Vector3Int, BaseObject> _cells = new Dictionary<Vector3Int, BaseObject>();

    // 현재 맵에 대한 정보 (좌표)
    private int MinX;
    private int MaxX;
    private int MinY;
    private int MaxY;

    public Vector3Int World2Cell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }
    public Vector3 Cell2World(Vector3Int cellPos) { return CellGrid.CellToWorld(cellPos); }

    ECellCollisionType[,] _collision;

    public void LoadMap(string mapName)
    {
        DestroyMap();

        GameObject map = Managers.Resource.Instantiate(mapName);
        map.transform.position = Vector3.zero;
        map.name = $"@Map_{mapName}";

        Map = map;
        MapName = mapName;
        CellGrid = map.GetComponent<Grid>();

        ParseCollisionData(map, mapName);

        SpawnObjectsByData(map, mapName);
    }

    // 다른 맵 이동 시, 현재 데이터를 지우는 용도
    public void DestroyMap()
    {
        ClearObjects();

        if (Map != null)
            Managers.Resource.Destroy(Map);
    }

    void ParseCollisionData(GameObject map, string mapName, string tilemap = "Tilemap_Collision")
    {
        // Tilemap_Collision 이 켜져있으면 매우 이상하게 보일 수 있기에
        // 혹시 켜져있다면 꺼준다
        GameObject collision = Util.FindChild(map, tilemap, true);
        if (collision != null)
            collision.SetActive(false);

        // Collision 관련 파일을 읽어와
        // 해당 파일의 맵 정보를 읽는다
        TextAsset txt = Managers.Resource.Load<TextAsset>($"{mapName}Collision");
        StringReader reader = new StringReader(txt.text);

        MinX = int.Parse(reader.ReadLine());
        MaxX = int.Parse(reader.ReadLine());
        MinY = int.Parse(reader.ReadLine());
        MaxY = int.Parse(reader.ReadLine());

        int xCount = MaxX - MinX + 1;
        int yCount = MaxY - MinY + 1;
        _collision = new ECellCollisionType[xCount, yCount];

        for (int y = 0; y < yCount; y++)
        {
            string line = reader.ReadLine();
            for (int x = 0; x < xCount; x++)
            {
                switch (line[x])
                {
                    case Define.MAP_TOOL_WALL:
                        _collision[x, y] = ECellCollisionType.Wall;
                        break;
                    case Define.MAP_TOOL_NONE:
                        _collision[x, y] = ECellCollisionType.None;
                        break;
                    case Define.MAP_TOOL_SEMI_WALL:
                        _collision[x, y] = ECellCollisionType.SemiWall;
                        break;
                }
            }
        }
    }

    void SpawnObjectsByData(GameObject map, string mapName, string tilemap = "Tilemap_Object")
    {
        Tilemap tm = Util.FindChild<Tilemap>(map, tilemap, true);

        // 일단 꺼두기
        if (tm != null)
            tm.gameObject.SetActive(false);

        // 한칸 한칸 검사
        for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
        {
            for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                CustomTile tile = tm.GetTile(cellPos) as CustomTile;
                if (tile == null)
                    continue;

                if (tile.ObjectType == Define.EObjectType.Env)
                {
                    Vector3 worldPos = Cell2World(cellPos);
                    Env env = Managers.Object.Spawn<Env>(worldPos, tile.DataTemplateID);
                    env.SetCellPos(cellPos, true);
                }
                else
                {
                    if (tile.CreatureType == Define.ECreatureType.Monster)
                    {
                        Vector3 worldPos = Cell2World(cellPos);
                        Monster monster = Managers.Object.Spawn<Monster>(worldPos, tile.DataTemplateID);
                        monster.SetCellPos(cellPos, true);
                    }
                    else if (tile.CreatureType == Define.ECreatureType.Npc)
                    {
                        // To do
                    }
                }
            }
        }
    }

    public bool MoveTo(Creature obj, Vector3Int cellPos, bool forceMove = false)
    {
        if (CanGo(cellPos) == false)
            return false;

        // 기존 좌표에 있던 오브젝트를 밀어준다.
        // (이전 위치에서 제거)
        // (단, 처음 신청했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음)
        RemoveObject(obj);

        // 새 좌표에 오브젝트를 등록한다.
        AddObject(obj, cellPos);

        // 셀 좌표 이동
        obj.SetCellPos(cellPos, forceMove);

        //Debug.Log($"Move To {cellPos}");

        return true;
    }

    #region Helpers
    public BaseObject GetObject(Vector3Int cellPos)
    {
        // 없으면 null
        _cells.TryGetValue(cellPos, out BaseObject value);
        return value;
    }

    public BaseObject GetObject(Vector3 worldPos)
    {
        Vector3Int cellPos = World2Cell(worldPos);
        return GetObject(cellPos);
    }

    // 딕셔너리에서 물체 삭제
    public bool RemoveObject(BaseObject obj)
    {
        BaseObject prev = GetObject(obj.CellPos);

        // 처음 신청했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음
        if (prev != obj)
            return false;

        _cells[obj.CellPos] = null;
        return true;
    }

    public bool AddObject(BaseObject obj, Vector3Int cellPos)
    {
        // 그 위치에 갈 수 있나?
        if (CanGo(cellPos) == false)
        {
            Debug.LogWarning($"AddObject Failed");
            return false;
        }

        // 해당 위치에 누가 있나?
        BaseObject prev = GetObject(cellPos);
        if (prev != null)
        {
            Debug.LogWarning($"AddObject Failed");
            return false;
        }

        _cells[cellPos] = obj;
        return true;
    }

    public bool CanGo(Vector3 worldPos, bool ignoreObjects = false, bool ignoreSemiWall = false)
    {
        return CanGo(World2Cell(worldPos), ignoreObjects, ignoreSemiWall);
    }

    // 해당 위치에 갈 수 있는지를 체크
    public bool CanGo(Vector3Int cellPos, bool ignoreObjects = false, bool ignoreSemiWall = false)
    {
        // 해당 범위 체크
        if (cellPos.x < MinX || cellPos.x > MaxX)
            return false;
        if (cellPos.y < MinY || cellPos.y > MaxY)
            return false;

        // 물체 무시하는게 아니라면, 해당 위치에 물체가 있는지 체크
        if (ignoreObjects == false)
        {
            BaseObject obj = GetObject(cellPos);
            if (obj != null)
                return false;
        }

        int x = cellPos.x - MinX;
        int y = MaxY - cellPos.y;
        ECellCollisionType type = _collision[x, y];
        if (type == ECellCollisionType.None)
            return true;

        if (ignoreSemiWall && type == ECellCollisionType.SemiWall)
            return true;

        return false;
    }

    public void ClearObjects()
    {
        _cells.Clear();
    }

    #endregion

    // BFS vs 다익스트라 vs A*
    // BFS : 너비 우선 탐색
    // 일반적인 최소 거리 탐색법
    // 시작점에서 모든 탐색지를 탐색하여 목적지를 찾는 방식
    // 하위의 다익스트라, A* 역시 BFS의 변형이다
    //
    // 다익스트라 
    // : 그래프 탐색 알고리즘 중 하나,(탐욕)
    // '현재까지 알려진 최단 경로 중' 가중치가 작은 노드를 '우선적'으로 방문하는 방식
    // 우선적으로 탐색하기에 '우선순위 큐'가 사용된다
    //
    // A* : 다익스트라의 변형으로 '휴리스틱' 정보를 통하여 효율을 높임
    // 휴리스틱으로 '노드'에 대한 '가중치'를 비교하는 것이 가장 큰 특징이다
    // (일반적인 다익스트라는 각 노드의 가중치가 일정한 편이다)
    //
    // '한 번에 길을 찾는 경우'
    // 알고보니 해당 위치가 '막혀 있는 경우'
    // 거의 모든 경우에 대한 탐색을 하기에
    // BFS와 다를 게 없음
    //
    // 다만 알고보니 멀리 돌아가는 길이 있었다던가 하는 경우는,
    // 이 경우가 '올바른 경로'를 찾을 수 있었으므로 유용함
    //
    // 반대로 '여러 번 나누어 찾는 경우'
    // 멀리 돌아갈 때, 올바른 경로를 찾을 수 없어서 벽에 박힐 수 있지만
    // 연산 시간을 절약할 수 있음

    #region A* PathFinding
    public struct PQNode : IComparable<PQNode>
    {
        public int H; // Heuristic (목적지 까지의 거리)
        public Vector3Int CellPos;
        public int Depth; // 도착할때까지 건너온 칸 수

        public int CompareTo(PQNode other)
        {
            if (H == other.H)
                return 0;
            return H < other.H ? 1 : -1;
        }
    }

    // 가중치 (현재는 8 방향 모두 동일하게 설정)
    List<Vector3Int> _delta = new List<Vector3Int>()
    {
        new Vector3Int(0, 1, 0), // U
		new Vector3Int(1, 1, 0), // UR
		new Vector3Int(1, 0, 0), // R
		new Vector3Int(1, -1, 0), // DR
		new Vector3Int(0, -1, 0), // D
		new Vector3Int(-1, -1, 0), // LD
		new Vector3Int(-1, 0, 0), // L
		new Vector3Int(-1, 1, 0), // LU
	};

    public List<Vector3Int> FindPath(Vector3Int startCellPos, Vector3Int destCellPos, int maxDepth = 10)
    {
        // 지금까지 제일 좋은 후보 기록.
        Dictionary<Vector3Int, int> best = new Dictionary<Vector3Int, int>();

        // 경로 추적 용도.
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

        // 현재 발견된 후보 중에서 가장 좋은 후보를 빠르게 뽑아오기 위한 도구.
        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>(); // OpenList

        Vector3Int pos = startCellPos;
        Vector3Int dest = destCellPos;

        // destCellPos에 도착 못하더라도 제일 가까운 애로.
        Vector3Int closestCellPos = startCellPos;
        int closestH = (dest - pos).sqrMagnitude;

        // 시작점 발견 (예약 진행)
        {
            // 휴리스틱 비용 계산 (벡터의 크기)
            int h = (dest - pos).sqrMagnitude;
            pq.Push(new PQNode() { H = h, CellPos = pos, Depth = 1 });
            parent[pos] = pos; // 시작점은 부모가 자기 자신 (판별 조건)
            best[pos] = h;
        }

        // 사실 이 부분은 다익스트라 와 비슷하다
        while (pq.Count > 0)
        {
            // 제일 좋은 후보를 찾는다
            PQNode node = pq.Pop();
            pos = node.CellPos;

            // 목적지 도착했으면 바로 종료.
            if (pos == dest)
                break;

            // 무한으로 깊이 들어가진 않음.
            if (node.Depth >= maxDepth)
                break;

            // 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약한다.
            foreach (Vector3Int delta in _delta)
            {
                Vector3Int next = pos + delta;

                // 갈 수 없는 장소면 스킵.
                if (CanGo(next) == false)
                    continue;

                // 예약 진행
                int h = (dest - next).sqrMagnitude;

                // 안에 값 없다면 최댓값으로 만들어준다
                if (best.ContainsKey(next) == false)
                    best[next] = int.MaxValue;

                // 이미 더 좋은 값이 있다면 패스
                if (best[next] <= h)
                    continue;

                best[next] = h;

                pq.Push(new PQNode() { H = h, CellPos = next, Depth = node.Depth + 1 });
                parent[next] = pos;

                // 목적지까지는 못 가더라도, 그나마 제일 좋았던 후보 기억
                if (closestH > h)
                {
                    closestH = h;
                    closestCellPos = next;
                }
            }
        }

        // 목적지에 도달 못했으므로
        // 제일 가까운 애라도 찾음
        if (parent.ContainsKey(dest) == false)
            return CalcCellPathFromParent(parent, closestCellPos);

        return CalcCellPathFromParent(parent, dest);
    }

    List<Vector3Int> CalcCellPathFromParent(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int dest)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        if (parent.ContainsKey(dest) == false)
            return cells;

        Vector3Int now = dest;

        // 거슬러 올라간다
        while (parent[now] != now)
        {
            cells.Add(now);
            now = parent[now];
        }

        cells.Add(now);
        cells.Reverse();

        return cells;
    }

    #endregion
}
