using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class CCBase : EffectBase
{
    protected ECreatureState lastState;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        EffectType = EEffectType.CrowdControl;
        return true;
    }

    public override void ApplyEffect()
    {
        // 부모쪽에서 보여주는 것 적용
        base.ApplyEffect();

        lastState = Owner.CreatureState;
        if (lastState == ECreatureState.OnDamaged)
            return;

        Owner.CreatureState = ECreatureState.OnDamaged;
    }

    // 시간이 지나서 풀린 경우, 마지막 상태로 돌려준다 (일반적으론 Idle)
    public override bool ClearEffect(EEffectClearType clearType)
    {
        if (base.ClearEffect(clearType) == true)
            Owner.CreatureState = lastState;

        return true;
    }
}
