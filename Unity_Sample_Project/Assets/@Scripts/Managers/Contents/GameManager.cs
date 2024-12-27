using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// 직렬화 가능 : 데이터를 특정한 형식으로 변환하여 저장하거나 전송 가능하도록 설정
// C/C++에서 메모리의 비트를 '어떻게' 읽을 수 있냐와 관련된 개념이자
// 역직렬화를 통하여 원래 데이터를 읽어오는 방식
[Serializable]
public class GameSaveData
{
    // 자원
    public int Wood = 0;
    public int Mineral = 0;
    public int Meat = 0;
    public int Gold = 0;

    // 영웅정보
    public List<HeroSaveData> Heroes = new List<HeroSaveData>();
}

[Serializable]
public class HeroSaveData
{
    // 데이터 상 영웅의 데이터
    public int DataId = 0;
    public int Level = 1;
    public int Exp = 0;
    // 보유 여부
    public HeroOwningState OwningState = HeroOwningState.Unowned;
}

public enum HeroOwningState
{
    Unowned, // 없음
    Owned,   // 가짐
    Picked,  // 선택해서 사용 중
}

public class GameManager
{
    #region Hero
    // player 뿐 아니라 npc 포함하여 이동시킬 예정이기에
    private Vector2 _moveDir;
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value;
            OnMoveDirChanged?.Invoke(value);
        }
    }

    private Define.EJoystickState _joystickState;
    public Define.EJoystickState JoystickState
    {
        get { return _joystickState; }
        set
        {
            _joystickState = value;
            OnJoystickStateChanged?.Invoke(_joystickState);
        }
    }
    #endregion

    #region Teleport
    public void TeleportHeroes(Vector3 position)
    {
        TeleportHeroes(Managers.Map.World2Cell(position));
    }

    public void TeleportHeroes(Vector3Int cellPos)
    {
        foreach (var hero in Managers.Object.Heroes)
        {
            Vector3Int randCellPos = Managers.Game.GetNearbyPosition(hero, cellPos);
            Managers.Map.MoveTo(hero, randCellPos, forceMove: true);
        }

        Vector3 worldPos = Managers.Map.Cell2World(cellPos);
        Managers.Object.Camp.ForceMove(worldPos);
        Camera.main.transform.position = worldPos;
    }
    #endregion

    #region Helper
    public Vector3Int GetNearbyPosition(BaseObject hero, Vector3Int pivot, int range = 5)
    {
        int x = Random.Range(-range, range);
        int y = Random.Range(-range, range);

        // 랜덤한 빈 위치를 찾도록 try
        for (int i = 0; i < 100; i++)
        {
            Vector3Int randCellPos = pivot + new Vector3Int(x, y, 0);
            if (Managers.Map.CanGo(hero, randCellPos))
                return randCellPos;
        }

        Debug.LogError($"GetNearbyPosition Failed");

        return Vector3Int.zero;
    }
    #endregion

    #region Action
    // Delegate - 나중에 이동시킬때 한번에 이동시키도록!
    // Hero + Npc (구독자)
    public event Action<Vector2> OnMoveDirChanged;
    public event Action<Define.EJoystickState> OnJoystickStateChanged;
    #endregion


}
