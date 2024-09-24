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

        HeroCamp camp = Managers.Object.Spawn<HeroCamp>(new Vector3(-10, -5, 0), 0);

        for (int i = 0; i <5; i++)
        {
            // 게임 신에서 직접 생성하면 관리가 어려워짐
            // 생성용 클래스
            //int heroTemplateID = HERO_WIZARD_ID + Random.Range(0,5);
            int heroTemplateID = HERO_WIZARD_ID;
            Hero temp = Managers.Object.Spawn<Hero>(new Vector3(-10 + Random.Range(-5,5), -5 + Random.Range(-5, 5), 0), heroTemplateID);
        }

        CameraController cameraController = Camera.main.GetOrAddComponent<CameraController>();
        cameraController.Target = camp;

        Managers.UI.ShowBaseUI<UI_Joystick>();

        {
            //Managers.Object.Spawn<Monster>(new Vector3Int(0, 1, 0), MONSTER_BEAR_ID);
            //Managers.Object.Spawn<Monster>(new Vector3Int(1, 1, 0), MONSTER_SLIME_ID);
            Managers.Object.Spawn<Monster>(new Vector3(3, 1, 0), MONSTER_GOBLIN_ARCHER_ID);
        }

        {
            Env env = Managers.Object.Spawn<Env>(new Vector3(0,2,0),ENV_TREE1_ID);
            env.EnvState = EEnvState.Idle;
        }

        return true;
    }
    public override void Clear()
    {

    }
}
