using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class UI_WaypointPopup : UI_Popup
{
    enum GameObjects
    {
        WaypointList
    }

    enum Buttons
    {
        CloseButton,
    }

    List<UI_StageItem> _items = new List<UI_StageItem>();

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        
        GetButton((int)Buttons.CloseButton).gameObject.BindEvent(OnClickCloseButton);

        Refresh();
        return true;
    }

    public void SetInfo()
    {
		Refresh();
    }

    // 인벤토리 등, '갱신'용 함수는 따로 빼두면 쓸일이 많음
    // UI 부분과 데이터 부분을 분리하여 관리하는 것이 유용
    void Refresh()
    {
        if (_init == false)
            return;

        _items.Clear();

		GameObject parent = GetObject((int)GameObjects.WaypointList);

        foreach (var stage in Managers.Map.StageTransition.Stages)
        {
            // 동적 생성
            // 실제로는 최대개수를 미리 만들어 두는 편이 더 효율적
            UI_StageItem item = Managers.UI.MakeSubItem<UI_StageItem>(parent.transform);

            item.SetInfo(stage, () =>
            {
                Managers.UI.ClosePopupUI(this);
            });

            _items.Add(item);
		}
    }

    void OnClickCloseButton(PointerEventData evt)
    {
        Managers.UI.ClosePopupUI(this);
    }
}

