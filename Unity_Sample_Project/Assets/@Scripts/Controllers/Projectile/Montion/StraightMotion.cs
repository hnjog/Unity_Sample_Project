using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightMotion : ProjectileMotionBase
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        return true;
    }

    // 부모 클래스의 SetInfo를 '숨기고' 새로운 메서드를 정의하기 위하여 new 를 사용
    // - 부모의 virtual 함수가 아닌 경우, 이런식으로 재정의를 하는 방식으로도 사용할 수는 있음
    // (권장되는 방식은 아니지만, 부모 클래스를 수정할 수 없는 경우 등의 처리)
    // 
    // 기본적으로는 override와 유사해보이지만
    // 이건 함수를 '숨기는' 역할이기에 '다형성'이 먹히지 않는다
    // (부모 클래스의 참조 변수로 자식 클래스의 SetInfo를 호출하더라도, 부모 클래스에서 정의된 메서드 호출)
    public new void SetInfo(int dataTemplateID, Vector3 startPosition, Vector3 targetPosition, Action endCallback)
    {
        base.SetInfo(dataTemplateID, startPosition, targetPosition, endCallback);
    }

    // 일단 코루틴으로 구현하지만
    // 너무 많은 탄막이 움직이는 경우, 성능 부담이 있을 수 있음
    protected override IEnumerator CoLaunchProjectile()
    {
        float journeyLength = Vector3.Distance(StartPosition, TargetPosition); // 목적까지의 거리
        float totalTime = journeyLength / ProjectileData.ProjSpeed; // 예상 시간
        float elapsedTime = 0;

        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;

            // 이동
            float normalizedTime = elapsedTime / totalTime;
            transform.position = Vector3.Lerp(StartPosition, TargetPosition, normalizedTime);

            if (LookAtTarget)
                LookAt2D(TargetPosition - transform.position);

            // 다음 프레임까지 대기
            yield return null;
        }

        transform.position = TargetPosition;
        EndCallback?.Invoke();
    }
}
