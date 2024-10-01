using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Define;

public class SkillComponent : InitBase
{
    public List<SkillBase> SkillList { get; } = new List<SkillBase>();

    Creature _owner;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public void SetInfo(Creature owner, CreatureData creatureData)
    {
        _owner = owner;

        // skill 개수 제한
        AddSkill(creatureData.DefaultSkillId, ESkillSlot.Default);
        AddSkill(creatureData.EnvSkillId, ESkillSlot.Env);
        AddSkill(creatureData.SkillAId, ESkillSlot.A);
        AddSkill(creatureData.SkillBId, ESkillSlot.B);
    }

    public void AddSkill(int skillTemplateID , ESkillSlot skillSlot)
    {
        if (skillTemplateID <= 0) 
            return;

        if(Managers.Data.SkillDic.TryGetValue(skillTemplateID, out var data) == false)
        {
            Debug.LogWarning($"AddSkill Failed {skillTemplateID}");
            return;
        }

        // 현재 붙어있는 게임 오브젝트에 스킬 컴포넌트를 추가하고
        // 해당 컴포넌트의 클래스(SkillBase)를 반환 (그것을 캐스팅하여 사용)
        SkillBase skill = gameObject.AddComponent(Type.GetType(data.ClassName)) as SkillBase;
        if (skill == null)
            return;

        skill.SetInfo(_owner, skillTemplateID);

        SkillList.Add(skill);
    }

    public SkillBase GetReadySkill()
    {
        // TEMP
        return SkillList[0];
    }
}
