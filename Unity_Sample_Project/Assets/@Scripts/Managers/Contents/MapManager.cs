using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
}
