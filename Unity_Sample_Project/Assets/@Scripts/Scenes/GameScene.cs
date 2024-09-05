using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = Define.EScene.GameScene;

        // Map 로드
        GameObject map = Managers.Resource.Instantiate("BaseMap");
        map.transform.position = Vector3.zero;
        map.name = "@BaseMap";

        // 게임 신에서 직접 생성하면 관리가 어려워짐
        // 생성용 클래스
        //Managers.Resource.Instantiate("Hero");



        return true;
    }
    public override void Clear()
    {

    }
}
