using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Event = Spine.Event;

// 직접 사용할 클래스가 아니라 스킬 클래스 들의 부모 클래스
public abstract class SkillBase : InitBase
{
    // 스킬 사용자
    public Creature Owner { get; protected set; }
    // 쿨타임
    public float RemainCoolTime { get; set; }

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
            Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
            Owner.SkeletonAnim.AnimationState.Event += OnOwnerAnimEventHandler;
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
        Owner.SkeletonAnim.AnimationState.Event -= OnOwnerAnimEventHandler;
    }

    public virtual void DoSkill()
    {
        // 여기서 쿨타임 관리?
        RemainCoolTime = SkillData.CoolTime;

        // 자신을 준비된 스킬에서 해제 시킨다
        if (Owner.Skills != null)
            Owner.Skills.ActiveSkills.Remove(this);

        // 공속??
        float timeScale = 1.0f;
        
        if (Owner.Skills.SkillList[(int)Define.ESkillSlot.Default] == this)
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = timeScale;
        else
            Owner.PlayAnimation(0, SkillData.AnimName, false).TimeScale = 1;

        // 쿨타임 코루틴
        StartCoroutine(CoCountdownCooldown());
    }

    private IEnumerator CoCountdownCooldown()
    {
        RemainCoolTime = SkillData.CoolTime;
        yield return new WaitForSeconds(SkillData.CoolTime);
        RemainCoolTime = 0;

        // 준비된 스킬에 추가
        if (Owner.Skills != null)
            Owner.Skills.ActiveSkills.Add(this);
    }

    public virtual void CancelSkill()
    {

    }

    protected virtual void GenerateProjectile(Creature owner, Vector3 spawnPos)
    {
        Projectile projectile = Managers.Object.Spawn<Projectile>(spawnPos, SkillData.ProjectileId);

        LayerMask excludeMask = 0;
        excludeMask.AddLayer(Define.ELayer.Default);
        excludeMask.AddLayer(Define.ELayer.Projectile);
        excludeMask.AddLayer(Define.ELayer.Env);
        excludeMask.AddLayer(Define.ELayer.Obstacle);

        switch(owner.ObjectType)
        {
            case Define.EObjectType.Hero:
                excludeMask.AddLayer(Define.ELayer.Hero);
                break;
            case Define.EObjectType.Monster:
                excludeMask.AddLayer(Define.ELayer.Monster);
                break;
        }

        projectile.SetSpawnInfo(Owner, this, excludeMask);
    }

    private void OnOwnerAnimEventHandler(TrackEntry trackEntry, Event e)
    {
        // 다른스킬의 애니메이션 이벤트도 받기 때문에 자기꺼만 써야함
        if (trackEntry.Animation.Name == SkillData.AnimName)
            OnAttackEvent();
    }

    protected abstract void OnAttackEvent();

    public virtual void GenerateAoE(Vector3 spawnPos)
    {
        AoEBase aoe = null;
        int id = SkillData.AoEId;
        string className = Managers.Data.AoEDic[id].ClassName;

        Type componentType = Type.GetType(className);

        if (componentType == null)
        {
            Debug.LogError("AoE Type not found: " + className);
            return;
        }

        GameObject go = Managers.Object.SpawnGameObject(spawnPos, "AoE");
        go.name = Managers.Data.AoEDic[id].ClassName;
        aoe = go.AddComponent(componentType) as AoEBase;
        aoe.SetInfo(SkillData.AoEId, Owner, this);
    }
}
