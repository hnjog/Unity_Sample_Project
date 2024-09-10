using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<key,value> // Generic
{
    Dictionary<key,value> MakeDict();
}
public class DataManager
{
    //public Dictionary<int, Data.TestData> TestDic { get; private set; } = new Dictionary<int, Data.TestData>();

    public void Init()
    {
        //TestDic = LoadJson<Data.TestDataLoader, int, Data.TestData>("TestData").MakeDict();
    }

    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
        // 특정 경로의 Text 에셋
        // Addressable 방식으로 load
        TextAsset textAsset = Managers.Resource.Load<TextAsset>(path);

        // Newton Json을 convert를 이용하여
        // 해당 클래스 방식으로 캐스팅하여 return 받음
        return JsonConvert.DeserializeObject<Loader>(textAsset.text);
    }
}
