using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Item
{
    // 메모리에만 존재하는 방식
    // 굳이 스킬처럼 mono 붙여서 컴포넌트로 이용할 필요는 없음
    public ItemSaveData SaveData { get; set; }

    public int InstanceId
    {
        get { return SaveData.InstanceId; }
        set { SaveData.InstanceId = value; }
    }

    public long DbId
    {
        get { return SaveData.DbId; }
    }

    public int DataId
    {
        get { return SaveData.DataId; }
        set { SaveData.DataId = value; }
    }

    public int Count
    {
        get { return SaveData.Count; }
        set { SaveData.Count = value; }
    }

    public int EquipSlot
    {
        get { return SaveData.EquipSlot; }
        set { SaveData.EquipSlot = value; }
    }

    public Data.ItemData TemplateData
    {
        get
        {
            return Managers.Data.ItemDic[DataId];
        }
    }

    public EItemType ItemType { get; private set; }
    public EItemSubType SubType { get; private set; }

    public Item(int dataId)
    {
        DataId = dataId;
        ItemType = TemplateData.Type;
        SubType = TemplateData.SubType;
    }

    public virtual bool Init()
    {
        return true;
    }
}
