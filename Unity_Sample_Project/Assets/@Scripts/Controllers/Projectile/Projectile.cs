using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : BaseObject
{
    public Creature Owner {  get; private set; }
    public SkillBase Skill { get; private set; }
    public Data.ProjectileData ProjectileData { get; private set; }

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

        // 시간 임시
        StartCoroutine(CoReserveDestroy(5.0f));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseObject target = other.GetComponent<BaseObject>();
        if (target.IsValid() == false)
            return;

        // 데미지 입히기
        target.OnDamaged(Owner, Skill);
        Managers.Object.Despawn(this);
    }

    private IEnumerator CoReserveDestroy(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        Managers.Object.Despawn(this);
    }
}
