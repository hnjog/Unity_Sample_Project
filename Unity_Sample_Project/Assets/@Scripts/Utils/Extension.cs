using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// C#의 Extenstion Method를 위한 문법
// (기존 클래스나 구조체에 새로운 메서드를 추가할 수 있는 문법)
// 기존 타입을 '수정하지 않고' 해당 타입의 기능을 확장시킬 수 있음

public static class Extension
{
    // 확정 메서드의 2가지 요인
    // 1. static : 별도의 개체 없이 호출될 수 있도록
    // 2. 첫번째 인자로 this : 해당 타입의 인스턴스에서 호출할 수 있도록 한다
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    public static void BindEvent(this GameObject go, Action<PointerEventData> action = null, Define.EUIEvent type = Define.EUIEvent.Click)
    {
        // BindEvent : static임
        UI_Base.BindEvent(go, action, type);
    }

    public static bool IsValid(this GameObject go)
    {
        return go != null && go.activeSelf;
    }


    public static bool IsValid(this BaseObject bo)
    {
        if (bo == null || bo.isActiveAndEnabled == false)
            return false;

        return true;
    }

    public static void DestroyChilds(this GameObject go)
    {
        foreach (Transform child in go.transform)
            Managers.Resource.Destroy(child.gameObject);
    }

    public static void TranslateEx(this Transform transform, Vector3 dir)
    {
        // GetComponent를 통해 Hero 같은 자식 객체 컴포넌트라도 가져올 수 있음
        BaseObject bo = transform.gameObject.GetComponent<BaseObject>();
        if (bo != null)
            bo.TranslateEx(dir);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]); //swap
        }
    }
}
