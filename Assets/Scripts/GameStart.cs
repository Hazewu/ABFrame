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
    }

    private void Start()
    {
        m_audioClip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameResources/Audioclips/Battle.mp3");
        m_audio.clip = m_audioClip;
        m_audio.Play();
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
}
