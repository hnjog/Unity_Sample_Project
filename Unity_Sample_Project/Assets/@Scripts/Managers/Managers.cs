using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    // 절대적으로 한번만 초기화 하기 위함 (일부 강제 종료 등의 상황에서 재 생성 되는 것을 막기 위함)
    public static bool Initialized { get; set; } = false;

    // 하위 매니저를 관리
    // 다른 신으로 가도 사라지지 않도록 설정하며, 자기 자신을 생성
    private static Managers s_instance;
    private static Managers Instance {  get { Init(); return s_instance; } }

    #region Contents
    private GameManager _game = new GameManager();
    private ObjectManager _object = new ObjectManager();
    private MapManager _map = new MapManager();
    private InventoryManager _inventory = new InventoryManager();

    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }
    public static MapManager Map { get { return Instance?._map; } }
    public static InventoryManager Inventory { get { return Instance?._inventory; } }

    #endregion

    #region Core
    private DataManager _data = new DataManager();
    private PoolManager _pool = new PoolManager();
    private ResourceManager _resource = new ResourceManager();
    private SceneManagerEx _scene = new SceneManagerEx();
    private SoundManager _sound = new SoundManager();
    private UIManager _ui = new UIManager();
    
    public static DataManager Data { get { return Instance?._data; } }
    public static PoolManager Pool { get {  return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SceneManagerEx Scene { get {  return Instance?._scene; } }
    public static SoundManager Sound { get {  return Instance?._sound; } }
    public static UIManager UI { get {  return Instance?._ui; } }
    #endregion

    #region Language
    private static Define.ELanguage _language = Define.ELanguage.Korean;
    public static Define.ELanguage Language
    {
        get { return _language; }
        set
        {
            _language = value;
        }
    }

    // 텍스트를 사용할때, 이걸 사용하여 세팅된 텍스트 데이터를 가져온다
    public static string GetText(string textId)
    {
        switch (_language)
        {
            case Define.ELanguage.Korean:
                return Managers.Data.TextDic[textId].KOR;
            case Define.ELanguage.English:
                break;
            case Define.ELanguage.French:
                break;
            case Define.ELanguage.SimplifiedChinese:
                break;
            case Define.ELanguage.TraditionalChinese:
                break;
            case Define.ELanguage.Japanese:
                break;
        }

        return "";
    }
    #endregion

    public static void Init()
    {
        if(s_instance == null && Initialized == false)
        {
            Initialized = true;

            GameObject gameObject = GameObject.Find("@Managers");
            if(gameObject == null )
            {
                gameObject = new GameObject { name = "@Managers" };
                gameObject.AddComponent<Managers>();
            }

            // 새로운 신으로 로드될때 파괴되지 않도록 설정
            // 게임 전반에 유지되는 오브젝트에 사용
            DontDestroyOnLoad(gameObject);

            // 초기화
            s_instance = gameObject.AddComponent<Managers>();
        }
    }
}
