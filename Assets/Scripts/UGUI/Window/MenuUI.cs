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
        LoadMonsterData();
    }

    private void LoadMonsterData()
    {
        MonsterData monsterData = ConfigManager.Instance.FindData<MonsterData>(CFG.TABLE_MONSTER);
        foreach (MonsterBase data in monsterData.AllMonster)
        {

            Debug.Log(string.Format("ID:{0} 名字: {1} 外观: {2} 高度: {3} 稀有度: {4}", data.Id,
                data.Name, data.OutLook, data.Height, data.Rare));
        }
    }

    private void OnLoadSpriteTest1(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Img1.sprite = sp;
            Debug.Log("图片1加载出来了");
        }
    }

    private void OnLoadSpriteTest2(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            m_MainPanel.m_Img2.sprite = sp;
            Debug.Log("图片2加载出来了");
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
