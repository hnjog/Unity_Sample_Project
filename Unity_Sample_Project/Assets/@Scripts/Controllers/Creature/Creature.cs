using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Creature : BaseObject
{
    public float Speed { get; protected set; } = 1.0f;
    public ECreatureType CreatureType { get; protected set; } = ECreatureType.None;

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
        CreatureState = ECreatureState.Idle;
        return true;
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