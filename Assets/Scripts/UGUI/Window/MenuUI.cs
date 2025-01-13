using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : HaWindow
{
    private MenuPanel m_MainPanel;
    private AudioClip m_Clip;

    public override void Awake(params object[] paramArr)
    {
        m_MainPanel = WndObj.GetComponent<MenuPanel>();
        AddButtonClickListener(m_MainPanel.m_StartBtn, OnStartBtnClicked);
        AddButtonClickListener(m_MainPanel.m_LoadBtn, OnLoadBtnClicked);
        AddButtonClickListener(m_MainPanel.m_ExitBtn, OnExitBtnClicked);

        //m_Clip = ResourceManager.Instance.LoadResource<AudioClip>(ConstStr_Sound.BGM_MENU);
        //m_MainPanel.m_AudioSource.clip = m_Clip;
        //m_MainPanel.m_AudioSource.Play();

        ResourceManager.Instance.AsyncLoadResource("Assets/GameResources/UI/Escape.png", OnLoadSpriteTest1, LoadResPriority.RES_LOW, true);
        ResourceManager.Instance.AsyncLoadResource("Assets/GameResources/UI/Head.png", OnLoadSpriteTest2, LoadResPriority.RES_HIGH, true);
    }

    private void OnLoadSpriteTest1(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Img1.sprite = sp;
            Debug.Log("ͼƬ1���س�����");
        }
    }

    private void OnLoadSpriteTest2(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Img2.sprite = sp;
            Debug.Log("ͼƬ2���س�����");
        }
    }

    //public override void OnUpdate()
    //{
    //    if (Input.GetKeyDown(KeyCode.A) && m_Clip != null)
    //    {
    //        Debug.Log("AAAAAAAAAAA");
    //        ResourceManager.Instance.ReleaseResource(m_Clip, true);
    //        m_MainPanel.m_AudioSource.clip = null;
    //        m_Clip = null;
    //    }
    //}

    private void OnStartBtnClicked()
    {
        Debug.Log("����˿�ʼ��Ϸ��");
    }

    private void OnLoadBtnClicked()
    {
        Debug.Log("����˼�����Ϸ��");
    }

    private void OnExitBtnClicked()
    {
        Debug.Log("������˳���Ϸ��");
    }
}
