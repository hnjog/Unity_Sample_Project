using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    #region Hero
    // player 뿐 아니라 npc 포함하여 이동시킬 예정이기에
    private Vector2 _moveDir;
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value;
            OnMoveDirChanged?.Invoke(value);
        }
    }

    private Define.EJoystickState _joystickState;
    public Define.EJoystickState JoystickState
    {
        get { return _joystickState; }
        set
        {
            _joystickState = value;
            OnJoystickStateChanged?.Invoke(_joystickState);
        }
    }
    #endregion

    #region Action
    // Delegate - 나중에 이동시킬때 한번에 이동시키도록!
    // Hero + Npc (구독자)
    public event Action<Vector2> OnMoveDirChanged;
    public event Action<Define.EJoystickState> OnJoystickStateChanged;
    #endregion
}
