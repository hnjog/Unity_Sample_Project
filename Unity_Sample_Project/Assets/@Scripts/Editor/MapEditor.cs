using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
#endif

public class MapEditor : MonoBehaviour
{
#if UNITY_EDITOR
    // % (Ctrl), # (Shift), & (Alt)
    [MenuItem("Tools/GenerateMap %#m")]
    private static void GenerateMap()
    {
        // 선택된 오브젝트 대상으로
        GameObject[] gameObjects = Selection.gameObjects;

        foreach (GameObject go in gameObjects)
        {
            Tilemap tm = Util.FindChild<Tilemap>(go, "Tilemap_Collision", true);

            // 클라와 서버가 같이 사용할 파일
            // using 문의 역할
            // - 자원 자동 해제 (IDisposable 인터페이스 가 구현된 객체 사용시 유용)
            // 해당 블록이 끝날 때, Dispose가 호출되어 자원을 해제 한다
            using (var writer = File.CreateText($"Assets/@Resources/Data/MapData/{go.name}Collision.txt"))
            {
                writer.WriteLine(tm.cellBounds.xMin);
                writer.WriteLine(tm.cellBounds.xMax);
                writer.WriteLine(tm.cellBounds.yMin);
                writer.WriteLine(tm.cellBounds.yMax);

                for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
                {
                    for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            if (tile.name.Contains("O"))
                                writer.Write(Define.MAP_TOOL_NONE);
                            else
                                writer.Write(Define.MAP_TOOL_SEMI_WALL);
                        }
                        else
                            writer.Write(Define.MAP_TOOL_WALL);
                    }
                    writer.WriteLine();
                }
            }
        }

        Debug.Log("Map Collision Generation Complete");
    }

    // ctrl + shift + o
    [MenuItem("Tools/Create Object Tile Asset %#o")]
    public static void CreateObjectTile()
    {
        // Monster
        Dictionary<int, Data.MonsterData> MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        foreach (var data in MonsterDic.Values)
        {
            // Unity 에디터에서 사용할 수 있는 데이터 객체 생성
            // ScriptableObject는 인스펙터 창 등에서 데이터 객체를 만들기 위해 사용
            // (에셋 저장도 가능)
            CustomTile customTile = ScriptableObject.CreateInstance<CustomTile>();
            customTile.Name = data.DescriptionTextID;
            customTile.DataTemplateID = data.DataId;
            customTile.ObjectType = Define.EObjectType.Monster;

            string name = $"{data.DataId}_{data.DescriptionTextID}";
            string path = "Assets/@Resources/TileMaps/Tiles/Dev/Monster";
            path = Path.Combine(path, $"{name}.Asset");

            if (File.Exists(path))
                continue;

            AssetDatabase.CreateAsset(customTile, path);
        }

        // Env
        Dictionary<int, Data.EnvData> Env = LoadJson<Data.EnvDataLoader, int, Data.EnvData>("EnvData").MakeDict();
        foreach (var data in Env.Values)
        {

            CustomTile customTile = ScriptableObject.CreateInstance<CustomTile>();
            customTile.Name = data.DescriptionTextID;
            customTile.DataTemplateID = data.DataId;
            customTile.ObjectType = Define.EObjectType.Env;

            string name = $"{data.DataId}_{data.DescriptionTextID}";
            string path = "Assets/@Resources/TileMaps/Tiles/Dev/Env";
            path = Path.Combine(path, $"{name}.Asset");

            if (File.Exists(path))
                continue;

            AssetDatabase.CreateAsset(customTile, path);
        }
    }

    private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/@Resources/Data/JsonData/{path}.json");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
#endif
}
