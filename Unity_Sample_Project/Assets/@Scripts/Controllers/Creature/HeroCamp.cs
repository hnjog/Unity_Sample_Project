using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroCamp : BaseObject
{
    // Hero 를 컨트롤하기 쉽도록 도와주는 클래스 (Helper 클래스는 아니고)

    Vector2 _moveDir = Vector2.zero;

    public float Speed { get; set; } = 5.0f;

    public Transform Pivot { get; private set; }
    public Transform Destination { get; private set; }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Managers.Game.OnMoveDirChanged -= HandleOnMoveDirChanged;
        Managers.Game.OnMoveDirChanged += HandleOnMoveDirChanged;

        // bit flag
        Collider.includeLayers = (1 << (int)Define.ELayer.Obstacle); // 충돌 허용
        Collider.excludeLayers = (1 << (int)Define.ELayer.Monster) | (1 << (int)Define.ELayer.Hero); // 충돌 x

        ObjectType = Define.EObjectType.HeroCamp;

        Pivot = Util.FindChild<Transform>(gameObject, "Pivot", true);
        Destination = Util.FindChild<Transform>(gameObject, "Destination", true);

        return true;
    }

    void Update()
    {
        Vector3 dir = _moveDir * Time.deltaTime * Speed;
        Vector3 newPos = transform.position + dir;

        if (Managers.Map == null)
            return;
        if (Managers.Map.CanGo(newPos, ignoreObjects: true, ignoreSemiWall: true) == false)
            return;

        transform.position = newPos;
    }

    private void HandleOnMoveDirChanged(Vector2 dir)
    {
        _moveDir = dir;

        if (dir != Vector2.zero)
        {
            // radian to degree
            float angle = Mathf.Atan2(-dir.x, +dir.y) * 180 / Mathf.PI;
            Pivot.eulerAngles = new Vector3(0, 0, angle);
        }
    }
}
