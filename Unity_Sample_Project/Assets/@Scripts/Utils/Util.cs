using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using static Define;

public static class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        // 특정 컴포넌트에서 해당 컴포넌트가 있는지 확인
        T component = go.GetComponent<T>();
        // 없다면 붙여준다
        if (component == null)
            component = go.AddComponent<T>();

        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        // 현재 Object만 탐색
        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            // 자식 오브젝트에서도 탐색
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static T ParseEnum<T>(string value)
    {
        // 문자열을 Enum 으로 변경
        return (T)Enum.Parse(typeof(T), value, true);
    }

    public static Color HexToColor(string color)
    {
        // # 안붙어있으면 붙여준다
        if (color.Contains("#") == false)
            color = $"#{color}";

        ColorUtility.TryParseHtmlString(color, out Color parsedColor);

        return parsedColor;
    }

    // 영웅이면 몬스터, 몬스터면 적을 반환
    public static EObjectType DetermineTargetType(EObjectType ownerType, bool findAllies)
    {
        if (ownerType == Define.EObjectType.Hero)
        {
            return findAllies ? EObjectType.Hero : EObjectType.Monster;
        }
        else if (ownerType == Define.EObjectType.Monster)
        {
            return findAllies ? EObjectType.Monster : EObjectType.Hero;
        }

        return EObjectType.None;
    }

    public static float GetEffectRadius(EEffectSize size)
    {
        switch (size)
        {
            case EEffectSize.CircleSmall:
                return EFFECT_SMALL_RADIUS;
            case EEffectSize.CircleNormal:
                return EFFECT_NORMAL_RADIUS;
            case EEffectSize.CircleBig:
                return EFFECT_BIG_RADIUS;
            case EEffectSize.ConeSmall:
                return EFFECT_SMALL_RADIUS * 2f;
            case EEffectSize.ConeNormal:
                return EFFECT_NORMAL_RADIUS * 2f;
            case EEffectSize.ConeBig:
                return EFFECT_BIG_RADIUS * 2f;
            default:
                return EFFECT_SMALL_RADIUS;
        }
    }

    // 랜덤한 가중치 계산
    public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
    {
        float totalWeight = sequence.Sum(weightSelector);

        double itemWeightIndex = new System.Random().NextDouble() * totalWeight;
        float currentWeightIndex = 0;

        foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
        {
            currentWeightIndex += item.Weight;

            // If we've hit or passed the weight we are after for this item then it's the one we want....
            if (currentWeightIndex >= itemWeightIndex)
                return item.Value;

        }

        return default(T);
    }

    public static IPAddress GetIpv4Address(string hostAddress)
    {
        IPAddress[] ipAddr = Dns.GetHostAddresses(hostAddress);

        if (ipAddr.Length == 0)
        {
            Debug.LogError("AuthServer DNS Failed");
            return null;
        }

        foreach (IPAddress ip in ipAddr)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip;
            }
        }

        Debug.LogError("AuthServer IPv4 Failed");
        return null;
    }
}
