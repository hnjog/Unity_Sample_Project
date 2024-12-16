using Data;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Npc : BaseObject
{
    public NpcData Data { get; set; }

    private SkeletonAnimation _skeletonAnim;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Npc;
        return true;
    }

    public void SetInfo(int dataId)
    {
        Data = Managers.Data.NpcDic[dataId];
        gameObject.name = $"{Data.DataId}_{Data.Name}";

        #region Spine Animation
        SetSpineAnimation(Data.SkeletonDataID, SortingLayers.NPC);
        PlayAnimation(0, AnimName.IDLE, true);
        #endregion

    }
}
