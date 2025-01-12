using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMapManager : Singleton<GameMapManager>
{
    // 当前场景名
    public string CurrentMapName { get; set; }
    // 场景加载是否完成
    public bool AlreadyLoadScene { get; private set; }

    // 加载场景开始回调
    public Action LoadSceneStartCallBack;
    // 加载场景完成回调
    public Action LoadSceneOverCallBack;

    // 切换场景进度条
    public static int LoadingProgress = 0;

    private MonoBehaviour m_Mono;

    public void Init(MonoBehaviour mono)
    {
        m_Mono = mono;
    }

    /// <summary>
    /// 设置场景环境
    /// </summary>
    /// <param name="name"></param>
    private void SetSceneSetting(string name)
    {
        // 设置各种场景环境，可以根据配表来
    }

    public void LoadScene(string name)
    {
        LoadingProgress = 0;
        m_Mono.StartCoroutine(LoadSceneAsync(name));
        UIManager.Instance.PopUpWnd(ConstStr_UI.PREFAB_PANEL_LOADING, true, name);
    }

    private IEnumerator LoadSceneAsync(string name)
    {
        if (LoadSceneStartCallBack != null)
        {
            LoadSceneStartCallBack();
        }
        ClearCache();
        AlreadyLoadScene = false;
        // 加载空场景
        AsyncOperation unLoadScene = SceneManager.LoadSceneAsync("Empty", LoadSceneMode.Single);
        while (unLoadScene != null && !unLoadScene.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        // 加载目标场景
        LoadingProgress = 0;
        int targetProgress = 0;
        AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);

        // 未加载完成
        if (asyncScene != null && !asyncScene.isDone)
        {
            asyncScene.allowSceneActivation = false;
            // 还差得多
            while (asyncScene.progress < 0.9f)
            {
                targetProgress = (int)asyncScene.progress * 100;
                yield return new WaitForEndOfFrame();
                // 平滑过渡
                while (LoadingProgress < targetProgress)
                {
                    ++LoadingProgress;
                    yield return new WaitForEndOfFrame();
                }
            }
            CurrentMapName = name;
            SetSceneSetting(name);
            // 自行加载剩余的10%
            targetProgress = 100;
            // 98表示基本上要搞完了
            while (LoadingProgress < targetProgress - 2)
            {
                ++LoadingProgress;
                yield return new WaitForEndOfFrame();
            }
            LoadingProgress = 100;
            asyncScene.allowSceneActivation = true;
            AlreadyLoadScene = true;
            if (LoadSceneOverCallBack != null)
            {
                LoadSceneOverCallBack();
            }
        }
        yield return null;
    }

    /// <summary>
    /// 跳场景需要清除的东西
    /// </summary>
    private void ClearCache()
    {
        ObjectManager.Instance.ClearCache();
        ResourceManager.Instance.ClearCache();
    }
}
