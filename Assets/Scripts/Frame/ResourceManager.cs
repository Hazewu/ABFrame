﻿using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System.Threading;
using System.IO;
using UnityEditor.PackageManager.Requests;
using Unity.VisualScripting.Antlr3.Runtime;

public class ResourceManager : Singleton<ResourceManager>
{
    // 是否从AB包中加载资源
    private bool m_LoadFromAssetBundle = false;
    // 缓存引用计数为零的资源列表，游戏资源能快速加载，达到缓存最大的时候释放这个列表里面最早没用的资源
    private CMapList<ResourceItem> m_NoReferenceAssetMapList = new CMapList<ResourceItem>();
    // 缓存使用的资源列表
    private Dictionary<uint, ResourceItem> AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();

    // 中间类，回调类的类对象池
    private ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResParamPool = ObjectManager.Instance.GetOrCreateClassPool<AsyncLoadResParam>(50);
    private ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = ObjectManager.Instance.GetOrCreateClassPool<AsyncCallBack>(100);

    // Mono脚本用于开启协程
    private MonoBehaviour m_StartMono;
    // 正在异步加载的资源列表，分优先级存储
    private List<AsyncLoadResParam>[] m_LoadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.RES_NUM];
    // 正在异步加载的Dic
    private Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();

    // 最长连续卡着加载资源的时间，单位微秒
    private const long MAXLOADRESITEM = 200000;

    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
        {
            m_LoadingAssetList[i] = new List<AsyncLoadResParam>();
        }
        m_StartMono = mono;
        m_StartMono.StartCoroutine(AsyncLoadCor());
    }

    private ResourceItem GetCacheResourceItem(uint crc, int addRefCount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item))
        {
            if (item != null)
            {
                item.RefCount += addRefCount;
                item.m_LastUseTime = Time.realtimeSinceStartup;
            }
        }

        return item;
    }

    private void CacheResource(string path, ref ResourceItem item, uint crc, Object obj, int addRefCount = 1)
    {
        if (item == null)
        {
            Debug.LogError("ResourceItem is null, path: " + path);
        }

        if (obj == null)
        {
            Debug.LogError("ResourceLoad Fail: " + path);
        }

        item.m_Obj = obj;
        item.m_Guid = obj.GetInstanceID();
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addRefCount;
        ResourceItem oldItem = null;
        if (AssetDic.TryGetValue(item.m_Crc, out oldItem))
        {
            // 不能重复add，会报错，所以这里需要替换
            AssetDic[item.m_Crc] = item;
        }
        else
        {
            AssetDic.Add(item.m_Crc, item);
        }
    }

    /// <summary>
    /// 缓存太多，清除最早没有使用的资源
    /// </summary>
    private void WashOut()
    {
        //// 当，当前内存使用大于80%时，进行清除最早没用的资源
        //{
        //    if (m_NoReferenceAssetMapList.size() <= 0)
        //        return;

        //    ResourceItem item = m_NoReferenceAssetMapList.Back();
        //    DestroyResourceItem(item, true);
        //    m_NoReferenceAssetMapList.Pop();
        //}
    }


#if UNITY_EDITOR
    private T LoadAssetByEditor<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    public T LoadResource<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path)) return null;

        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);

        if (item != null)
        {
            return item.m_Obj as T;
        }

        T obj = null;

#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj as T;
            }
            else
            {
                obj = LoadAssetByEditor<T>(path);
            }
        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as T;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }

        CacheResource(path, ref item, crc, obj);

        return obj;
    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destroyCache"></param>
    private void DestroyResourceItem(ResourceItem item, bool destroyCache = false)
    {
        if (item == null || item.RefCount > 0) return;

        if (!destroyCache)
        {
            // 不销毁，移动到头部，当要清理缓存时，进行清理
            m_NoReferenceAssetMapList.InsertToHead(item);
            return;
        }

        // 一定执行
        if (!AssetDic.Remove(item.m_Crc))
        {
            return;
        }

        // 释放assetBundle引用
        AssetBundleManager.Instance.ReleaseAsset(item);
        if (item.m_Obj != null)
        {
            item.m_Obj = null;
#if UNITY_EDITOR
            // 卸载无引用的游离资源
            Resources.UnloadUnusedAssets();
#endif
        }
    }

    /// <summary>
    /// 对外提供obj的回收，前提这个obj是由LoadResource给出去的
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(Object obj, bool destroyObj = false)
    {
        if (obj == null) return false;

        ResourceItem item = null;
        foreach (ResourceItem res in AssetDic.Values)
        {
            if (res.m_Guid == obj.GetInstanceID())
            {
                item = res;
                break;
            }

        }
        if (item == null)
        {
            Debug.LogError("AssetDic里不存在该资源:" + obj.name + " 可能释放了多次");
            return false;
        }

        item.RefCount--;

        DestroyResourceItem(item, destroyObj);
        return true;
    }

    /// <summary>
    /// 对外提供资源的回收
    /// </summary>
    /// <param name="path"></param>
    /// <param name="destroyObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(string path, bool destroyObj = false)
    {
        if (string.IsNullOrEmpty(path)) return false;

        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError("AssetDic里不存在该资源:" + path + " 可能释放了多次");
        }

        item.RefCount--;

        DestroyResourceItem(item, destroyObj);
        return true;
    }

    /// <summary>
    /// 异步加载
    /// </summary>
    /// <returns></returns>
    IEnumerator AsyncLoadCor()
    {
        List<AsyncCallBack> callBackList = null;
        // 上一次yield的时间
        long lastYieldTime = System.DateTime.Now.Ticks;
        while (true)
        {
            bool hasYield = false;
            // 每一轮加载，高、中、低，而不是全部高执行完成后再去中、低
            for (int i = 0; i < (int)LoadResPriority.RES_NUM; i++)
            {
                List<AsyncLoadResParam> loadingList = m_LoadingAssetList[i];
                if (loadingList.Count <= 0)
                    continue;

                AsyncLoadResParam loadingItem = loadingList[0];
                loadingList.RemoveAt(0);
                callBackList = loadingItem.m_CallBackList;

                // 开始加载资源
                Object obj = null;
                ResourceItem item = null;
#if UNITY_EDITOR
                if (!m_LoadFromAssetBundle)
                {
                    item = AssetBundleManager.Instance.FindResourceItem(loadingItem.m_Crc);
                    // item.m_obj肯定是空的，因为在之前已经判断过了，没缓存
                    obj = LoadAssetByEditor<Object>(loadingItem.m_Path);
                    // 模拟异步加载
                    yield return new WaitForSeconds(0.5f);
                }
#endif
                if (obj == null)
                {
                    item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.m_Crc);
                    if (item != null && item.m_AssetBundle != null)
                    {
                        AssetBundleRequest abRequest = null;
                        if (loadingItem.m_IsSprite)
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);
                        }
                        else
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync(item.m_AssetName);
                        }
                        yield return abRequest;
                        if (abRequest.isDone)
                        {
                            obj = abRequest.asset;
                        }
                        lastYieldTime = System.DateTime.Now.Ticks;
                    }
                }
                // 加载完成，缓存
                CacheResource(loadingItem.m_Path, ref item, loadingItem.m_Crc, obj, callBackList.Count);

                // 执行回调
                for (int j = 0; j < callBackList.Count; j++)
                {
                    AsyncCallBack callBack = callBackList[j];
                    if (callBack != null && callBack.m_DealFinish != null)
                    {
                        callBack.m_DealFinish(loadingItem.m_Path, obj, callBack.m_Param1, callBack.m_Param2, callBack.m_Param3);
                        callBack.m_DealFinish = null;
                    }

                    // 回调完成，回收
                    callBack.Reset();
                    m_AsyncCallBackPool.Recycle(callBack);
                }

                // 所有回调执行完成，回收
                obj = null;
                callBackList.Clear();
                m_LoadingAssetDic.Remove(loadingItem.m_Crc);

                loadingItem.Reset();
                m_AsyncLoadResParamPool.Recycle(loadingItem);

                if (System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESITEM)
                {
                    yield return null;
                    lastYieldTime = System.DateTime.Now.Ticks;
                    hasYield = true;
                }
            }

            if (!hasYield || System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESITEM)
            {
                lastYieldTime = System.DateTime.Now.Ticks;
                yield return null;
            }
        }
    }

    /// <summary>
    /// 异步加载资源（仅仅是不需要实例化的资源，例如音频、图片等等）
    /// </summary>
    /// <param name="path"></param>
    /// <param name="dealFinish"></param>
    /// <param name="priority"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    /// <param name="crc"></param>
    public void AsyncLoadResource(string path, OnAsyncObjFinish dealFinish, LoadResPriority priority, object param1 = null, object param2 = null, object param3 = null, uint crc = 0)
    {
        if (crc == 0)
        {
            crc = CRC32.GetCRC32(path);
        }

        ResourceItem item = GetCacheResourceItem(crc);
        // 有缓存资源，可以直接使用
        if (item != null)
        {
            if (dealFinish != null)
            {
                dealFinish(path, item.m_Obj, param1, param2, param3);
            }
            return;
        }

        // 判断是否在加载中
        AsyncLoadResParam param = null;
        // 没有则创建新的
        if (!m_LoadingAssetDic.TryGetValue(crc, out param) || param == null)
        {
            param = m_AsyncLoadResParamPool.Spawn(true);
            param.m_Crc = crc;
            param.m_Path = path;
            param.m_Priority = priority;
            m_LoadingAssetDic.Add(crc, param);
            // 加入到优先级列表，等待
            m_LoadingAssetList[(int)priority].Add(param);
        }

        // 往回调列表中加回调
        AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
        callBack.m_DealFinish = dealFinish;
        callBack.m_Param1 = param1;
        callBack.m_Param2 = param2;
        callBack.m_Param3 = param3;
        param.m_CallBackList.Add(callBack);
    }

    /// <summary>
    /// 预加载
    /// </summary>
    /// <param name="path"></param>
    public void PreloadRes(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc, 0);
        if (item != null) return;

        // 未加载，需要进行加载
        Object obj = null;

#if UNITY_EDITOR
        if (!m_LoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item.m_Obj != null)
            {
                obj = item.m_Obj;
            }
            else
            {
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }

        CacheResource(path, ref item, crc, obj);

        // 预加载资源在跳场景时不清空缓存
        item.m_Clear = false;
        ReleaseResource(obj, false);
    }

    /// <summary>
    /// 清空缓存，跳场景时调用
    /// </summary>
    public void ClearCache()
    {
        List<ResourceItem> tempList = new List<ResourceItem>();

        foreach (ResourceItem item in AssetDic.Values)
        {
            if (item.m_Clear)
            {
                tempList.Add(item);
            }
        }

        foreach (ResourceItem item in tempList)
        {
            DestroyResourceItem(item, true);
        }
        tempList.Clear();
    }
}

/// <summary>
/// 双向链表节点
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkListNode<T> where T : class, new()
{
    // 前一个节点
    public DoubleLinkListNode<T> prev = null;
    // 后一个节点
    public DoubleLinkListNode<T> next = null;
    // 当前节点
    public T t = null;
}

/// <summary>
/// 双向链表结构
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkList<T> where T : class, new()
{
    // 表头
    public DoubleLinkListNode<T> Head = null;
    // 表尾
    public DoubleLinkListNode<T> Tail = null;
    // 双向链表结构类对象池
    private ClassObjectPool<DoubleLinkListNode<T>> m_DoubleLinkListNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkListNode<T>>(200);
    // 个数
    private int m_count = 0;

    public int Count
    {
        get
        {
            return m_count;
        }
    }

    /// <summary>
    /// 从头部添加数据
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToHead(T t)
    {
        DoubleLinkListNode<T> pNode = m_DoubleLinkListNodePool.Spawn(true);
        pNode.prev = null;
        pNode.next = null;
        pNode.t = t;
        return AddToHead(pNode);
    }

    /// <summary>
    /// 从头部添加数据
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToHead(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null) return null;

        pNode.prev = null;
        // 头节点为空
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            // 接起来
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }
        m_count++;
        return Head;
    }

    /// <summary>
    /// 从尾部添加数据
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToTail(T t)
    {
        DoubleLinkListNode<T> pNode = m_DoubleLinkListNodePool.Spawn(true);
        pNode.prev = null;
        pNode.next = null;
        pNode.t = t;
        return AddToTail(pNode);
    }

    /// <summary>
    /// 从尾部添加数据
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToTail(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null) return null;

        pNode.next = null;
        // 尾节点为空
        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            // 接起来
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }
        m_count++;
        return Tail;
    }

    /// <summary>
    /// 移除某个节点
    /// </summary>
    /// <param name="pNode"></param>
    public void RemoveNode(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null) return;

        if (pNode == Head)
        {
            Head = pNode.next;
        }

        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }

        // 非头节点
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }

        // 非尾节点
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }

        // 清空pNode
        pNode.next = pNode.prev = null;
        pNode.t = null;
        m_DoubleLinkListNodePool.Recycle(pNode);
        m_count--;
    }

    /// <summary>
    /// 把某个节点移动到头部
    /// </summary>
    /// <param name="pNode"></param>
    public void MoveToHead(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null || pNode == Head) return;

        // 是一个孤立节点，不处理
        if (pNode.prev == null && pNode.next == null) return;

        // 尾节点
        if (pNode == Tail)
        {
            Tail = pNode.prev;
        }

        // 非头节点
        if (pNode.prev != null)
        {
            pNode.prev.next = pNode.next;
        }

        // 非尾节点
        if (pNode.next != null)
        {
            pNode.next.prev = pNode.prev;
        }

        pNode.prev = null;
        pNode.next = Head;
        Head = pNode;

        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

public class CMapList<T> where T : class, new()
{
    DoubleLinkList<T> m_DLink = new DoubleLinkList<T>();
    // 以T的值为key，保证了Node值的唯一性
    Dictionary<T, DoubleLinkListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkListNode<T>>();

    /// <summary>
    /// 插入一个节点到表头
    /// </summary>
    /// <param name="t"></param>
    public void InsertToHead(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            m_DLink.AddToHead(node);
            return;
        }
        m_DLink.AddToHead(t);
        m_FindMap.Add(t, m_DLink.Head);
    }

    /// <summary>
    /// 从尾部弹出数据
    /// </summary>
    /// <returns></returns>
    public T Pop()
    {
        T rtn = null;
        if (m_DLink != null)
        {
            rtn = m_DLink.Tail.t;
            Remove(rtn);
        }
        return rtn;
    }

    /// <summary>
    /// 删除某个数据
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return;
        }
        m_DLink.RemoveNode(node);
        m_FindMap.Remove(t);
    }

    /// <summary>
    ///  获取尾部数据
    /// </summary>
    /// <returns></returns>
    public T Back()
    {
        return m_DLink.Tail == null ? null : m_DLink.Tail.t;
    }

    /// <summary>
    /// 获取数量
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
        return m_FindMap.Count;
    }

    /// <summary>
    /// 查找节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Find(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 刷新某个节点，把节点移动到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool ReFlash(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return false;
        }
        m_DLink.MoveToHead(node);
        return true;
    }

    /// <summary>
    /// 清空整个链表
    /// </summary>
    public void Clear()
    {
        // 从尾部进行清空
        while (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.t);
        }
    }

    ~CMapList()
    {
        Clear();
    }
}

public enum LoadResPriority
{
    // 最高优先级
    RES_HIGH = 0,
    // 一般优先级
    RES_MIDDLE,
    // 低优先级
    RES_LOW,
    RES_NUM
}

/// <summary>
/// 异步加载的参数类
/// </summary>
public class AsyncLoadResParam
{
    // 已经给list赋值了
    public List<AsyncCallBack> m_CallBackList = new List<AsyncCallBack>();
    public uint m_Crc;
    public string m_Path;
    public bool m_IsSprite = false;
    public LoadResPriority m_Priority = LoadResPriority.RES_LOW;

    public void Reset()
    {
        m_CallBackList.Clear();
        m_Crc = 0;
        m_Path = "";
        m_IsSprite = false;
        m_Priority = LoadResPriority.RES_LOW;
    }
}

/// <summary>
/// 加载完成的委托
/// </summary>
/// <param name="path"></param>
/// <param name="obj"></param>
/// <param name="param1"></param>
/// <param name="param2"></param>
/// <param name="param3"></param>
public delegate void OnAsyncObjFinish(string path, Object obj, object param1 = null, object param2 = null, object param3 = null);

/// <summary>
/// 回调类，存储所有的委托回调和参数
/// </summary>
public class AsyncCallBack
{
    // 加载完成的回调
    public OnAsyncObjFinish m_DealFinish = null;
    // 回调参数
    public object m_Param1 = null, m_Param2 = null, m_Param3 = null;

    public void Reset()
    {
        m_DealFinish = null;
        m_Param1 = null;
        m_Param2 = null;
        m_Param3 = null;
    }
}