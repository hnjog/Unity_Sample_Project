using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Object 를 제네릭 T로 넘김으로서 모든 "PreLoad" 라벨의 에셋을 Load 한다
        // Async : 비동기 -> 끝나지 않아도 알아서 실행 (멀티 스레드)
        // 이후, 콜백함수를 통해 완료됨을 확인
        // 
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count} / {totalCount}");

            if(count == totalCount)
            {

            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
