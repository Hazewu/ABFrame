using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public enum UIMsgID
{
    None = 0,
}


public class UIManager : Singleton<UIManager>
{
    // UI���ڵ�
    private RectTransform m_UIRoot;
    // ���ڸ��ڵ�
    private RectTransform m_WndRoot;
    // UI�����
    private Camera m_UICamera;
    // EventSystem�ڵ㣬Ҫ�����UI���������Ӧ
    private EventSystem m_EventSystem;
    // ��Ļ�Ŀ�߱�
    private float m_CanvasRate = 0;

    // UI�Ĵ���·��
    private const string UI_PREFAB_PATH = "Assets/GameResources/Prefabs/UGUI/Panel/";
    // ע��Ĵ���
    private Dictionary<string, System.Type> m_RegisterDic = new Dictionary<string, System.Type>();
    // ���д򿪵Ĵ���
    private Dictionary<string, HaWindow> m_WindowDic = new Dictionary<string, HaWindow>();
    // ���д򿪵Ĵ����б�
    private List<HaWindow> m_WindowList = new List<HaWindow>();

    /// <summary>
    /// ��ʼ��
    /// </summary>
    /// <param name="uiRoot"></param>
    /// <param name="wndRoot"></param>
    /// <param name="uiCamera"></param>
    /// <param name="eventSystem"></param>
    public void Init(RectTransform uiRoot, RectTransform wndRoot, Camera uiCamera, EventSystem eventSystem)
    {
        m_UIRoot = uiRoot;
        m_WndRoot = wndRoot;
        m_UICamera = uiCamera;
        m_EventSystem = eventSystem;
        m_CanvasRate = Screen.height / (m_UICamera.orthographicSize * 2);
    }

    /// <summary>
    /// ��ʾ������������UI
    /// </summary>
    /// <param name="show"></param>
    public void ShowOrHideUIRoot(bool show)
    {
        if (m_UIRoot != null && m_UIRoot.gameObject.activeSelf != show)
        {
            m_UIRoot.gameObject.SetActive(show);
        }
    }

    /// <summary>
    /// ����Ĭ��ѡ�����
    /// </summary>
    /// <param name="obj"></param>
    public void SetNormalSelectObj(GameObject obj)
    {
        if (m_EventSystem == null)
        {
            m_EventSystem = EventSystem.current;
        }
        m_EventSystem.firstSelectedGameObject = obj;
    }

    /// <summary>
    /// �������д򿪵Ĵ���
    /// </summary>
    public void OnUpdate()
    {
        for (int i = 0; i < m_WindowList.Count; i++)
        {
            if (m_WindowList[i] != null)
            {
                m_WindowList[i].OnUpdate();
            }
        }
    }

    /// <summary>
    /// ���ݴ��������Ҵ���
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T FindWndByName<T>(string name) where T : HaWindow
    {
        HaWindow wnd = null;
        if (m_WindowDic.TryGetValue(name, out wnd))
        {
            return (T)wnd;
        }
        return null;
    }

    public HaWindow PopUpWnd(string wndName, bool bTop = true, params object[] paramArr)
    {
        HaWindow wnd = FindWndByName<HaWindow>(wndName);
        if (wnd == null)
        {
            System.Type type = null;
            if (m_RegisterDic.TryGetValue(wndName, out type))
            {
                // �������ͣ�ʵ������
                wnd = System.Activator.CreateInstance(type) as HaWindow;
            }
            else
            {
                Debug.LogError("�Ҳ������ڶ�Ӧ�Ľű����������ǣ�" + wndName);
                return null;
            }

            GameObject wndObj = ObjectManager.Instance.InstantiateObject(UI_PREFAB_PATH + wndName, false, false);
            if (wndObj == null)
            {
                Debug.LogError("��������Prefabʧ�ܣ�" + wndName);
                return null;
            }

            if (!m_WindowDic.ContainsKey(wndName))
            {
                m_WindowDic.Add(wndName, wnd);
                m_WindowList.Add(wnd);
            }

            wnd.WndObj = wndObj;
            wnd.WndTrans = wndObj.transform;
            wnd.Name = wndName;
            wnd.Awake(paramArr);
            // ��ӵ����ڸ��ڵ�
            wndObj.transform.SetParent(m_WndRoot, false);

            ShowWnd(wnd, bTop, paramArr);
        }
        else
        {
            ShowWnd(wndName, bTop, paramArr);
        }

        return wnd;
    }
    /// <summary>
    /// ����ע�᷽��
    /// </summary>
    /// <typeparam name="T">���ڷ�����</typeparam>
    /// <param name="name">������</param>
    public void Register<T>(string name) where T : HaWindow
    {
        if (!m_RegisterDic.ContainsKey(name))
        {
            m_RegisterDic.Add(name, typeof(T));
        }
    }

    /// <summary>
    /// ������Ϣ������
    /// </summary>
    /// <param name="name"></param>
    /// <param name="msgID"></param>
    /// <param name="paramArr"></param>
    /// <returns></returns>
    public bool SendMessageToWnd(string name, UIMsgID msgID = 0, params object[] paramArr)
    {
        HaWindow wnd = FindWndByName<HaWindow>(name);
        if (wnd != null)
        {
            return wnd.OnMessage(msgID, paramArr);
        }
        return false;
    }

    /// <summary>
    /// ���ݴ���������ʾ����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bTop"></param>
    /// <param name="paramArr"></param>
    public void ShowWnd(string name, bool bTop = true, params object[] paramArr)
    {
        HaWindow wnd = FindWndByName<HaWindow>(name);
        ShowWnd(wnd, bTop, paramArr);
    }

    /// <summary>
    /// ���ݴ��ڶ�����ʾ����
    /// </summary>
    /// <param name="wnd"></param>
    /// <param name="bTop"></param>
    /// <param name="paramArr"></param>
    public void ShowWnd(HaWindow wnd, bool bTop, params object[] paramArr)
    {
        if (wnd != null)
        {
            if (wnd.WndObj != null && !wnd.WndObj.activeSelf)
            {
                wnd.WndObj.SetActive(true);
            }
            if (bTop)
            {
                // �Ƶ������Ⱦ������ʾ��������
                wnd.WndTrans.SetAsLastSibling();
            }
            wnd.OnShow(paramArr);
        }
    }

    /// <summary>
    /// ���ݴ������ֹرմ���
    /// </summary>
    /// <param name="name"></param>
    /// <param name="destroy"></param>
    public void CloseWnd(string name, bool destroy = false)
    {
        HaWindow wnd = FindWndByName<HaWindow>(name);
        CloseWnd(wnd, destroy);
    }

    /// <summary>
    /// ���ݴ��ڶ���رմ���
    /// </summary>
    /// <param name="wnd"></param>
    /// <param name="destroy"></param>
    public void CloseWnd(HaWindow wnd, bool destroy = false)
    {
        if (wnd != null)
        {
            wnd.OnDisabled();
            wnd.OnClose();
            if (m_WindowDic.ContainsKey(wnd.Name))
            {
                m_WindowDic.Remove(wnd.Name);
                m_WindowList.Remove(wnd);
            }

            if (destroy)
            {
                ObjectManager.Instance.ReleaseObject(wnd.WndObj, 0, true);
            }
            else
            {
                ObjectManager.Instance.ReleaseObject(wnd.WndObj, -1, false, false);
            }
            wnd.WndObj = null;
            wnd = null;
        }
    }

    /// <summary>
    /// �ر����д���
    /// </summary>
    public void CloseAllWnd()
    {
        for (int i = m_WindowList.Count - 1; i >= 0; i--)
        {
            CloseWnd(m_WindowList[i]);
        }
    }

    /// <summary>
    /// �л���Ψһ����
    /// </summary>
    /// <param name="name"></param>
    /// <param name="bTop"></param>
    /// <param name="paramArr"></param>
    public void SwitchStateByName(string name, bool bTop = true, params object[] paramArr)
    {
        CloseAllWnd();
        PopUpWnd(name, bTop, paramArr);
    }

    /// <summary>
    /// �����������ش���
    /// </summary>
    /// <param name="name"></param>
    public void HideWnd(string name)
    {
        HaWindow wnd = FindWndByName<HaWindow>(name);
        HideWnd(wnd);
    }

    /// <summary>
    /// ���ݴ��ڶ������ش���
    /// </summary>
    /// <param name="wnd"></param>
    public void HideWnd(HaWindow wnd)
    {
        if (wnd != null)
        {
            wnd.WndObj.SetActive(false);
            wnd.OnDisabled();
        }
    }
}
