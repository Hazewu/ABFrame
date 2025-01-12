using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUI : HaWindow
{
    private MenuPanel m_MainPanel;

    public override void Awake(params object[] paramArr)
    {
        m_MainPanel = WndObj.GetComponent<MenuPanel>();
        AddButtonClickListener(m_MainPanel.m_StartBtn, OnStartBtnClicked);
        AddButtonClickListener(m_MainPanel.m_LoadBtn, OnLoadBtnClicked);
        AddButtonClickListener(m_MainPanel.m_ExitBtn, OnExitBtnClicked);
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
