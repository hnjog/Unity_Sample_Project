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
        // Map : 그리드 방식
        // - 클라이언트에서라면 물리법칙을 이용할 수 있으나
        // 서버에서는 매우 부하가 큰 작업이기에 보통 MMO 등에서는 이러한 Grid 방식을 통해
        // 이동과 길찾기 등을 고려한다 
        //
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

    private static Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/@Resources/Data/JsonData/{path}.json");
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
#endif
}
