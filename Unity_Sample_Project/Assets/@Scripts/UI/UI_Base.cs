using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Base : InitBase
{
    // 딕셔러니 : c++ 의 map 같이 key , value 로 관리되는 자료구조
    // Object[] : '배열'을 받는다
    protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();

    // [SerializeField] 나 public 을 통하여 에디터에서 일일이 자원을 집어넣는 방식은
    // 차후 관리를 어렵게 만들 수 있으므로 주의가 필요하다

    private void Awake()
    {
        Init();
    }

    // 주어진 열거형 타입의 이름을 사용하여
    // 해당 타입을 가진 
    protected void Bind<T>(Type type) where T : UnityEngine.Object
    {
        // type을 string으로 받으며, 해당 열거형의 '모든 요소'를 문자열 배열로 받는다
        // ex : 해당 Enum 안에 a,b,c 라는 요소가 있다면 {"a","b","c"}를 return 받음
        string[] names = Enum.GetNames(type);
        // 그 크기로 Object[] 로 잡고 생성한다
        UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
        // 이후 딕셔너리에 열거형 타입과 함께 넣어준다
        _objects.Add(typeof(T), objects);

        // 열거형 이름의 개수만큼
        // 반복하여 탐색
        // * 자식 오브젝트의 이름이 '열거형'의 이름 중 하나여야 하기에 선행 정책 필요
        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objects[i] = Util.FindChild(gameObject, names[i], true);
            else
                objects[i] = Util.FindChild<T>(gameObject, names[i], true);

            if (objects[i] == null)
                Debug.Log($"Failed to bind({names[i]})");
        }
    }

    protected void BindObjects(Type type) { Bind<GameObject>(type); }
    protected void BindImages(Type type) { Bind<Image>(type); }
    protected void BindTexts(Type type) { Bind<TMP_Text>(type); }
    protected void BindButtons(Type type) { Bind<Button>(type); }
    protected void BindToggles(Type type) { Bind<Toggle>(type); }

    protected T Get<T>(int idx) where T : UnityEngine.Object
    {
        UnityEngine.Object[] objects = null;
        if (_objects.TryGetValue(typeof(T), out objects) == false)
            return null;

        return objects[idx] as T;
    }

    protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
    protected TMP_Text GetText(int idx) { return Get<TMP_Text>(idx); }
    protected Button GetButton(int idx) { return Get<Button>(idx); }
    protected Image GetImage(int idx) { return Get<Image>(idx); }
    protected Toggle GetToggle(int idx) { return Get<Toggle>(idx); }

    public static void BindEvent(GameObject go, Action<PointerEventData> action = null, Define.EUIEvent type = Define.EUIEvent.Click)
    {
        // type 의 이벤트 발생 시,
        // action을 호출시키는 일종의 Delegate 등록
        UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

        switch (type)
        {
            case Define.EUIEvent.Click:
                evt.OnClickHandler -= action;
                evt.OnClickHandler += action;
                break;
            case Define.EUIEvent.PointerDown:
                evt.OnPointerDownHandler -= action;
                evt.OnPointerDownHandler += action;
                break;
            case Define.EUIEvent.PointerUp:
                evt.OnPointerUpHandler -= action;
                evt.OnPointerUpHandler += action;
                break;
            case Define.EUIEvent.Drag:
                evt.OnDragHandler -= action;
                evt.OnDragHandler += action;
                break;
        }
    }
}
