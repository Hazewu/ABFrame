using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HaWindow
{
    // 引用的GameObject
    public GameObject WndObj { get; set; }
    // 引用的Transform
    public Transform WndTrans { get; set; }
    // 窗口名字
    public string Name { get; set; }
    // 所有的Button
    private List<Button> m_AllBtns = new List<Button>();
    // 所有的Toggle
    private List<Toggle> m_AllToggles = new List<Toggle>();

    public virtual void Awake(params object[] paramArr) { }

    public virtual void OnShow(params object[] paramArr) { }

    public virtual void OnUpdate() { }

    public virtual void OnClose()
    {
        RemoveAllButoonListeners();
        RemoveAllToggleListeners();
        m_AllBtns.Clear();
        m_AllToggles.Clear();
    }

    public virtual void OnDisabled() { }

    /// <summary>
    /// 当有消息传来，处理
    /// </summary>
    /// <param name="msgID"></param>
    /// <param name="paramArr"></param>
    /// <returns></returns>
    public virtual bool OnMessage(UIMsgID msgID, params object[] paramArr)
    {
        return true;
    }

    /// <summary>
    /// 移除所有的button事件
    /// </summary>
    public void RemoveAllButoonListeners()
    {
        foreach (Button btn in m_AllBtns)
        {
            btn.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// 移除所有的toggle事件
    /// </summary>
    public void RemoveAllToggleListeners()
    {
        foreach (Toggle toggle in m_AllToggles)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }
    }

    /// <summary>
    /// 添加button事件监听
    /// </summary>
    /// <param name="btn"></param>
    /// <param name="action"></param>
    public void AddButtonClickListener(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn != null)
        {
            if (!m_AllBtns.Contains(btn))
            {
                m_AllBtns.Add(btn);
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action);
            btn.onClick.AddListener(BtnnPlaySound);
        }
    }

    private void BtnnPlaySound()
    {

    }

    /// <summary>
    /// 添加toggle事件监听
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="action"></param>
    public void AddToggleClickListener(Toggle toggle, UnityEngine.Events.UnityAction<bool> action)
    {
        if (toggle != null)
        {
            if (!m_AllToggles.Contains(toggle))
            {
                m_AllToggles.Add(toggle);
            }
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener(action);
            toggle.onValueChanged.AddListener(TogglePlaySound);
        }
    }

    private void TogglePlaySound(bool isOn)
    {

    }

    /// <summary>
    /// 同步替换图片
    /// </summary>
    /// <param name="path"></param>
    /// <param name="img"></param>
    /// <param name="setNativeSize"></param>
    /// <returns></returns>
    public bool ChangeImageSprite(string path, Image img, bool setNativeSize = false)
    {
        if (img == null) return false;

        Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
        if (sp != null)
        {
            if (img.sprite != null)
                img.sprite = null;

            img.sprite = sp;
            if (setNativeSize)
            {
                img.SetNativeSize();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 异步替换图片
    /// </summary>
    /// <param name="path"></param>
    /// <param name="img"></param>
    /// <param name="setNativeSize"></param>
    public void ChangeImageSpriteAsync(string path, Image img, bool setNativeSize = false)
    {
        if (img == null) return;
        ResourceManager.Instance.AsyncLoadResource(path, OnLoadSpriteFinish, LoadResPriority.RES_MIDDLE, img, setNativeSize);
    }

    /// <summary>
    /// 图片加载完成
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    private void OnLoadSpriteFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        if (obj != null)
        {
            Sprite sp = obj as Sprite;
            Image img = param1 as Image;
            bool setNativeSize = (bool)param2;
            if (img.sprite != null)
                img.sprite = null;

            img.sprite = sp;
            if (setNativeSize)
            {
                img.SetNativeSize();
            }
        }
    }
}
