using Data;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Define;
using static UnityEngine.GraphicsBuffer;

public class Creature : BaseObject
{
    public BaseObject Target {  get; protected set; }
    public SkillComponent Skills { get; protected set; }

    public Data.CreatureData CreatureData { get; protected set; }

    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;

    public EffectComponent Effects { get; set; }

    float DistToTargetSqr
    {
        get
        {
            // 대상 방향
            Vector3 dir = (Target.transform.position - transform.position);

            // 여분 셀 만큼 거리를 줄어든 것으로 판정한다
            float distToTarget = Math.Max(0, dir.magnitude - Target.ExtraCells * 1f - ExtraCells * 1f); // 임시
            return distToTarget * distToTarget;
        }
    }

    #region Stats
    // int 로 만들지 float로 만들지는 보통 lead 플머가 정한다
    // float 장점 : 편리함, 별도의 구현 제약이 없음
    // int 장점 : 속도가 빠름 , 그러나 소수점 등을 계산하려면 int * 10000 같은 크랙 방식을 적용
    public float Hp { get; set; }
    public CreatureStat MaxHp;
    public CreatureStat Atk;
    public CreatureStat CriRate;
    public CreatureStat CriDamage;
    public CreatureStat ReduceDamageRate;
    public CreatureStat LifeStealRate;
    public CreatureStat ThornsDamageRate; // 쏜즈
    public CreatureStat MoveSpeed;
    public CreatureStat AttackSpeedRate;
    #endregion

    protected float AttackDistance
    {
        get
        {
            float env = 2.2f;
            if (Target != null && Target.ObjectType == EObjectType.Env)
                return Mathf.Max(env, Collider.radius + Target.Collider.radius + 0.1f);

            float baseValue = CreatureData.AtkRange;
            return baseValue;
        }
    }

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

        if(CreatureType == ECreatureType.Hero)
            CreatureData = Managers.Data.HeroDic[templateID];
        else
            CreatureData = Managers.Data.MonsterDic[templateID];

        gameObject.name = $"{CreatureData.DataId}_{CreatureData.DescriptionTextID}";

        //Collider
        Collider.offset = new Vector2 (CreatureData.ColliderOffsetX, CreatureData.ColliderOffsetY);
        Collider.radius = CreatureData.ColliderRadius;

        // RigidBody
        RigidBody.mass = 0;

        // Spine
        SetSpineAnimation(CreatureData.SkeletonDataID, SortingLayers.CREATURE);

        // Skills
        Skills = gameObject.GetOrAddComponent<SkillComponent>();
        Skills.SetInfo(this, CreatureData);

        // Stat
        Hp = CreatureData.MaxHp;
        MaxHp = new CreatureStat(CreatureData.MaxHp);
        Atk = new CreatureStat(CreatureData.Atk);
        CriRate = new CreatureStat(CreatureData.CriRate);
        CriDamage = new CreatureStat(CreatureData.CriDamage);
        ReduceDamageRate = new CreatureStat(0);
        LifeStealRate = new CreatureStat(0);
        ThornsDamageRate = new CreatureStat(0);
        MoveSpeed = new CreatureStat(CreatureData.MoveSpeed);
        AttackSpeedRate = new CreatureStat(1);

        // State
        CreatureState = ECreatureState.Idle;

        // Effect
        Effects = gameObject.AddComponent<EffectComponent>();
        Effects.SetInfo(this);

        // Map
        StartCoroutine(CoLerpToCellPos());
    }

    protected override void UpdateAnimation()
    {
        switch (CreatureState)
        {
            case ECreatureState.Idle:
                PlayAnimation(0, AnimName.IDLE, true);
                break;
            case ECreatureState.Skill:
                //PlayAnimation(0, AnimName.ATTACK_A, true);
                break;
            case ECreatureState.Move:
                PlayAnimation(0, AnimName.MOVE, true);
                break;
            case ECreatureState.OnDamaged: // CC기를 맞은 상황
                PlayAnimation(0, AnimName.IDLE, true);
                Skills.CurrentSkill.CancelSkill();
                break;
            case ECreatureState.Dead:
                PlayAnimation(0, AnimName.DEAD, true);
                RigidBody.simulated = false;
                break;
            default:
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
                case ECreatureState.OnDamaged:
                    UpdateOnDamaged();
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
    protected virtual void UpdateSkill() 
    {
        // 스킬 대기중이 아님
        if (_coWait != null)
            return;

        // 타겟이 존재하지 않거나
        // 이상한 타겟이 잡혔으면 무효 처리
        if (Target.IsValid() == false || Target.ObjectType == EObjectType.HeroCamp)
        {
            CreatureState = ECreatureState.Idle;
            return;
        }

        // 캐릭터 공격거리 체크
        float distToTargetSqr = DistToTargetSqr;
        float attackDistanceSqr = AttackDistance * AttackDistance;
        if (distToTargetSqr > attackDistanceSqr)
        {
            // 거리가 너무 멀다
            CreatureState = ECreatureState.Idle;
            return;
        }

        // DoSkill
        Skills.CurrentSkill.DoSkill();

        LookAtTarget(Target);

        var trackEntry = SkeletonAnim.state.GetCurrent(0);
        float delay = trackEntry.Animation.Duration;

        StartWait(delay);
    }
    protected virtual void UpdateOnDamaged() { }

    protected virtual void UpdateDead() { }
    #endregion

    // 스킬 딜레이 체크를 위해 부활
    #region Wait
    protected Coroutine _coWait;

    protected void StartWait(float seconds)
    {
        CancelWait();
        _coWait = StartCoroutine(CoWait(seconds));
    }

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

    #region Battle
    public override void OnDamaged(BaseObject attacker,SkillBase skill)
    {
        base.OnDamaged(attacker, skill);

        if (attacker.IsValid() == false)
            return;

        Creature creature = attacker as Creature;
        if (creature == null)
            return;

        float finalDamage = creature.Atk.Value;
        Hp = Mathf.Clamp(Hp - finalDamage, 0, MaxHp.Value);

        Managers.Object.ShowDamageFont(CenterPosition, finalDamage, transform, false);

        if (Hp <= 0)
        {
            OnDead(attacker, skill);
            CreatureState = ECreatureState.Dead;
            return;
        }

        // 피격 스킬에 따른 Effect 적용
        if (skill.SkillData.EffectIds != null)
            Effects.GenerateEffects(skill.SkillData.EffectIds.ToArray(), EEffectSpawnType.Skill);

    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);
    }

    // IEnumerable : 컬렉션의 요소를 열거할 수 있다고 정의하는 인터페이스
    // (공식 표준 라이브러리에서 제공)
    // 열거자를 제공하여 컬렉션의 요소를 순회할 수 있게 해준다
    // 컬렉션 : 여러 데이터를 관리하는 데이터 구조 (List, Dictionary, HashSet 등등)
    // 열거자 : 컬렉션의 요소를 순회할 수 있도록 해주는 객체
    protected BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs, Func<BaseObject,bool> func = null)
    {
        BaseObject target = null;
        float bestDistanceSqr = float.MaxValue;
        float searchDistanceSqr = range * range;

        foreach (BaseObject obj in objs)
        {
            Vector3 dir = obj.transform.position - transform.position;
            float distToTargetSqr = dir.sqrMagnitude;

            // 서치 범위보다 멀리 있으면 스킵.
            if (distToTargetSqr > searchDistanceSqr)
                continue;

            // 이미 더 좋은 후보를 찾았으면 스킵.
            if (distToTargetSqr > bestDistanceSqr)
                continue;

            // 추가 조건 등으로 다양한 상황에 맞게 사용하도록
            if (func != null && func.Invoke(obj) == false)
                continue;

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    protected void ChaseOrAttackTarget(float chaseRange, float attackRange)
    {
        float distToTargetSqr = DistToTargetSqr;
        float attackDistanceSqr = attackRange * attackRange;

        if (distToTargetSqr <= attackDistanceSqr)
        {
            // 공격 범위 이내로 들어왔다면 공격.
            // 코루틴에서 UpdateSkill 호출
            CreatureState = ECreatureState.Skill;
            return;
        }
        else
        {
            // 공격 범위 밖이라면 추적.
            FindPathAndMoveToCellPos(Target.transform.position, HERO_DEFAULT_MOVE_DEPTH);

            // 너무 멀어지면 포기.
            float searchDistanceSqr = chaseRange * chaseRange;
            if (distToTargetSqr > searchDistanceSqr)
            {
                Target = null;
                CreatureState = ECreatureState.Move;
            }
            return;
        }
    }
    #endregion

    #region Misc
    protected bool IsValid(BaseObject bo)
    {
        return bo.IsValid();
    }
    #endregion

    #region Map
    public EFindPathResult FindPathAndMoveToCellPos(Vector3 destWorldPos, int maxDepth, bool forceMoveCloser = false)
    {
        Vector3Int destCellPos = Managers.Map.World2Cell(destWorldPos);
        return FindPathAndMoveToCellPos(destCellPos, maxDepth, forceMoveCloser);
    }

    public EFindPathResult FindPathAndMoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
    {
        if (LerpCellPosCompleted == false)
            return EFindPathResult.Fail_LerpCell;

        List<Vector3Int> path = Managers.Map.FindPath(this, CellPos, destCellPos, maxDepth);

        // path 크기가 너무 작음(1이하)
        // 제대로된 길이 아님

        if (path.Count < 2)
            return EFindPathResult.Fail_NoPath;

        // 근처라도 괜찮다
        // 목표 근처에서 비비는 경우에 대한 임시 처리용
        if (forceMoveCloser)
        {
            // 현재 ~ 목적지 , 경로 ~ 목적지 를 비교하여
            // 현재~ 목적지 상태가 '다음 경로 ~ 목적지' 보다 가깝다면
            // 더 진행할 필요가 없음
            Vector3Int diff1 = CellPos - destCellPos;
            Vector3Int diff2 = path[1] - destCellPos;
            if (diff1.sqrMagnitude <= diff2.sqrMagnitude)
                return EFindPathResult.Fail_NoPath;
        }

        Vector3Int dirCellPos = path[1] - CellPos;
        Vector3Int nextPos = CellPos + dirCellPos;

        if (Managers.Map.MoveTo(this, nextPos) == false)
            return EFindPathResult.Fail_MoveTo;

        return EFindPathResult.Success;
    }

    public bool MoveToCellPos(Vector3Int destCellPos, int maxDepth, bool forceMoveCloser = false)
    {
        if (LerpCellPosCompleted == false)
            return false;

        return Managers.Map.MoveTo(this, destCellPos);
    }

    // 코루틴을 통한 러프한 이동
    protected IEnumerator CoLerpToCellPos()
    {
        while (true)
        {
            Hero hero = this as Hero;
            if (hero != null)
            {
                float div = 5;
                Vector3 campPos = Managers.Object.Camp.Destination.transform.position;
                Vector3Int campCellPos = Managers.Map.World2Cell(campPos);
                float ratio = Math.Max(1, (CellPos - campCellPos).magnitude / div);

                LerpToCellPos(CreatureData.MoveSpeed * ratio);
            }
            else
                LerpToCellPos(CreatureData.MoveSpeed);

            yield return null;
        }
    }
    #endregion
}