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