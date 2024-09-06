using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Hero : Creature
{
    Vector2 _moveDir = Vector2.zero;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        CreatureType = ECreatureType.Hero;
        CreatureState = ECreatureState.Idle;
        Speed = 5.0f;

        // Game의 이벤트 구독하기 위함
        // + 만 해도 되지만, 혹시나 2번 호출하는 경우를 대비하여 미리 한번 빼준다
        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        Managers.Game.OnJoystickStateChanged -= HandleOnJoystickStateChanged;
        Managers.Game.OnJoystickStateChanged += HandleOnJoystickStateChanged;

        return true;
    }

    void Update()
    {
        // extention 문법으로 transform으로 사용 가능
        // 부모 함수이기에 그냥 사용하는 방식도 있음
        // 이동 관련을 좀 더 명확하게 보여주고 싶기에??
        // 지정한 곳에 이동
        // 내부에서 Translate 호출하기에 DeltaTime 곱하여 넘겨준다
        transform.TranslateEx(_moveDir * Time.deltaTime * Speed);
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;
        Debug.Log(dir);
    }

    private void HandleOnJoystickStateChanged(EJoystickState joystickState)
    {
        switch (joystickState)
        {
            case Define.EJoystickState.PointerDown:
                CreatureState = Define.ECreatureState.Move;
                break;
            case Define.EJoystickState.Drag:
                break;
            case Define.EJoystickState.PointerUp:
                CreatureState = Define.ECreatureState.Idle;
                break;
            default:
                break;
        }
    }
}
