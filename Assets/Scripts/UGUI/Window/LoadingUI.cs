using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUI : HaWindow
{
    private LoadingPanel m_MainPanel;
    private string m_SceneName;

    /// <summary>
    /// 第一个参数传需要加载的场景名字
    /// </summary>
    /// <param name="paramArr"></param>
    public override void Awake(params object[] paramArr)
    {
        m_MainPanel = WndTrans.GetComponent<LoadingPanel>();
        m_SceneName = (string)paramArr[0];
    }

    public override void OnUpdate()
    {
        if (m_MainPanel == null)
            return;
        m_MainPanel.m_Slider.value = GameMapManager.LoadingProgress / 100.0f;
        m_MainPanel.m_TextProgress.text = string.Format("{0}%", GameMapManager.LoadingProgress);
        if (GameMapManager.LoadingProgress >= 100)
        {
            Debug.Log(m_SceneName + " 场景加载完成");
            LoadOtherScene();
        }
    }

    /// <summary>
    /// 加载对应场景第一个UI
    /// </summary>
    private void LoadOtherScene()
    {
        // 根据场景名字打开对应场景第一个界面
        if (m_SceneName == ConstStr_UI.SCENE_MENU)
        {
            UIManager.Instance.PopUpWnd(ConstStr_UI.PREFAB_PANEL_MENU);
        }
        // 关闭加载UI
        UIManager.Instance.CloseWnd(ConstStr_UI.PREFAB_PANEL_LOADING);
    }
}
