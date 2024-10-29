using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Monster : Creature
{
    public override ECreatureState CreatureState
    {
        get { return base.CreatureState; }
        set
        {
            if (_creatureState != value)
            {
                // 부모 클래스의 Setter 호출
                // UpdateAnimation을 호출하기 위함
                base.CreatureState = value;

                switch (value)
                {
                    // 상태 변화마다 Tick의 기준을 다르게 잡아 반응성을 수정할 수 있음
                    case ECreatureState.Idle:
                        UpdateAITick = 0.5f;
                        break;
                    case ECreatureState.Move:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Skill:
                        UpdateAITick = 0.0f;
                        break;
                    case ECreatureState.Dead:
                        UpdateAITick = 1.0f;
                        break;
                }
            }
        }
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = EObjectType.Monster;

        // AI 작동하도록 코루틴 실행
        StartCoroutine(CoUpdateAI());

        return true;
    }

    public override void SetInfo(int templateID)
    {
        base.SetInfo(templateID);

        CreatureState = ECreatureState.Idle;
    }

    void Start()
    {
        // 생성 시점에서 초기 위치 등록 (Init 은 Awake 에 호출되므로)
        _initPos = transform.position;
    }

    #region AI
    // 나중에 Data로 빼기
    Vector3 _destPos;
    Vector3 _initPos; // 초기 위치

    protected override void UpdateIdle()
    {
        // Patrol
        {
            // 10%의 확률로 정찰하도록
            int patrolPercent = 10;
            int rand = Random.Range(0, 100);
            if (rand <= patrolPercent)
            {
                _destPos = _initPos + new Vector3(Random.Range(-2, 2), Random.Range(-2, 2));
                CreatureState = ECreatureState.Move;
                return;
            }
        }

        // Search Player
        // 범위 내에 플레이어가 들어왔다면
        // 'func:' : 함수 매개변수 전달 문법 (명명된 인수)
        // func<t> : 제네릭 대리자 (함수 포인터와 유사)
        Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
        if (creature != null)
        {
            Target = creature;
            CreatureState = ECreatureState.Move;
            return;
        }
    }

    protected override void UpdateMove()
    {
        // 적 발견의 상태인가
        if (Target.IsValid() == false)
        {
            Creature creature = FindClosestInRange(MONSTER_SEARCH_DISTANCE, Managers.Object.Heroes, func: IsValid) as Creature;
            if (creature != null)
            {
                Target = creature;
                CreatureState = ECreatureState.Move;
                return;
            }

            // Move
            FindPathAndMoveToCellPos(_destPos, MONSTER_DEFAULT_MOVE_DEPTH);

            if (LerpCellPosCompleted)
            {
                CreatureState = ECreatureState.Idle;
                return;
            }
        }
        else
        {
            // Chase
            ChaseOrAttackTarget(MONSTER_SEARCH_DISTANCE, AttackDistance);

            // 너무 멀어지면 포기.
            if (Target.IsValid() == false)
            {
                Target = null;
                _destPos = _initPos;
                return;
            }
        }
    }

    protected override void UpdateSkill()
    {
        base.UpdateSkill();

        // 애니메이션 쪽에서 상태를 바꿔주도록 설정하였으므로
        if(Target.IsValid() == false)
        {
            Target = null;
            _destPos = _initPos;
            CreatureState = ECreatureState.Move;
            return;
        }
    }

    protected override void UpdateDead()
    {
    }
    #endregion

    #region Battle
    public override void OnDamaged(BaseObject attacker, SkillBase skill)
    {
        base.OnDamaged(attacker, skill);

    }

    public override void OnDead(BaseObject attacker, SkillBase skill)
    {
        base.OnDead(attacker, skill);

        Managers.Object.Despawn(this);
    }
    #endregion
}
