using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;
using System.Threading;

public class ResourceManager : Singleton<ResourceManager>
{
    // 缓存引用计数为零的资源列表，游戏资源能快速加载，达到缓存最大的时候释放这个列表里面最早没用的资源
    private CMapList<ResourceItem> m_NoReferenceAssetMapList = new CMapList<ResourceItem>();
    // 缓存使用的资源列表
    private Dictionary<uint, ResourceItem> AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();

    // 是否从AB包中加载资源
    private bool m_LoadFromAssetBundle = true;

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

        // 一定执行
        if (!AssetDic.Remove(item.m_Crc))
        {
            return;
        }

        if (!destroyCache)
        {
            // 不销毁，移动到头部，当要清理缓存时，进行清理
            m_NoReferenceAssetMapList.InsertToHead(item);
            return;
        }

        // 释放assetBundle引用
        AssetBundleManager.Instance.ReleaseAsset(item);
        if (item.m_Obj != null)
        {
            item.m_Obj = null;
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
            Debug.LogError("AssetDic里不存在改资源:" + obj.name + " 可能释放了多次");
            return false;
        }

        item.RefCount--;

        DestroyResourceItem(item, destroyObj);
        return true;
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
    public int size()
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