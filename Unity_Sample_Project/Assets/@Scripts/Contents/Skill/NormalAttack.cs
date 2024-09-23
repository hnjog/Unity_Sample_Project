using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttack : SkillBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public override void SetInfo(Creature owner, int skillTemplateID)
    {
        base.SetInfo(owner, skillTemplateID);
    }

    public override void DoSkill()
    {
        base.DoSkill();

        Owner.CreatureState = Define.ECreatureState.Skill;
        Owner.PlayAnimation(0, SkillData.AnimName, false);

        Owner.LookAtTarget(Owner.Target);
    }
    protected override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        if(e.ToString().Contains(SkillData.AnimName))
        {
            OnAttackEvent();
        }
    }

    protected virtual void OnAttackEvent()
    {
        if (Owner.Target.IsValid() == false)
            return;

        if (SkillData.ProjectileId == 0)
        {
            // Melee
            Owner.Target.OnDamaged(Owner, this);
        }
        else
        {
            // Ranged
            GenerateProjectile(Owner, Owner.CenterPosition);
        }
    }
    protected override void OnAnimCompleteHandler(TrackEntry trackEntry)
    {
        if (Owner.Target.IsValid() == false)
            return;

        // 애니메이션 해제 시,
        // 여전히 Skill 상태라면 강제로 이동 상태로 바꾸어준다
        // 이미 Skill 상태가 끝난 경우에는 문제가 없으나,
        // 그렇지 않은 경우는 ForceMove 등으로 상태가 캔슬된 경우를 대비
        if (Owner.CreatureState == Define.ECreatureState.Skill)
            Owner.CreatureState = Define.ECreatureState.Move;
    }
    
}
