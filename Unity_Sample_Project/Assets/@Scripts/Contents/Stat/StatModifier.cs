using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// Stat을 수정하는 데이터 보존 용도의 클래스
// 아이템, 스킬 등의 다양한 곳에서 사용할 예정
public class StatModifier
{
    public readonly float Value;        // 수치
    public readonly EStatModType Type;  // 합, 퍼센트 합, 퍼센트 곱
    public readonly int Order;          // 우선순위
    public readonly object Source;      // 이 데이터를 가진 녀석

    public StatModifier(float value, EStatModType type, int order, object source)
    {
        Value = value;
        Type = type;
        Order = order;
        Source = source;
    }

    public StatModifier(float value, EStatModType type) : this(value, type, (int)type, null) { }

    public StatModifier(float value, EStatModType type, int order) : this(value, type, order, null) { }

    public StatModifier(float value, EStatModType type, object source) : this(value, type, (int)type, source) { }
}
