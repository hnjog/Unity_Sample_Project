using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Event = Spine.Event;

// 직접 사용할 클래스가 아니라 스킬 클래스 들의 부모 클래스
public abstract class SkillBase : InitBase
{
    // 스킬 사용자
    public Creature Owner { get; protected set; }

    // 같은 스킬이라도 레벨 등이 다를 수 있으므로
    public Data.SkillData SkillData { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    public virtual void SetInfo(Creature owner, int skillTemplateID)
    {
        Owner = owner;
        SkillData = Managers.Data.SkillDic[skillTemplateID];

        // Register AnimEvent
        if (Owner.SkeletonAnim != null && Owner.SkeletonAnim.AnimationState != null)
        {
            // 스켈레톤 애니메이션 시작, 종료 시에 따른 이벤트 등록(Delegate) - 보통은 Init 말고도 종료시에도 빼주는 것이 안정적
            // 메모리 릭 방지
            Owner.SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
            Owner.SkeletonAnim.AnimationState.Event += OnAnimEventHandler;
            Owner.SkeletonAnim.AnimationState.Complete -= OnAnimCompleteHandler;
            Owner.SkeletonAnim.AnimationState.Complete += OnAnimCompleteHandler;
        }
    }

    // 비활성화 시 호출됨
    private void OnDisable()
    {
        // OnDisable 호출 중 '강제 종료' 등의 상황이라면
        // 이미 다른 개체들이 존재하지 않을 가능성이 있기에
        // if 검사들을 통해 괜히 널 크래시가 나지 않도록 한다
        if (Managers.Game == null)
            return;

        if (Owner.IsValid() == false)
            return;

        if (Owner.SkeletonAnim == null)
            return;

        if (Owner.SkeletonAnim.AnimationState == null)
            return;

        // Delegate 에서 이벤트 제거해준다
        Owner.SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
        Owner.SkeletonAnim.AnimationState.Complete -= OnAnimCompleteHandler;
    }

    public virtual void DoSkill()
    {

    }

    protected abstract void OnAnimEventHandler(TrackEntry trackEntry, Event e);
    protected abstract void OnAnimCompleteHandler(TrackEntry trackEntry);
}
