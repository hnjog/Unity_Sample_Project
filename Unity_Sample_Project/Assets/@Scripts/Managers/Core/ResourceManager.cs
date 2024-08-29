using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

// MonoBehaviour 상속
// : 유니티의 GameObject에서 컴포넌트로 사용 가능
// Managers에서 이 클래스들을 관리할 예정이므로 상속을 받지 않음
public class ResourceManager
{
    // Addressable
    // 필요할때 마다 로드?
    // 한 번에 로드? -> 이 방식으로 사용 (로딩할 때 한번에!)
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    #region Load Resource
    // where : 제약 조건 (T가 Object의 하위 클래스)
    // 여담 - C++의 템플릿 과 C#의 제네릭 차이
    // C++은 컴파일 타임에 템플릿 인스턴스 함수를 만들어 사용되는 모든 타입의 임시 함수를 만든다
    // (그렇기에 컴파일 시 오류가 발견 가능)
    // C#은 런타임 시에 타입이 결정되어 작업을 수행 + where로 사용을 제한
    //
    public T Load<T>(string key) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
            return resource as T; // Resoucre를 T로 캐스팅하여 반환 (성공하겠지만(where) 실패시엔 null)

        return null;
    }

    public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
    {
        GameObject prefab = Load<GameObject>(key);
        if (prefab == null)
        {
            Debug.LogError($"Failed to load prefab : {key}");
            return null;
        }

        // Addressable의 Instance 방식을 사용하지 않는 이유는
        // refCount에 영향을 주기에 Clear 쪽에서도 같은 방식을 이용해야 함
        // -> 제거를 할때 Addressable로 '사용되었는지'를 파악하는 여부가 까다롭기에
        // 일반적인 Unity 방식으로 사용

        //if(pooling)
        //    return Managers.Pool.Pop(prefab);

        GameObject go = Object.Instantiate(prefab, parent);
        go.name = prefab.name;
        return go;
    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;

        //if (Managers.Pool.Push(go))
        //    return;

        Object.Destroy(go);
    }

    #endregion

    /* 
      기본적인 Unity의 자원 관리 방식(Old School)
      Resources 폴더를 만들고 그 안에 아트 리소스들을 죄다 넣어 관리하여
      게임이 실행될때 그 폴더 내부의 요소를 활용이 가능해짐
     (엔진이 해당 이름의 폴더 내부의 요소를 자동으로 인식하며, 런타임 중 동적 로드가 가능하게 함)
      (Ex : Resources.Load("Asset"))
     다만 해당 폴더에 있는 모든 자원이 메모리에 로드되기에 주의
     (그렇기에 Addressable 방식이 더 효율적인 메모리 관리가 가능)

     추가적으로 이 방식은 차후 어플리케이션의 '배포'에 불리한 영향을 줄 수 있음
     빌드할 때, '처음부터' 해당 리소스들을 '포함'하여 빌드하기에
     '모든 자원'을 다시 패키징하게 됨
     (아주 조금의 내용만 패치하더라도 모든 내용이 패키징에 포함되어야....)
     그렇기에 User가 처음부터 다시 다운을 받는 패치가 만들어질 수 있음!

    그렇지 않으려면 패키지를 쪼개 번들을 만들게 된다
    필요한 부분만 추가하여 번들을 만들게 되면 스토어의 심사기간도 줄어들며,
    패치의 크기도 작아짐
      
     */

    #region Addressable
    // Action<T> : 콜백 함수 (C#의 델리게이트 타입)
    // 특정 타입인 T를 인자로 받는 메서드를 참조
    // 여러개의 인자라면 Action<int, string>, Action<int, int, int> 이런식
    private void LoadAsync<T>(string key, Action<T> callback = null) where T : Object
    {
        if (_resources.TryGetValue(key, out Object resource))
        {
            // Delegate 호출
            callback?.Invoke(resource as T);
            return;
        }

        string loadKey = key;
        if (key.Contains(".sprite"))
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";

        // 비동기 로드
        // (비동기 : 유니티의 메인 스레드는 건드리지 않고 다른 작업을 동시에 수행)
        // LoadAssetAsync 를 이용하여 비동기적으로 리소스를 로드함
        var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
        // 로드 완료에 대한 처리를 (람다)
        // Delegate에 등록해준다
        asyncOperation.Completed += (op) =>
        {
            _resources.Add(key, op.Result);
            _handles.Add(key, asyncOperation);
            callback?.Invoke(op.Result);
        };
    }

    public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : Object
    {
        var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        opHandle.Completed += (op) =>
        {
            int loadCount = 0;
            int totalCount = op.Result.Count;

            foreach (var result in op.Result)
            {
                // .sprite를 붙여 다른 타입으로 인식하지 않도록
                // 캐스팅을 바꾸어줌 (Addressable의 버그를 예방)
                if (result.PrimaryKey.Contains(".sprite"))
                {
                    LoadAsync<Sprite>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
                else
                {
                    LoadAsync<T>(result.PrimaryKey, (obj) =>
                    {
                        loadCount++;
                        callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                    });
                }
            }

        };
    }

    public void Clear()
    {
        _resources.Clear();

        foreach (var handle in _handles)
            Addressables.Release(handle);

        _handles.Clear();
    }
    #endregion
}
