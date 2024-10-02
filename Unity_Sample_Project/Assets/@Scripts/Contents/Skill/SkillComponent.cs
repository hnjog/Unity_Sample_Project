using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Define;
using Random = UnityEngine.Random;

public class SkillComponent : InitBase
{
    // 내가 들고 있는 스킬 (여기서 slot Enum을 이용해서 스킬 가져다 사용)
    public List<SkillBase> SkillList { get; } = new List<SkillBase>();

    // 사용 가능한 스킬
    public List<SkillBase> ActiveSkills { get; set; } = new List<SkillBase>();

    public SkillBase CurrentSkill
    {
        get
        {
            if (ActiveSkills.Count == 0)
                return SkillList[(int)ESkillSlot.Default];

            // 사용 가능한 skill 중 랜덤으로
            int randomIndex = Random.Range(0, ActiveSkills.Count);
            return ActiveSkills[randomIndex];
        }
    }

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
        // ID가 잘못된 경우이다
        if (skillTemplateID <= 0)
        {
            Debug.LogWarning($"SkillID Weird : {skillTemplateID}");
            return;
        }

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
        switch (skillSlot)
        {
            case ESkillSlot.A:
                ActiveSkills.Add(skill);
                break;
            case ESkillSlot.B:
                ActiveSkills.Add(skill);
                break;
        }
    }

}
