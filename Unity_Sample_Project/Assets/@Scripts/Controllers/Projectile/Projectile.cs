using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : BaseObject
{
    public Creature Owner {  get; private set; }
    public SkillBase Skill { get; private set; }
    public Data.ProjectileData ProjectileData { get; private set; }
    public ProjectileMotionBase ProjectileMotion { get; private set; }

    private SpriteRenderer _spriteRenderer;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        ObjectType = Define.EObjectType.Projectile;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sortingOrder = SortingLayers.PROJECTILE;

        return true;
    }

    public void SetInfo(int dataTemplateID)
    {
        ProjectileData = Managers.Data.ProjectileDic[dataTemplateID];
        _spriteRenderer.sprite = Managers.Resource.Load<Sprite>(ProjectileData.ProjectileSpriteName);

        if(_spriteRenderer.sprite == null)
        {
            Debug.LogWarning($"Projectile Sprite Missing {ProjectileData.ProjectileSpriteName}");
            return;
        }
    }

    public void SetSpawnInfo(Creature owner, SkillBase skill, LayerMask layer)
    {
        Owner = owner;
        Skill = skill;

        Collider.excludeLayers = layer;

        // 현 시점에서는 없는게 맞음
        if (ProjectileMotion != null)
            Destroy(ProjectileMotion);

        // 클래스 이름으로 치환하여 사용
        string componentName = skill.SkillData.ComponentName;
        // 타입 정보를 통해 AddComponent 한다 (리플렉션)
        ProjectileMotion = gameObject.AddComponent(Type.GetType(componentName)) as ProjectileMotionBase;

        // Temp : 임시 코드
        StraightMotion straightMotion = ProjectileMotion as StraightMotion;
        // 일단 End 콜백을 Despawn 하도록 설정
        if (straightMotion != null)
            straightMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.Object.Despawn(this); });

        ParabolaMotion parabolaMotion = ProjectileMotion as ParabolaMotion;
        if (parabolaMotion != null)
            parabolaMotion.SetInfo(ProjectileData.DataId, owner.CenterPosition, owner.Target.CenterPosition, () => { Managers.Object.Despawn(this); });


        // 시간 임시
        StartCoroutine(CoReserveDestroy(5.0f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseObject target = other.GetComponent<BaseObject>();
        if (target.IsValid() == false)
            return;

        // 데미지 입히기 (Temp)
        target.OnDamaged(Owner, Skill);
        Managers.Object.Despawn(this);
    }

    private IEnumerator CoReserveDestroy(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.Object.Despawn(this);
    }
}
