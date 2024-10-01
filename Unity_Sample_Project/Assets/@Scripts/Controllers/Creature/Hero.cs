using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Define;
using Spine;

public class Hero : Creature
{
    public bool NeedArrange { get; set; }

    public override ECreatureState CreatureState
    {
        get { return _creatureState; }
        set
        {
            if (_creatureState != value)
            {
                base.CreatureState = value;
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
                case EHeroMoveState.ForcePath:
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

        // 이제 직접 물리적인 역할을 하지 않기 위하여
        // Map
        Collider.isTrigger = true;
        RigidBody.simulated = false;

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

    protected override void UpdateIdle()
    {
        // 우선순위
        // 1. 커서 이동 시 강제로 상태를 변경해야 함 + 너무 멀리간 경우도 포함
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        // 2. 몬스터 사냥 하기
        Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
        if (creature != null)
        {
            Target = creature;
            CreatureState = ECreatureState.Move;
            HeroMoveState = EHeroMoveState.TargetMonster;
            return;
        }

        // 3. 주변 Env 채집
        Env env = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Envs) as Env;
        if (env != null)
        {
            Target = env;
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
        // 강제로 경로 찾아 이동 중인 상황
        if (HeroMoveState == EHeroMoveState.ForcePath)
        {
            MoveByForcePath();
            return;
        }

        // 길을 못찾는 등에 따라 너무 멀어지는 경우에 대한 처리
        if (CheckHeroCampDistanceAndForcePath())
            return;

        // 1. 커서 이동 시 강제로 상태를 변경
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            EFindPathResult result = FindPathAndMoveToCellPos(HeroCampDest.position, HERO_DEFAULT_MOVE_DEPTH);
            return;
        }

        // 2. 몬스터 사냥 하기
        if (HeroMoveState == EHeroMoveState.TargetMonster)
        {
            // 몬스터 죽었으면 포기.
            if (Target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            SkillBase skill = Skills.GetReadySkill();
            //ChaseOrAttackTarget(AttackDistance, HERO_SEARCH_DISTANCE);
            ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, skill);
            return;
        }

        // 3. 주변 Env 채집
        if (HeroMoveState == EHeroMoveState.CollectEnv)
        {
            // 몬스터가 있으면 포기.
            Creature creature = FindClosestInRange(HERO_SEARCH_DISTANCE, Managers.Object.Monsters) as Creature;
            if (creature != null)
            {
                Target = creature;
                HeroMoveState = EHeroMoveState.TargetMonster;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Env 이미 채집했으면 포기.
            if (Target.IsValid() == false)
            {
                HeroMoveState = EHeroMoveState.None;
                CreatureState = ECreatureState.Move;
                return;
            }

            SkillBase skill = Skills.GetReadySkill();
            ChaseOrAttackTarget(HERO_SEARCH_DISTANCE, skill);
            return;
        }

        // 4. 캠프 주변으로 소집
        if (HeroMoveState == EHeroMoveState.ReturnToCamp)
        {
            Vector3 destPos = HeroCampDest.position;
            if (FindPathAndMoveToCellPos(destPos, HERO_DEFAULT_MOVE_DEPTH) == EFindPathResult.Success)
                return;

            // 실패 사유 검사.
            BaseObject obj = Managers.Map.GetObject(destPos);
            if (obj.IsValid())
            {
                // 내가 그 자리를 차지하고 있다면
                if (obj == this)
                {
                    HeroMoveState = EHeroMoveState.None;
                    NeedArrange = false;
                    return;
                }

                // 다른 영웅이 멈춰있다면.
                Hero hero = obj as Hero;
                if (hero != null && hero.CreatureState == ECreatureState.Idle)
                {
                    HeroMoveState = EHeroMoveState.None;
                    NeedArrange = false;
                    return;
                }
            }
        }

        // 4. 기타 (누르다 뗐을 때)
        if (LerpCellPosCompleted)
            CreatureState = ECreatureState.Idle;
    }

    // 강제 경로
    Queue<Vector3Int> _forcePath = new Queue<Vector3Int>();

    bool CheckHeroCampDistanceAndForcePath()
    {
        // 너무 멀어서 못 간다.
        Vector3 destPos = HeroCampDest.position;
        Vector3Int destCellPos = Managers.Map.World2Cell(destPos);

        // 10칸 이하라면 너무 멀진 않으니까
        if ((CellPos - destCellPos).magnitude <= 10)
            return false;

        // 갈수 없나?
        if (Managers.Map.CanGo(destCellPos, ignoreObjects: true) == false)
            return false;

        // 넓게 잡아서 길 찾아본다
        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destCellPos, 100);

        // 가까이에 있네?
        if (path.Count < 2)
            return false;

        HeroMoveState = EHeroMoveState.ForcePath;

        _forcePath.Clear();
        foreach (var p in path)
        {
            _forcePath.Enqueue(p);
        }
        _forcePath.Dequeue();

        return true;
    }

    void MoveByForcePath()
    {
        // 다 도착했다
        if (_forcePath.Count == 0)
        {
            HeroMoveState = EHeroMoveState.None;
            return;
        }

        // 가장 앞의 요소
        Vector3Int cellPos = _forcePath.Peek();

        // Depth : 한번에 이동할 최대 이동 거리
        // 이동시킨다
        if (MoveToCellPos(cellPos, 2))
        {
            // 이동 성공시 해당 부분은 제거
            _forcePath.Dequeue();
            return;
        }

        // 실패 사유가 영웅이라면.
        Hero hero = Managers.Map.GetObject(cellPos) as Hero;
        if (hero != null && hero.CreatureState == ECreatureState.Idle)
        {
            HeroMoveState = EHeroMoveState.None;
            return;
        }
    }

    protected override void UpdateSkill()
    {
        if (HeroMoveState == EHeroMoveState.ForceMove)
        {
            CreatureState = ECreatureState.Move;
            return;
        }

        if (Target.IsValid() == false)
        {
            CreatureState = ECreatureState.Move;
            return;
        }
    }
    protected override void UpdateDead()
    {
    }
    
    #endregion

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
    }
}
