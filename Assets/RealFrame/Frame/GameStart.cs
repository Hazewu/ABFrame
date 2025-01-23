using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#region UI 加载框架
public class GameStart : MonoSingleton<GameStart>
{
    protected override void Awake()
    {
        base.Awake();
        // 切换场景时，不销毁GameStart
        DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    private void Start()
    {
        LoadConfig();
        UIManager.Instance.Init(
            transform.Find("UIRoot") as RectTransform,
            transform.Find("UIRoot/WndRoot") as RectTransform,
            transform.Find("UIRoot/UICamera").GetComponent<Camera>(),
            transform.Find("UIRoot/EventSystem").GetComponent<EventSystem>()
        );
        RegisterUI();

        GameMapManager.Instance.Init(this);

        //ObjectManager.Instance.PreloadGameObject(ConstStr_Obj.PREFAB_DOOR, 5);
        //ResourceManager.Instance.PreloadRes(ConstStr_Sound.BGM_MENU);

        GameMapManager.Instance.LoadScene(ConstStr_UI.SCENE_MENU);
    }

    private void LoadConfig()
    {
        ConfigManager.Instance.LoadData<MonsterData>(CFG.TABLE_MONSTER);
        ConfigManager.Instance.LoadData<BuffData>(CFG.TABLE_BUFF);
        ConfigManager.Instance.LoadData<Poetry>(CFG.TABLE_POETRY);
    }

    private void RegisterUI()
    {
        UIManager.Instance.Register<MenuUI>(ConstStr_UI.PREFAB_PANEL_MENU);
        UIManager.Instance.Register<LoadingUI>(ConstStr_UI.PREFAB_PANEL_LOADING);
    }

    private void Update()
    {
        UIManager.Instance.OnUpdate();
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        //ResourceManager.Instance.ClearCache();
        Resources.UnloadUnusedAssets();
        Debug.Log("清空编辑器缓存");
#endif
    }
}
#endregion




#region test ObjectManager异步加载
//public class GameStart : MonoBehaviour
//{
//    private GameObject m_obj;
//    private void Awake()
//    {
//        // 切换场景时，不销毁GameStart
//        DontDestroyOnLoad(gameObject);
//        AssetBundleManager.Instance.LoadAssetBundleConfig();
//        ResourceManager.Instance.Init(this);
//        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
//    }

//    private void Start()
//    {
//        ObjectManager.Instance.InstantiateObjectAsync("Assets/GameResources/Prefabs/Door.prefab", true, true, OnLoadFinish, LoadResPriority.RES_HIGH);
//    }

//    private void OnLoadFinish(string path, Object obj, object param1, object param2, object param3)
//    {
//        m_obj = obj as GameObject;
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.A) && m_obj)
//        {
//            // 缓存，不销毁
//            ObjectManager.Instance.ReleaseObject(m_obj);
//            m_obj = null;
//        }
//        else if (Input.GetKeyDown(KeyCode.D) && m_obj == null)
//        {
//            ObjectManager.Instance.InstantiateObjectAsync("Assets/GameResources/Prefabs/Left.prefab", true, true, OnLoadFinish, LoadResPriority.RES_HIGH);
//        }
//        else if (Input.GetKeyDown(KeyCode.S) && m_obj)
//        {
//            // 销毁
//            ObjectManager.Instance.ReleaseObject(m_obj, 0, true);
//            m_obj = null;
//        }
//        else if (Input.GetKeyDown(KeyCode.W) && m_obj)
//        {
//            // 隐藏
//            ObjectManager.Instance.ReleaseObject(m_obj, -1, false, false);
//            m_obj = null;
//        }
//    }

//    private void OnApplicationQuit()
//    {
//#if UNITY_EDITOR
//        //ResourceManager.Instance.ClearCache();
//        Resources.UnloadUnusedAssets();
//        Debug.Log("清空编辑器缓存");
//#endif
//    }
//}
#endregion


#region test ObjectManager同步加载 和 预加载
//public class GameStart : MonoBehaviour
//{
//    private GameObject obj;
//    private void Awake()
//    {
//        // 切换场景时，不销毁GameStart
//        DontDestroyOnLoad(gameObject);
//        AssetBundleManager.Instance.LoadAssetBundleConfig();
//        ResourceManager.Instance.Init(this);
//        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
//    }

//    private void Start()
//    {
//        //obj = ObjectManager.Instance.InstantiateObject("Assets/GameResources/Prefabs/Door.prefab");
//        ObjectManager.Instance.PreloadGameObject("Assets/GameResources/Prefabs/Door.prefab", 5);
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.A) && obj)
//        {
//            // 缓存，不销毁
//            ObjectManager.Instance.ReleaseObject(obj);
//            obj = null;
//        }
//        else if (Input.GetKeyDown(KeyCode.D) && obj == null)
//        {
//            obj = ObjectManager.Instance.InstantiateObject("Assets/GameResources/Prefabs/Door.prefab", true);
//        }
//        else if (Input.GetKeyDown(KeyCode.S) && obj)
//        {
//            // 销毁
//            ObjectManager.Instance.ReleaseObject(obj, 0, true);
//            obj = null;
//        }
//        else if (Input.GetKeyDown(KeyCode.W) && obj)
//        {
//            // 隐藏
//            ObjectManager.Instance.ReleaseObject(obj, -1, false, false);
//            obj = null;
//        }
//    }

//    private void OnApplicationQuit()
//    {
//#if UNITY_EDITOR
//        //ResourceManager.Instance.ClearCache();
//        Resources.UnloadUnusedAssets();
//        Debug.Log("清空编辑器缓存");
//#endif
//    }
//}

#endregion

#region test ResourceManager资源加载
//public class GameStart : MonoBehaviour
//{
//    public AudioSource m_audio;
//    private AudioClip m_audioClip;
//    private void Awake()
//    {
//        // 切换场景时，不销毁GameStart
//        DontDestroyOnLoad(gameObject);
//        AssetBundleManager.Instance.LoadAssetBundleConfig();
//        ResourceManager.Instance.Init(this);
//        ObjectManager.Instance.Init(transform.Find("RecyclePookTrs"), transform.Find("SceneTrs"));
//    }

//    private void Start()
//    {
//        //Debug.Log("1加载:" + System.DateTime.Now.Ticks);
//        //// 同步加载
//        //m_audioClip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameResources/Audioclips/Battle.mp3");
//        //m_audio.clip = m_audioClip;
//        //m_audio.Play();

//        //// 异步加载
//        //ResourceManager.Instance.AsyncLoadResource("Assets/GameResources/Audioclips/Battle.mp3", OnLoadFinish, LoadResPriority.RES_MIDDLE);
//        //Debug.Log("2加载:" + System.DateTime.Now.Ticks);

//        // 预加载
//        ResourceManager.Instance.PreloadRes("Assets/GameResources/Audioclips/Normal.mp3");
//    }

//    void OnLoadFinish(string path, Object obj, object param1, object param2, object param3)
//    {
//        m_audioClip = obj as AudioClip;
//        m_audio.clip = m_audioClip;
//        m_audio.Play();
//        Debug.Log("加载完成:" + System.DateTime.Now.Ticks);
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.A) && m_audioClip)
//        {
//            m_audio.Stop();
//            m_audio.clip = null;
//            ResourceManager.Instance.ReleaseResource(m_audioClip, true);
//            m_audioClip = null;
//        }
//        else if (Input.GetKeyDown(KeyCode.D) && !m_audioClip)
//        {
//            long time = System.DateTime.Now.Ticks;
//            m_audioClip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameResources/Audioclips/Normal.mp3");
//            Debug.Log("预加载时间：" + (System.DateTime.Now.Ticks - time));
//            m_audio.clip = m_audioClip;
//            m_audio.Play();
//        }
//    }

//    private void OnApplicationQuit()
//    {
//#if UNITY_EDITOR
//        //ResourceManager.Instance.ClearCache();
//        Resources.UnloadUnusedAssets();
//        Debug.Log("清空编辑器缓存");
//#endif
//    }
//}
#endregion
