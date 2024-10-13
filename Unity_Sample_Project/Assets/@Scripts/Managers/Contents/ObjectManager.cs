using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class ObjectManager
{
    // Object를 관리하는 Manager
    // 전반적인 생성 담당 (Factory)

    // Container (List 방식도 존재)
    public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
    public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
    public HashSet<Env> Envs { get; } = new HashSet<Env>();
    public HashSet<Projectile> Projectiles { get; } = new HashSet<Projectile>();
    public HashSet<EffectBase> Effects { get; } = new HashSet<EffectBase>();
    public HeroCamp Camp { get; private set; }

    #region Roots
    // Root 부모를 만들어 게임 Scene 등에서 관측하기 편하도록
    public Transform GetRootTransform(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
            root = new GameObject { name = name };

        return root.transform;
    }

    public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
    public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
    public Transform EnvRoot { get { return GetRootTransform("@Envs"); } }
    public Transform ProjectileRoot { get { return GetRootTransform("@Projectiles"); } }
    public Transform EffectRoot { get { return GetRootTransform("@Effects"); } }
    #endregion

    public void ShowDamageFont(Vector2 position, float damage, Transform parent, bool isCritical = false)
    {
        GameObject go = Managers.Resource.Instantiate("DamageFont", pooling: true);
        DamageFont damageText = go.GetComponent<DamageFont>();
        damageText.SetInfo(position, damage, parent, isCritical);
    }

    public GameObject SpawnGameObject(Vector3 position, string prefabName)
    {
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
        go.transform.position = position;
        return go;
    }

    public T Spawn<T>(Vector3Int cellPos, int templateID) where T : BaseObject
    {
        Vector3 spawnPos = Managers.Map.Cell2World(cellPos);
        return Spawn<T>(spawnPos, templateID);
    }

    public T Spawn<T>(Vector3 position,int templateID) where T : BaseObject
    {
        // 규칙 선행 필수
        // - Prefab 이름을 Class 타입과 동일하게 작성
        string prefabName = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate(prefabName);
        go.name = prefabName;
        go.transform.position = position;

        BaseObject obj = go.GetComponent<BaseObject>();

        // Reflection을 사용하는 방식도 존재하지만,
        // 성능상 유리하진 않기에
        // 미리 상속시킨 타입 변수를 이용
        if (obj.ObjectType == EObjectType.Creature)
        {
            Creature creature = go.GetComponent<Creature>();
            switch (creature.CreatureType)
            {
                case ECreatureType.Hero:
                    // Root 밑으로 들어가도록 붙여준다
                    obj.transform.parent = HeroRoot;
                    // 캐스팅 (실패시 null)
                    Hero hero = creature as Hero;
                    Heroes.Add(hero);
                    break;
                case ECreatureType.Monster:
                    obj.transform.parent = MonsterRoot;
                    Monster monster = creature as Monster;
                    Monsters.Add(monster);
                    break;
            }

            creature.SetInfo(templateID);
        }
        else if (obj.ObjectType == EObjectType.Projectile)
        {
            obj.transform.parent = ProjectileRoot;

            Projectile projectile = go.GetComponent<Projectile>();
            Projectiles.Add(projectile);

            projectile.SetInfo(templateID);
        }
        else if (obj.ObjectType == EObjectType.Env)
        {
            obj.transform.parent = EnvRoot;

            Env env = go.GetComponent<Env>();
            Envs.Add(env);

            env.SetInfo(templateID);
        }
        else if (obj.ObjectType == EObjectType.HeroCamp)
        {
            Camp = go.GetComponent<HeroCamp>();
        }

        return obj as T;
    }

    public void Despawn<T>(T obj) where T : BaseObject
    {
        EObjectType objectType = obj.ObjectType;

        if (obj.ObjectType == EObjectType.Creature)
        {
            Creature creature = obj.GetComponent<Creature>();
            switch (creature.CreatureType)
            {
                case ECreatureType.Hero:
                    Hero hero = creature as Hero;
                    Heroes.Remove(hero);
                    break;
                case ECreatureType.Monster:
                    Monster monster = creature as Monster;
                    Monsters.Remove(monster);
                    break;
            }
        }
        else if (obj.ObjectType == EObjectType.Projectile)
        {
            Projectile projectile = obj as Projectile;
            Projectiles.Remove(projectile);
        }
        else if (obj.ObjectType == EObjectType.Env)
        {
            Env env = obj as Env;
            Envs.Remove(env);
        }
        else if (obj.ObjectType == EObjectType.HeroCamp)
        {
            Camp = null;
        }

        Managers.Resource.Destroy(obj.gameObject);
    }

    #region Skill 판정
    public List<Creature> FindConeRangeTargets(Creature owner, Vector3 dir, float range, int angleRange, bool isAllies = false)
    {
        List<Creature> targets = new List<Creature>();
        List<Creature> ret = new List<Creature>();

        ECreatureType targetType = Util.DetermineTargetType(owner.CreatureType, isAllies);

        if (targetType == ECreatureType.Monster)
        {
            var objs = Managers.Map.GatherObjects<Monster>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }
        else if (targetType == ECreatureType.Hero)
        {
            var objs = Managers.Map.GatherObjects<Hero>(owner.transform.position, range, range);
            targets.AddRange(objs);
        }

        foreach (var target in targets)
        {
            // 1. 거리안에 있는지 확인
            var targetPos = target.transform.position;
            float distance = Vector3.Distance(targetPos, owner.transform.position);

            // 거리 너무 멀다
            if (distance > range)
                continue;

            // 2. 각도 확인
            if (angleRange != 360)
            {
                BaseObject ownerTarget = (owner as Creature).Target;

                // 2. 부채꼴 모양 각도 계산
                // 내적을 통해서
                // 사용자가 타겟을 바라보는 방향과, 현재 dir(시전자가 주시하는 방향)을 내적하여
                // -1 ~ 1 의 값을 얻는다 (1 : 두 벡터가 같은 방향, 0 : 수직, -1 두 벡터가 반대 방향)
                float dot = Vector3.Dot((targetPos - owner.transform.position).normalized, dir.normalized);
                // 아크 코사인 * 라디안to각도 를 통하여 실제 각도 계산
                float degree = Mathf.Rad2Deg * Mathf.Acos(dot);

                // dir이 바라보는 방향을 기준으로
                // angleRange는 원뿔 모양이다
                // 따라서 해당 기점에서 왼쪽과 오른쪽을 탐색하므로
                // 절반을 나누어야 합당한 탐색이 가능하다
                if (degree > angleRange / 2f)
                    continue;
            }

            ret.Add(target);
        }

        return ret;
    }

    #endregion
}
