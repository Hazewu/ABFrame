using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{

    public AudioSource m_audio;
    private AudioClip m_audioClip;
    private void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
    }

    private void Start()
    {
        Debug.Log("1加载:" + System.DateTime.Now.Ticks);
        //// 同步加载
        //m_audioClip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameResources/Audioclips/Battle.mp3");
        //m_audio.clip = m_audioClip;
        //m_audio.Play();

        // 异步加载
        ResourceManager.Instance.AsyncLoadResource("Assets/GameResources/Audioclips/Battle.mp3", OnLoadFinish, LoadResPriority.RES_MIDDLE);
        Debug.Log("2加载:" + System.DateTime.Now.Ticks);
    }

    void OnLoadFinish(string path, Object obj, object param1, object param2, object param3)
    {
        m_audioClip = obj as AudioClip;
        m_audio.clip = m_audioClip;
        m_audio.Play();
        Debug.Log("加载完成:" + System.DateTime.Now.Ticks);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && m_audioClip)
        {
            m_audio.Stop();
            m_audio.clip = null;
            ResourceManager.Instance.ReleaseResource(m_audioClip, true);
            m_audioClip = null;
        }
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }
}
