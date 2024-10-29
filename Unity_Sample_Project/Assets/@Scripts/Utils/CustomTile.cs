using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CustomTile : Tile
{
    [Space] // Inspetor에서 필드 사이에 '공간'을 추가하는 역할
    [Space]
    [Header("For Designer")] // 해당 필드 위에 헤더를 추가
    public Define.EObjectType ObjectType;
    public int DataTemplateID;
    public string Name;
}
