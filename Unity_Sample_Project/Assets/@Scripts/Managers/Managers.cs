using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    // 하위 매니저를 관리
    // 다른 신으로 가도 사라지지 않도록 설정하며, 자기 자신을 생성
    private static Managers s_instance;
    private static Managers Instance {  get { Init(); return s_instance; } }

    #region Contents
    private GameManager _game = new GameManager();
    private ObjectManager _object = new ObjectManager();

    public static GameManager Game { get { return Instance?._game; } }
    public static ObjectManager Object { get { return Instance?._object; } }

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

    public static void Init()
    {
        if(s_instance == null)
        {
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
