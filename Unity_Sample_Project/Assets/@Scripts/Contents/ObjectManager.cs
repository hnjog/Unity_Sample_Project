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
    #endregion

    public T Spawn<T>(Vector3 position) where T : BaseObject
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
        }
        else if (obj.ObjectType == EObjectType.Projectile)
        {

        }
        else if (obj.ObjectType == EObjectType.Env)
        {

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

        }
        else if (obj.ObjectType == EObjectType.Env)
        {

        }

        Managers.Resource.Destroy(obj.gameObject);
    }
}