using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region CreatureData
    // 데이터 시트에서 받을 데이터 목록을 
    // 하나의 클래스로 묘사 -> 이를 Reflection 문법을 통해
    // Binary 데이터를 클래스에 맞게 매핑해준다
    // 데이터로 '사용할' 포맷 정도로 인식하는 클래스
    // '직렬화' 속성을 통해 해당 클래스 데이터를 다른 형식(Binary, JSON, XML 등)으로 변환 가능하도록 선언 
    [Serializable]
    public class CreatureData
    {
        public int DataId;
        public string DescriptionTextID;
        public string PrefabLabel;
        public float ColliderOffsetX;
        public float ColliderOffsetY;
        public float ColliderRadius;
        public float Mass;
        public float MaxHp;
        public float MaxHpBonus;
        public float Atk;
        public float AtkRange;
        public float AtkBonus;
        public float Def;
        public float MoveSpeed;
        public float TotalExp;
        public float HpRate;
        public float AtkRate;
        public float DefRate;
        public float MoveSpeedRate;
        public string IconImage;
        public string SkeletonDataID;
        public string AnimatorName;
        public List<int> SkillIdList = new List<int>();
        public int DropItemId;
    }

    [Serializable]
    public class CreatureDataLoader : ILoader<int, CreatureData>
    {
        public List<CreatureData> creatures = new List<CreatureData>();

        public Dictionary<int, CreatureData> MakeDict()
        {
            Dictionary<int, CreatureData> dict = new Dictionary<int, CreatureData>();
            foreach (CreatureData creature in creatures)
                dict.Add(creature.DataId, creature);
            return dict;
        }
    }
    #endregion

    #region MonsterData
    [Serializable]
    public class MonsterData : CreatureData
    {
    }

    [Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> monsters = new List<MonsterData>();
        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monster in monsters)
                dict.Add(monster.DataId, monster);
            return dict;
        }
    }
    #endregion

    #region HeroData
    [Serializable]
    public class HeroData : CreatureData
    {
    }

    [Serializable]
    public class HeroDataLoader : ILoader<int, HeroData>
    {
        public List<HeroData> heroes = new List<HeroData>();
        public Dictionary<int, HeroData> MakeDict()
        {
            Dictionary<int, HeroData> dict = new Dictionary<int, HeroData>();
            foreach (HeroData hero in heroes)
                dict.Add(hero.DataId, hero);
            return dict;
        }
    }
    #endregion

    #region SkillData
    [Serializable]
    public class SkillData
    {
        public int DataId;
        public string Name;
        public string ClassName;
        public string ComponentName;
        public string Description;
        public int ProjectileId;
        public string PrefabLabel;
        public string IconLabel;
        public string AnimName;
        public float CoolTime;
        public float DamageMultiplier;
        public float Duration;
        public float NumProjectiles;
        public string CastingSound;
        public float AngleBetweenProj;
        public float SkillRange;
        public float RotateSpeed;
        public float ScaleMultiplier;
        public float AngleRange;
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skills = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skill in skills)
                dict.Add(skill.DataId, skill);
            return dict;
        }
    }
    #endregion

    #region Env
    [Serializable]
    public class EnvData
    {
        public int DataId;
        public string DescriptionTextID;
        public string PrefabLabel;
        public float MaxHp;
        public int ResourceAmount;
        public float RegenTime;
        public List<String> SkeletonDataIDs = new List<String>();
        public int DropItemId;
    }

    [Serializable]
    public class EnvDataLoader : ILoader<int, EnvData>
    {
        public List<EnvData> envs = new List<EnvData>();
        public Dictionary<int, EnvData> MakeDict()
        {
            Dictionary<int, EnvData> dict = new Dictionary<int, EnvData>();
            foreach (EnvData env in envs)
                dict.Add(env.DataId, env);
            return dict;
        }
    }
    #endregion

}