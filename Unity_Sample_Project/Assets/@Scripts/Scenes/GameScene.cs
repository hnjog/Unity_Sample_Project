using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

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
        Hero hero = Managers.Object.Spawn<Hero>(new Vector3(-10,-5,0));
        hero.CreatureState = Define.ECreatureState.Move;

        CameraController cameraController = Camera.main.GetOrAddComponent<CameraController>();
        cameraController.Target = hero;

        Managers.UI.ShowBaseUI<UI_Joystick>();

        {
            Monster monster = Managers.Object.Spawn<Monster>(new Vector3Int(0, 1, 0));
            monster.CreatureState = ECreatureState.Idle;
        }

        return true;
    }
    public override void Clear()
    {

    }
}
