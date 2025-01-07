using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    // 솔로 플레이용 Id
    public int ItemDbIdGenerator = 1;
    // 아이템 정보
    public List<ItemSaveData> Items = new List<ItemSaveData>();
}

// 데이터를 저장할 때,
// 스킬 데이터 같은 것은 '가변적'이기에 해당 데이터에 넣어주기 보단
// 차후 만들어질 hero 객체에서 skill 데이터와 조합시키는 것이 더 유연하게 설계할 수 있음

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

[Serializable]
public class ItemSaveData
{
    public int InstanceId;  // DbId(데이터베이스) 말고 인게임에서 해당 아이템을 지정하는 용도의 Id
    public int DbId;        // 데이터 베이스 기준 해당 아이템의 ID(유니크한 id) -> 중복 없이 점점 늘어나기에 오래된 게임일수록 기하급수적으로 커진다
    public int DataId;      // 종류 구분용 Id
    public int Count;
    public int EquipSlot; // 단순히 장착하는 용도 뿐 아니라, 인벤, 창고 등 번호를 통해 분류 가능하다(1~10 / 100 / 300~~)
    // 온라인 게임이라면 owner 같은 소유자 id?? 가 필요할지도
    public int EnchantCount; // 강화 여부
}

public class GameManager
{
    #region GameData
    GameSaveData _saveData = new GameSaveData();
    public GameSaveData SaveData { get { return _saveData; } set { _saveData = value; } }

    public int Wood
    {
        get { return _saveData.Wood; }
        private set
        {
            _saveData.Wood = value;
            //(Managers.UI.SceneUI as UI_GameScene)?.RefreshWoodText();
        }
    }

    public int Mineral
    {
        get { return _saveData.Mineral; }
        private set
        {
            _saveData.Mineral = value;
            //(Managers.UI.SceneUI as UI_GameScene)?.RefreshMineralText();
        }
    }

    public int Meat
    {
        get { return _saveData.Meat; }
        private set
        {
            _saveData.Meat = value;
            //(Managers.UI.SceneUI as UI_GameScene)?.RefreshMeatText();
        }
    }

    public int Gold
    {
        get { return _saveData.Gold; }
        private set
        {
            _saveData.Gold = value;
            //(Managers.UI.SceneUI as UI_GameScene)?.RefreshGoldText();
        }
    }

    public List<HeroSaveData> AllHeroes { get { return _saveData.Heroes; } }
    public int TotalHeroCount { get { return _saveData.Heroes.Count; } }
    // LINQ, 매 프레임 사용할 것은 아니기에 사용
    public int UnownedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == HeroOwningState.Unowned).Count(); } }
    public int OwnedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == HeroOwningState.Owned).Count(); } }
    public int PickedHeroCount { get { return _saveData.Heroes.Where(h => h.OwningState == HeroOwningState.Picked).Count(); } }

    #endregion

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

    #region Save & Load	
    public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }

    public void InitGame()
    {
        // 신규 캐릭터 업데이트 등을 하면
        // 기존 데이터와 신규 데이터가 다르므로 
        // 패치 버전 등등을 고려하는 것이 좋을 수 있음
        if (File.Exists(Path))
            return;

        var heroes = Managers.Data.HeroDic.Values.ToList();
        foreach (HeroData hero in heroes)
        {
            HeroSaveData saveData = new HeroSaveData()
            {
                DataId = hero.DataId,
            };

            SaveData.Heroes.Add(saveData);
        }

        // TEMP
        SaveData.Heroes[0].OwningState = HeroOwningState.Picked;
        SaveData.Heroes[1].OwningState = HeroOwningState.Owned;
    }

    public void SaveGame()
    {
        string jsonStr = JsonUtility.ToJson(Managers.Game.SaveData);
        File.WriteAllText(Path, jsonStr);
        Debug.Log($"Save Game Completed : {Path}");
    }

    public bool LoadGame()
    {
        if (File.Exists(Path) == false)
            return false;

        string fileStr = File.ReadAllText(Path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(fileStr);

        if (data != null)
            Managers.Game.SaveData = data;

        Debug.Log($"Save Game Loaded : {Path}");
        return true;
    }
    #endregion

    #region Action
    // Delegate - 나중에 이동시킬때 한번에 이동시키도록!
    // Hero + Npc (구독자)
    public event Action<Vector2> OnMoveDirChanged;
    public event Action<Define.EJoystickState> OnJoystickStateChanged;
    #endregion
}
