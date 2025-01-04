using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<key,value> // Generic
{
    Dictionary<key,value> MakeDict();

    // Validate() : 예정
    // 런타임 중 데이터를 확인하는 방식이 위험할 수 있으므로
    // 데이터 변질 여부 등을 첫 로딩 때 검사하기 위함
}
public class DataManager
{
    // 필드 겸 프로퍼티 (변수와 속성) 역할을 겸함
    public Dictionary<int, Data.HeroData> HeroDic { get; private set; }
    public Dictionary<int, Data.HeroInfoData> HeroInfoDic { get; private set; }
    public Dictionary<int, Data.MonsterData> MonsterDic { get; private set; }
    public Dictionary<int, Data.SkillData> SkillDic { get; private set; }
    public Dictionary<int, Data.ProjectileData> ProjectileDic { get; private set; }
    public Dictionary<int, Data.EnvData> EnvDic { get; private set; }
    public Dictionary<int, Data.EffectData> EffectDic { get; private set; }
    public Dictionary<int, Data.AoEData> AoEDic { get; private set; }
    public Dictionary<int, Data.NpcData> NpcDic { get; private set; }
    public Dictionary<string, Data.TextData> TextDic { get; private set; } = new Dictionary<string, Data.TextData>();

    public Dictionary<int, Data.EquipmentData> EquipmentDic { get; private set; } = new Dictionary<int, Data.EquipmentData>();
    public Dictionary<int, Data.ConsumableData> ConsumableDic { get; private set; } = new Dictionary<int, Data.ConsumableData>();
    public Dictionary<int, Data.ItemData> ItemDic { get; private set; } = new Dictionary<int, Data.ItemData>();
    public Dictionary<int, Data.DropTableData> DropTableDic { get; private set; } = new Dictionary<int, Data.DropTableData>();

    public void Init()
    {
        HeroDic = LoadJson<Data.HeroDataLoader, int, Data.HeroData>("HeroData").MakeDict();
        HeroInfoDic = LoadJson<Data.HeroInfoDataLoader, int, Data.HeroInfoData>("HeroInfoData").MakeDict();
        MonsterDic = LoadJson<Data.MonsterDataLoader, int, Data.MonsterData>("MonsterData").MakeDict();
        SkillDic = LoadJson<Data.SkillDataLoader, int, Data.SkillData>("SkillData").MakeDict();
        ProjectileDic = LoadJson<Data.ProjectileDataLoader, int, Data.ProjectileData>("ProjectileData").MakeDict();
        EnvDic = LoadJson<Data.EnvDataLoader, int, Data.EnvData>("EnvData").MakeDict();
        EffectDic = LoadJson<Data.EffectDataLoader, int, Data.EffectData>("EffectData").MakeDict();
        AoEDic = LoadJson<Data.AoEDataLoader, int, Data.AoEData>("AoEData").MakeDict();
        NpcDic = LoadJson<Data.NpcDataLoader, int, Data.NpcData>("NpcData").MakeDict();
        TextDic = LoadJson<Data.TextDataLoader, string, Data.TextData>("TextData").MakeDict();
        EquipmentDic = LoadJson<Data.ItemDataLoader<Data.EquipmentData>, int, Data.EquipmentData>("Item_EquipmentData").MakeDict();
        ConsumableDic = LoadJson<Data.ItemDataLoader<Data.ConsumableData>, int, Data.ConsumableData>("Item_ConsumableData").MakeDict();
        DropTableDic = LoadJson<Data.DropTableDataLoader, int, Data.DropTableData>("DropTableData").MakeDict();

        foreach (var item in EquipmentDic)
            ItemDic.Add(item.Key, item.Value);

        foreach (var item in ConsumableDic)
            ItemDic.Add(item.Key, item.Value);
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        // 특정 경로의 Text 에셋
        // Addressable 방식으로 load
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);

        // Newton Json을 convert를 이용하여
        // 해당 클래스 방식으로 캐스팅하여 return 받음
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
