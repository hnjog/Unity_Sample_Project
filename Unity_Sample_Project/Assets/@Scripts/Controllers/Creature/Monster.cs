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

        CreatureType = ECreatureType.Monster;

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
    public float SearchDistance { get; private set; } = 8.0f;
    public float AttackDistance { get; private set; } = 4.0f;
    Creature _target;
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
        {
            Creature target = null;
            float bestDistanceSqr = float.MaxValue; // 가장 가까이에 있는 target 찾기
            
            float searchDistanceSqr = SearchDistance * SearchDistance;

            // 오브젝트 매니저에서 Player 찾기
            foreach (Hero hero in Managers.Object.Heroes)
            {
                // 방향 벡터 구하기
                Vector3 dir = hero.transform.position - transform.position;
                // 거리의 제곱 : 루트를 씌우지 않아도 거리 비교는 가능하므로
                float distToTargetSqr = dir.sqrMagnitude;

                if (distToTargetSqr > searchDistanceSqr)
                    continue;

                if (distToTargetSqr > bestDistanceSqr)
                    continue;

                target = hero;
                bestDistanceSqr = distToTargetSqr;
            }

            _target = target;

            // 찾았다면 이동
            if (_target != null)
                CreatureState = ECreatureState.Move;
        }
    }

    protected override void UpdateMove()
    {
        // 적 발견의 상태인가
        if (_target == null)
        {
            // Patrol or Return
            Vector3 dir = (_destPos - transform.position);
            if (dir.sqrMagnitude <= 0.01f)
            {
                CreatureState = ECreatureState.Idle;
            }

            SetRigidBodyVelocity(dir.normalized * MoveSpeed);
        }
        else
        {
            // Chase
            Vector3 dir = (_target.transform.position - transform.position);
            float distToTargetSqr = dir.sqrMagnitude;
            float attackDistanceSqr = AttackDistance * AttackDistance;

            if (distToTargetSqr < attackDistanceSqr)
            {
                // 공격 범위 이내로 들어왔으면 공격.
                CreatureState = ECreatureState.Skill;
                // TODO

                // 공격 후 대기
                StartWait(2.0f);
            }
            else
            {
                // 공격 범위 밖이라면 추적.
                SetRigidBodyVelocity(dir.normalized * MoveSpeed);

                // 너무 멀어지면 포기.
                float searchDistanceSqr = SearchDistance * SearchDistance;
                if (distToTargetSqr > searchDistanceSqr)
                {
                    _destPos = _initPos;
                    _target = null;
                    CreatureState = ECreatureState.Move;
                }
            }
        }
    }

    protected override void UpdateSkill()
    {
        // 대기 끝나지 않았음
        if (_coWait != null)
            return;

        // 이동 상태로 돌아감
        CreatureState = ECreatureState.Move;
    }

    protected override void UpdateDead()
    {

    }
    #endregion
}
