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

        m_Clip = ResourceManager.Instance.LoadResource<AudioClip>(ConstStr_Sound.BGM_MENU);
        m_MainPanel.m_AudioSource.clip = m_Clip;
        m_MainPanel.m_AudioSource.Play();
    }

    public override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A) && m_Clip != null)
        {
            Debug.Log("AAAAAAAAAAA");
            ResourceManager.Instance.ReleaseResource(m_Clip, true);
            m_MainPanel.m_AudioSource.clip = null;
            m_Clip = null;
        }
    }

    private void OnStartBtnClicked()
    {
        Debug.Log("点击了开始游戏！");
    }

    private void OnLoadBtnClicked()
    {
        Debug.Log("点击了加载游戏！");
    }

    private void OnExitBtnClicked()
    {
        Debug.Log("点击了退出游戏！");
    }
}
