using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class EffectComponent : MonoBehaviour
{
    public List<EffectBase> ActiveEffects = new List<EffectBase>();
    private Creature _owner;

    public void SetInfo(Creature Owner)
    {
        _owner = Owner;
    }

    // 이펙트를 걸어준다
    public List<EffectBase> GenerateEffects(IEnumerable<int> effectIds, EEffectSpawnType spawnType)
    {
        List<EffectBase> generatedEffects = new List<EffectBase>();

        // IEnumerable 은 컬렉션 내부를 순회할 수 있는 기능이 있는 인터페이스
        // 따라서 foreach를 돌릴 수 있음
        // 이펙트 id 순회
        foreach (int id in effectIds)
        {
            // 데이터 시트에 클래스 이름을 넣어
            // 데이터 시트의 세부 구현 난이도를 낮춘다
            string className = Managers.Data.EffectDic[id].ClassName;
            Type effectType = Type.GetType(className);

            if (effectType == null)
            {
                Debug.LogError($"Effect Type not found: {className}");
                return null;
            }

            // effectbase 생성하고
            GameObject go = Managers.Object.SpawnGameObject(_owner.CenterPosition, "EffectBase");
            go.name = Managers.Data.EffectDic[id].ClassName;

            // 이펙트 타입에 맞게 컴포넌트 붙여준다
            EffectBase effect = go.AddComponent(effectType) as EffectBase;
            // 일종의 부모 컴포넌트
            effect.transform.parent = _owner.Effects.transform;
            effect.transform.localPosition = Vector2.zero;

            // 조금 특이하게도 이쪽에서 오브젝트를 넣어준다
            Managers.Object.Effects.Add(effect);

            ActiveEffects.Add(effect);
            generatedEffects.Add(effect);

            effect.SetInfo(id, _owner, spawnType);
            effect.ApplyEffect(); // 이펙트가 작동되도록 재생
        }

        return generatedEffects;
    }

    public void RemoveEffects(EffectBase effects)
    {

    }

    public void ClearDebuffsBySkill()
    {
        foreach (var buff in ActiveEffects.ToArray())
        {
            if (buff.EffectType != EEffectType.Buff)
            {
                buff.ClearEffect(EEffectClearType.ClearSkill);
            }
        }
    }
}
