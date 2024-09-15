using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;

public class Creature : BaseObject
{
    public Data.CreatureData CreatureData { get; protected set; }

    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;

    #region Stats
    public float Hp { get; set; }
    public float MaxHp { get; set; }
    public float MaxHpBonusRate { get; set; }
    public float HealBonusRate { get; set; }
    public float HpRegen { get; set; }
    public float Atk { get; set; }
    public float AttackRate { get; set; }
    public float Def { get; set; }
    public float DefRate { get; set; }
    public float CriRate { get; set; }
    public float CriDamage { get; set; }
    public float DamageReduction { get; set; }
    public float MoveSpeedRate { get; set; }
    public float MoveSpeed { get; set; }
    #endregion

    protected ECreatureState _creatureState = ECreatureState.None;
    // 하위 크리쳐에서
    // 오버라이드 하여, 처리를 바꿀 수 있음
    public virtual ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                _creatureState = value;
                // 이전 상태와 다른 상태라면 애니메이션 변경
                UpdateAnimation();
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Creature;
        // 상태 변경 시, Spine 애니메이션이 변경되나
        // 현재 시점에선 SetInfo가 호출되지 않으므로 크래시가 난다
        // 따라서 SetInfo로 상태 변경을 옮김
        return true;
    }

    public virtual void SetInfo(int templateID)
    {
        DataTemplateID = templateID;
        CreatureData = Managers.Data.CreatureDic[templateID];
        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        //Collider
        Collider.offset = new Vector2 (CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.radius = CreatureData.ColliderRadius;

        // RigidBody
        RigidBody.mass = CreatureData.Mass;

        // Spine
        // 스켈레톤에 따라 프리팹을 별도로 만드는 방식도 존재
        // 여기서는 프리팹 수를 줄이기 위해 바꿔끼는 식
        SkeletonAnim.skeletonDataAsset = Managers.Resource.Load<SkeletonDataAsset>(CreatureData.SkeletonDataID);
        SkeletonAnim.Initialize(true);

        // Register AnimEvent
        if (SkeletonAnim.AnimationState != null)
        {
            SkeletonAnim.AnimationState.Event -= OnAnimEventHandler;
            SkeletonAnim.AnimationState.Event += OnAnimEventHandler;
        }

        // Spine SkeletonAnimation은 SpriteRenderer 를 사용하지 않고 MeshRenderer을 사용함.
        // 그렇기떄문에 2D Sort Axis가 안먹히게 되는데 SortingGroup을 SpriteRenderer, MeshRenderer을 같이 계산하여
        // 그릴 순서를 정해준다
        SortingGroup sg = Util.GetOrAddComponent<SortingGroup>(gameObject);
        sg.sortingOrder = SortingLayers.CREATURE;

        // Skills
        // CreatureData.SkillIdList;

        // Stat
        MaxHp = CreatureData.MaxHp;
        Hp = CreatureData.MaxHp;
        Atk = CreatureData.MaxHp;
        MaxHp = CreatureData.MaxHp;
        MoveSpeed = CreatureData.MoveSpeed;

        // State
        CreatureState = ECreatureState.Idle;
    }

    protected override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case ECreatureState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;
            case ECreatureState.Skill:
                PlayAnimation(0, AnimName.ATTACK_A, true);
                break;
            case ECreatureState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;
            case ECreatureState.Dead:
                PlayAnimation(0, AnimName.DEAD, true);
                RigidBody.simulated = false;
                break;
            default:
                break;
        }
    }

    public void ChangeColliderSize(EColliderSize size = EColliderSize.Normal)
    {
        switch (size)
        {
            case EColliderSize.Small:
                Collider.radius = CreatureData.ColliderRadius * 0.8f;
                break;
            case EColliderSize.Normal:
                Collider.radius = CreatureData.ColliderRadius;
                break;
            case EColliderSize.Big:
                Collider.radius = CreatureData.ColliderRadius * 1.2f;
                break;
        }
    }

    #region AI
    public float UpdateAITick { get; protected set; } = 0.0f;

    // 코루틴을 사용한 AI 관리 방식
    // 몬스터 개수가 많아지는 경우, 코루틴의 성능 이슈가 존재할 수 있기에
    // Update 혹은 상태 변화에 따른 이벤트 등의 방식도 고려 가능
    protected IEnumerator CoUpdateAI()
    {
        while (true)
        {
            switch (CreatureState)
            {
                case ECreatureState.Idle:
                    UpdateIdle();
                    break;
                case ECreatureState.Move:
                    UpdateMove();
                    break;
                case ECreatureState.Skill:
                    UpdateSkill();
                    break;
                case ECreatureState.Dead:
                    UpdateDead();
                    break;
            }

            // 다음 프레인에 다시 코루틴 실행
            if (UpdateAITick > 0)
                yield return new WaitForSeconds(UpdateAITick);
            // 다음 프레임까지 대기하고 다시 실행
            else
                yield return null;
        }
    }

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMove() { }
    protected virtual void UpdateSkill() { }
    protected virtual void UpdateDead() { }
    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker)
    {
        base.OnDamaged(attacker);

        if (attacker.IsValid() == false)
            return;

        Creature creature = attacker as Creature;
        if (creature == null)
            return;

        float finalDamage = creature.Atk; // TODO
        Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp);

        if (Hp <= 0)
        {
            OnDead(attacker);
            CreatureState = ECreatureState.Dead;
        }
    }

    public override void OnDead(BaseObject attacker)
    {
        base.OnDead(attacker);
    }
    #endregion

    #region Wait
    // 이 방식 말고도 float 용 변수를 따로 빼는 방식도 존재함
    protected Coroutine _coWait;

    protected void StartWait(float seconds)
    {
        // 이미 기다리는 경우 취소시킴
        CancelWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

    // 지정 시간을 대기하는 코루틴
    IEnumerator CoWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        _coWait = null;
    }

    protected void CancelWait()
    {
        if (_coWait != null)
            StopCoroutine(_coWait);
        _coWait = null;
    }
    #endregion
}