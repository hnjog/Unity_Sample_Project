using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

// 직렬화 가능
// - 직렬화 : 데이터 구조나 객체 상태를 특정 형식으로 변환하여 저장하거나 전송 (데이터를 바이트로 전환)
// (서버 등에 전송하는 등 네트워킹, 파일 저장, db 등에서 유용하게 사용)
// (그렇다고 모든 걸 직렬화할 필요는 없음 - 성능 및 보안 이슈)
[Serializable]
public class CreatureStat
{
    public float BaseValue { get; private set; } // 기본 값

    private bool _isDirty = true; // 더티 플래그 (해당 데이터가 수정되었는지의 여부 파악)

    [SerializeField]
    private float _value;
    public virtual float Value
    {
        get
        {
            // 더티 플래그가 체크 되어있으므로
            // 값 원할때 재계산 해서 준다
            if (_isDirty)
            {
                _value = CalculateFinalValue();
                _isDirty = false; // 재계산 완료 - 깨끗한 값
            }
            return _value;
        }

        private set { _value = value; }
    }

    // 이 스탯에 영향을 줄 녀석들을 담아줄 리스트
    public List<StatModifier> StatModifiers = new List<StatModifier>();

    public CreatureStat()
    {
    }

    public CreatureStat(float baseValue) : this()
    {
        BaseValue = baseValue;
    }

    public virtual void AddModifier(StatModifier modifier)
    {
        // 새로 능력치 건드는거 들어오면 더티 플래그 체크하여 나중에 재계산 하도록
        _isDirty = true;
        StatModifiers.Add(modifier);
    }

    public virtual bool RemoveModifier(StatModifier modifier)
    {
        // 능력치 건드는거 제거되면 더티 플래그 체크하여 나중에 재계산 하도록
        if (StatModifiers.Remove(modifier))
        {
            _isDirty = true;
            return true;
        }

        return false;
    }

    public virtual bool ClearModifiersFromSource(object source)
    {
        int numRemovals = StatModifiers.RemoveAll(mod => mod.Source == source);

        // 능력치 건드는거 제거되면 더티 플래그 체크하여 나중에 재계산 하도록
        if (numRemovals > 0)
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    private int CompareOrder(StatModifier a, StatModifier b)
    {
        if (a.Order == b.Order)
            return 0;

        return (a.Order < b.Order) ? -1 : 1;
    }


    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        // 적용되어 있는 스탯 수정하려는 요소들을 정렬하고 차례대로 적용
        StatModifiers.Sort(CompareOrder);

        for (int i = 0; i < StatModifiers.Count; i++)
        {
            StatModifier modifier = StatModifiers[i];

            switch (modifier.Type)
            {
                case EStatModType.Add: // 단순 합
                    finalValue += modifier.Value;
                    break;
                case EStatModType.PercentAdd: // 퍼센트 합
                    sumPercentAdd += modifier.Value;
                    // 퍼센트 합들은 모아서 한번에 더해준다
                    if (i == StatModifiers.Count - 1 || StatModifiers[i + 1].Type != EStatModType.PercentAdd)
                    {
                        finalValue *= 1 + sumPercentAdd;
                        sumPercentAdd = 0;
                    }
                    break;
                case EStatModType.PercentMult: // 퍼센트 곱
                    finalValue *= 1 + modifier.Value;
                    break;
            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}
