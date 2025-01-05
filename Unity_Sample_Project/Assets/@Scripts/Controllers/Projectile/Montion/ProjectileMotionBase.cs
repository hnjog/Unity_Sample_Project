using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileMotionBase : InitBase
{
    Coroutine _coLaunchProjectile;

    public Vector3 StartPosition { get; private set; } // 투사체 시작 위치
    public Vector3 TargetPosition { get; private set; } // 목표 위치 (PS : Vector3가 구조체(struct) 타입이라, '값' 형식으로 전달됨)
    public bool LookAtTarget { get; private set; } // Target의 방향을 바라봐야 하는가?
    public Data.ProjectileData ProjectileData { get; private set; } // 투사체 정보
    protected Action EndCallback { get; private set; } // 투사체 이동 완료 후 처리할 콜백함수

    protected float _speed;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    protected void SetInfo(int projectileTemplateID, Vector3 spawnPosition, Vector3 targetPosition, Action endCallback = null)
    {
        _speed = 5.0f;

        if(projectileTemplateID != 0)
        {
            ProjectileData = Managers.Data.ProjectileDic[projectileTemplateID];
            _speed = ProjectileData.ProjSpeed;
        }
        
        StartPosition = spawnPosition;
        TargetPosition = targetPosition;
        EndCallback = endCallback;

        LookAtTarget = true; // TEMP

        if (_coLaunchProjectile != null)
            StopCoroutine(_coLaunchProjectile);

        // 발사 시 투사체가 움직여야 할 것을 하위 클래스에서 처리하도록 가상 함수 처리
        _coLaunchProjectile = StartCoroutine(CoLaunchProjectile());
    }

    protected void LookAt2D(Vector2 forward)
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg);
    }

    protected abstract IEnumerator CoLaunchProjectile();
}
