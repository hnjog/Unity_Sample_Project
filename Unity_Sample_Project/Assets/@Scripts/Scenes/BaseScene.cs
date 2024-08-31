using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class BaseScene : InitBase
{
    public EScene SceneType { get; protected set; } = EScene.Unknown;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));

        // UI 먹통되지 않도록 EventSystem 체크
        if(obj == null)
        {
            GameObject go = new GameObject() { name = "@EventSystem" };
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        return true;
    }
}
