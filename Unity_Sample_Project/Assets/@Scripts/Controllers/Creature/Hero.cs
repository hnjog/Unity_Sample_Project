using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Define;
using Spine;

public class Hero : Creature
{
    bool _needArrange = true;
    public bool NeedArrange
    {
        get { return _needArrange; }
        set
        {
            _needArrange = value;

            if (value)
                ChangeColliderSize(EColliderSize.Big);
            else
                TryResizeCollider();
        }
    }

    public override ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;

                if (value == ECreatureState.Move)
                    RigidBody.mass = CreatureData.Mass;
                else
                    RigidBody.mass = CreatureData.Mass * 0.1f;
            }
        }
    }


    EHeroMoveState _heroMoveState = EHeroMoveState.None;
    public EHeroMoveState HeroMoveState
    {
        get { return _heroMoveState; }
        private set
        {
            _heroMoveState = value;
            switch (value)
            {
                case EHeroMoveState.CollectEnv:
                    NeedArrange = true;
                    break;
                case EHeroMoveState.TargetMonster:
                    NeedArrange = true;
                    break;
                case EHeroMoveState.ForceMove:
                    NeedArrange = true;
                    break;
            }
        }
    }
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        CreatureType = ECreatureType.Hero;

        // Game의 이벤트 구독하기 위함
        // + 만 해도 되지만, 혹시나 2번 호출하는 경우를 대비하여 미리 한번 빼준다
        Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
        Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        // State
        CreatureState = ECreatureState.Idle;
    }

    public Transform HeroCampDest
    {
        get
        {
            HeroCamp camp = Managers.Object.Camp;
            if (HeroMoveState == EHeroMoveState.ReturnToCamp)
                return camp.Pivot;

            return camp.Destination;
        }
    }

    #region AI
    public float SearchDistance { get; private set; } = 8.0f;
    public float AttackDistance
    {
        get
        {
            float targetRadius = (_target.IsValid() ? _target.ColliderRadius : 0);
            return ColliderRadius + targetRadius + 2.0f;
        }
    }

    public float StopDistance { get; private set; } = 1.0f;

    BaseObject _target;
    protected override void UpdateIdle()
    {
        // 우선순위
        // 1. 커서 이동 시 강제로 상태를 변경해야 함 + 너무 멀리간 경우도 포함
        if(HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        // 2. 몬스터 사냥 하기
        Creature creature = FindClosestInRange(SearchDistance, Managers.Object.Monsters) as Creature;
        if (creature != null)
        {
            _target = creature;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.TargetMonster;
            return;
        }

        // 3. 주변 Env 채집
        Env env = FindClosestInRange(SearchDistance, Managers.Object.Envs) as Env;
        if (env != null)
        {
            _target = env;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.CollectEnv;
            return;
        }

        // 4. Camp 주변으로 모이기
        if (NeedArrange)
        {
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.ReturnToCamp;
            return;
        }
    }
    protected override void UpdateMove()
    {
        // 1. 커서 이동 시 강제로 상태를 변경
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            SetRigidBodyVelocity(dir.normalized * MoveSpeed);
            return;
        }

        // 2. 몬스터 사냥 하기
        if (HeroMoveState == EHeroMoveState.TargetMonster)
        {
            // 몬스터 죽었으면 포기.
            if (_target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(AttackDistance, SearchDistance);
            return;
        }

        // 3. 주변 Env 채집
        if (HeroMoveState == EHeroMoveState.CollectEnv)
        {
            // 몬스터가 있으면 포기.
            Creature creature = FindClosestInRange(SearchDistance, Managers.Object.Monsters) as Creature;
            if (creature != null)
            {
                _target = creature;
                HeroMoveState = EHeroMoveState.TargetMonster;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Env 이미 채집했으면 포기.
            if (_target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            ChaseOrAttackTarget(AttackDistance, SearchDistance);
            return;
        }

        // 4. 캠프 주변으로 소집
        if (HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
            Vector3 dir = HeroCampDest.position - transform.position;
            float stopDistanceSqr = StopDistance * StopDistance;
            if (dir.sqrMagnitude <= StopDistance)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Idle;
                NeedArrange = false;
                return;
            }
            else
            {
                // 멀리 있을 수록 빨라짐
                float ratio = Mathf.Min(1, dir.magnitude); // TEMP
                float moveSpeed = MoveSpeed * (float)Math.Pow(ratio, 3);
                SetRigidBodyVelocity(dir.normalized * moveSpeed);
                return;
            }
        }

        // 4. 기타 (누르다 뗐을 때)
        CreatureState = ECreatureState.Idle;
    }
    protected override void UpdateSkill()
    {
        if(HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if (_target.IsValid() == false)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }
    protected override void UpdateDead()
    {

    }

    // IEnumerable : 컬렉션의 요소를 열거할 수 있다고 정의하는 인터페이스
    // (공식 표준 라이브러리에서 제공)
    // 열거자를 제공하여 컬렉션의 요소를 순회할 수 있게 해준다
    // 컬렉션 : 여러 데이터를 관리하는 데이터 구조 (List, Dictionary, HashSet 등등)
    // 열거자 : 컬렉션의 요소를 순회할 수 있도록 해주는 객체
    BaseObject FindClosestInRange(float range, IEnumerable<BaseObject> objs)
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

            target = obj;
            bestDistanceSqr = distToTargetSqr;
        }

        return target;
    }

    void ChaseOrAttackTarget(float attackRange, float chaseRange)
    {
        Vector3 dir = (_target.transform.position - transform.position);
        float distToTargetSqr = dir.sqrMagnitude;
        float attackDistanceSqr = attackRange * attackRange;

        if (distToTargetSqr <= attackDistanceSqr)
        {
            // 공격 범위 이내로 들어왔다면 공격.
            CreatureState = ECreatureState.Skill;
            return;
        }
        else
        {
            // 공격 범위 밖이라면 추적.
            SetRigidBodyVelocity(dir.normalized * MoveSpeed);

            // 너무 멀어지면 포기.
            float searchDistanceSqr = chaseRange * chaseRange;
            if (distToTargetSqr > searchDistanceSqr)
            {
                _target = null;
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
            }
            return;
        }
    }
    #endregion

    private void TryResizeCollider()
    {
        // 일단 충돌체 아주 작게.
        ChangeColliderSize(EColliderSize.Small);

        foreach (var hero in Managers.Object.Heroes)
        {
            if (hero.HeroMoveState == EHeroMoveState.ReturnToCamp)
                return;
        }

        // ReturnToCamp가 한 명도 없으면 콜라이더 조정.
        foreach (var hero in Managers.Object.Heroes)
        {
            // 단 채집이나 전투중이면 스킵.
            if (hero.CreatureState == ECreatureState.Idle)
                hero.ChangeColliderSize(EColliderSize.Big);
        }
    }

    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        switch (joystickState)
        {
            case Define.EJoystickState.PointerDown:
                HeroMoveState = EHeroMoveState.ForceMove;
                break;
            case Define.EJoystickState.Drag:
                HeroMoveState = EHeroMoveState.ForceMove;
                break;
            case Define.EJoystickState.PointerUp:
                HeroMoveState = EHeroMoveState.None;
                break;
            default:
                break;
        }
    }

    public override void OnAnimEventHandler(TrackEntry trackEntry, Spine.Event e)
    {
        base.OnAnimEventHandler(trackEntry, e);

        CreatureState = ECreatureState.Move;

        if (_target.IsValid() == false)
            return;

        // 피해를 받는 입장에서 처리
        _target.OnDamaged(this);
    }
}
