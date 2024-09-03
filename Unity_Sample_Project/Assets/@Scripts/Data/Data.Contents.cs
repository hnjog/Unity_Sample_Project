using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    #region TestData

    // 데이터 시트에서 받을 데이터 목록을 
    // 하나의 클래스로 묘사 -> 이를 Reflection 문법을 통해
    // Binary 데이터를 클래스에 맞게 매핑해준다
    // 데이터로 '사용할' 포맷 정도로 인식하는 클래스
    [Serializable]
    public class TestData
    {
        public int Level;
        public int Exp;
        public List<int> Skills;
        public float Speed;
        public string Name;
    }

    // 이걸 호출하여 Json을 매핑
    [Serializable]
    public class TestDataLoader : ILoader<int, TestData>
    {
        public List<TestData> tests = new List<TestData>();

        public Dictionary<int, TestData> MakeDict()
        {
            Dictionary<int, TestData> dict = new Dictionary<int, TestData>();
            foreach (TestData testData in tests)
                dict.Add(testData.Level, testData);

            return dict;
        }
    }
    #endregion
}