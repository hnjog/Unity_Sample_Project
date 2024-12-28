using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TitleScene : UI_Scene
{
    // Scene의 이름들
    enum GameObjects
    {
        StartImage
    }

    enum Texts
    {
        DisplayText
    }

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // 위에서 Enum으로 Scene에 배치된
        // 오브젝트들의 이름을 적어놓았기에
        // 그에 따른 자식들을 검색하여 Dictionary에 넣게된다
        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));

        // 이름을 기준으로 찾기
        // Extension Method
        GetObject((int)GameObjects.StartImage).BindEvent((evt) =>
            {
                Debug.Log("ChangeScene");
                Managers.Scene.LoadScene(Define.EScene.GameScene);
            });

        GetObject((int)GameObjects.StartImage).gameObject.SetActive(false);
        GetText((int)Texts.DisplayText).text = $"";

        StartLoadAssets();

        return true;
    }

    void StartLoadAssets()
    {
        //Object 를 제네릭 T로 넘김으로서 모든 "PreLoad" 라벨의 에셋을 Load 한다
        // Async : 비동기 -> 끝나지 않아도 알아서 실행 (멀티 스레드)
        // 이후, 콜백함수를 통해 완료됨을 확인
        // 
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
        {
            Debug.Log($"{key} {count} / {totalCount}");

            // 정상적으로 로딩 완료
            if (count == totalCount)
            {
                Managers.Data.Init();

                // 데이터 존재 확인
                if(Managers.Game.LoadGame() == false)
                {
                    // 데이터가 없다면 초기화하고 먼저 저장한다
                    Managers.Game.InitGame();
                    Managers.Game.SaveGame();
                }

                // 시작 준비
                GetObject((int)GameObjects.StartImage).gameObject.SetActive(true);
                GetText((int)Texts.DisplayText).text = "Touch To Start";
            }

        });
    }
}
