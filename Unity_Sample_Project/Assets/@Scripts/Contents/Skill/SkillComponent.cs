using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void SetInfo(Creature owner, List<int> skillTemplateIDs)
    {
        _owner = owner;

        // 받은 데이터 기반으로 현재 게임 오브젝트에 스킬 세팅
        foreach (int skillTemplateID in skillTemplateIDs)
            AddSkill(skillTemplateID);
    }

    public void AddSkill(int skillTemplateID = 0)
    {
        // 데이터 내부에서 스킬 타입을 클래스 이름과 동일하게 맞추어야 한다
        string className = Managers.Data.SkillDic[skillTemplateID].ClassName;

        // 현재 붙어있는 게임 오브젝트에 스킬 컴포넌트를 추가하고
        // 해당 컴포넌트의 클래스(SkillBase)를 반환 (그것을 캐스팅하여 사용)
        SkillBase skill = gameObject.AddComponent(Type.GetType(className)) as SkillBase;
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
