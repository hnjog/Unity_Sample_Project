using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Data;
using System;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

// Editor 폴더를 만들어 그 안에 스크립트가 생성되어 C# 에디터라는 프로젝트로 분리됨
public class DataTransformer : EditorWindow
{
#if UNITY_EDITOR
    [MenuItem("Tools/ParseExcel %#K")]
    public static void ParseExcelDataToJson()
    {
        // 툴 작동시
        // Ctrl + Shift + k
        ParseExcelDataToJson<HeroDataLoader, HeroData>("Hero");
        ParseExcelDataToJson<MonsterDataLoader, MonsterData>("Monster");
        ParseExcelDataToJson<EnvDataLoader, EnvData>("Env");
        ParseExcelDataToJson<SkillDataLoader, SkillData>("Skill");
        ParseExcelDataToJson<ProjectileDataLoader, ProjectileData>("Projectile");

        Debug.Log("DataTransformer Completed");
    }

    #region Helpers
    private static void ParseExcelDataToJson<Loader, LoaderData>(string filename) where Loader : new() where LoaderData : new()
    {
        Loader loader = new Loader();
        FieldInfo field = loader.GetType().GetFields()[0];
        field.SetValue(loader, ParseExcelDataToList<LoaderData>(filename));

        string jsonStr = JsonConvert.SerializeObject(loader, Formatting.Indented);
        File.WriteAllText($"{Application.dataPath}/@Resources/Data/JsonData/{filename}Data.json", jsonStr);
        AssetDatabase.Refresh();
    }

    // : new() - LoaderData 타입에 매개 변수 없는 생성자가 존재해야 함
    private static List<LoaderData> ParseExcelDataToList<LoaderData>(string filename) where LoaderData : new()
    {
        List<LoaderData> loaderDatas = new List<LoaderData>();

        string[] lines = File.ReadAllText($"{Application.dataPath}/@Resources/Data/ExcelData/{filename}Data.csv").Split("\n");

        for (int l = 1; l < lines.Length; l++)
        {
            string[] row = lines[l].Replace("\r", "").Split(',');
            if (row.Length == 0)
                continue;
            if (string.IsNullOrEmpty(row[0]))
                continue;

            // TestData 가 일단 위쪽에서 사용 중
            LoaderData loaderData = new LoaderData();

            // TestData의 모든 필드(변수 요소들)를 가져온다
            System.Reflection.FieldInfo[] fields = typeof(LoaderData).GetFields();
            // 필드를 순회하여 개체의 해당 필드를 가져온다
            for (int f = 0; f < fields.Length; f++)
            {
                // 해당 제네릭 타입의 타입 - 필드 정보를
                // 미리 가져온 이름으로 가져온다
                FieldInfo field = loaderData.GetType().GetField(fields[f].Name);
                Type type = field.FieldType;

                // 제네릭 타입이라면 ConvertList 호출하여 변환
                if (type.IsGenericType)
                {
                    object value = ConvertList(row[f], type);
                    // fieldInfo 이기에
                    // 실제로 값을 써줄 대상(이 필드가 속한 인스턴스)에 값을 써준다
                    field.SetValue(loaderData, value);
                }
                else
                {
                    object value = ConvertValue(row[f], field.FieldType);
                    field.SetValue(loaderData, value);
                }
            }

            loaderDatas.Add(loaderData);
        }

        return loaderDatas;
    }

    private static object ConvertValue(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        // 해당 타입에 맞도록 문자열을 변환
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        return converter.ConvertFromString(value);
    }

    private static object ConvertList(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        // Reflection
        // 인자 타입 가져오기
        Type valueType = type.GetGenericArguments()[0];
        // 해당 타입의 리스트 생성
        Type genericListType = typeof(List<>).MakeGenericType(valueType);
        var genericList = Activator.CreateInstance(genericListType) as IList;

        // Parse Excel
        // &로 값을 나누고
        // 각 요소를 ConvertValue를 통해 변환한다
        var list = value.Split('&').Select(x => ConvertValue(x, valueType)).ToList();

        // 이걸 리스트에 더해줌
        foreach (var item in list)
            genericList.Add(item);

        return genericList;
    }
    #endregion

#endif
}