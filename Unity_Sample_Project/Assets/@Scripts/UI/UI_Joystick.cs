using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class UI_Joystick : UI_Base
{
    enum GameObjects
    {
        JoystickBG,
        JoystickCursor,
    }

    private GameObject _background;
    private GameObject _cursor;
    private float _radius;
    private Vector2 _touchPos;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));

        _background = GetObject((int)GameObjects.JoystickBG);
        _cursor = GetObject((int)GameObjects.JoystickCursor);
        _radius = _background.GetComponent<RectTransform>().sizeDelta.y / 5;

        gameObject.BindEvent(OnPointerDown, type: Define.EUIEvent.PointerDown);
        gameObject.BindEvent(OnPointerUp, type: Define.EUIEvent.PointerUp);
        gameObject.BindEvent(OnDrag, type: Define.EUIEvent.Drag);

        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<Canvas>().worldCamera = Camera.main;

        return true;
    }

    #region Event
    public void OnPointerDown(PointerEventData eventData)
    {
        _touchPos = Input.mousePosition;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _background.transform.position = mouseWorldPos;
        _cursor.transform.position = mouseWorldPos;

        Managers.Game.JoystickState = EJoystickState.PointerDown;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _background.transform.position = _touchPos;
        _cursor.transform.position = _touchPos;

        Managers.Game.MoveDir = Vector2.zero;
        Managers.Game.JoystickState = EJoystickState.PointerUp;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 드래그한 위치 - 처음 터치한 위치
        // (A - B) : B에서 A를 향한 방향
        // 를 통해 방향 벡터를 구할 수 있음
        Vector2 touchDir = (eventData.position - _touchPos);

        // 최대 거리
        float moveDist = Mathf.Min(touchDir.magnitude, _radius);
        // 방향은 normalize를 통해 단위 벡터로
        Vector2 moveDir = touchDir.normalized;
        // 해당 거리로 이동할 새로운 좌표 위치
        Vector2 newPosition = _touchPos + moveDir * moveDist;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(newPosition);
        _cursor.transform.position = worldPos;

        Managers.Game.MoveDir = moveDir;
        Managers.Game.JoystickState = EJoystickState.Drag;
    }
    #endregion
}
