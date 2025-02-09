using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectManager : Singleton<ObjectManager>
{
    private Dictionary<Type, object> m_ClassPoolDic = new Dictionary<Type, object>();

    private Transform m_RecyclePoolTrs;
    private Transform m_SceneTrs;
    // 对象池
    private Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();
    // ResourceObj的类对象池
    private ClassObjectPool<ResourceObj> m_ResourceObjPool = null;
    // 暂存ResObj的Dic，把实例化的资源存起来，key=guid
    private Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();
    // 根据异步的guid存储ResourceObj，来判断是否正在异步加载
    private Dictionary<long, ResourceObj> m_AsyncResObjDic = new Dictionary<long, ResourceObj>();

    public void Init(Transform recycleTrs, Transform sceneTrs)
    {
        m_ResourceObjPool = GetOrCreateClassPool<ResourceObj>(500);
        m_RecyclePoolTrs = recycleTrs;
        m_SceneTrs = sceneTrs;
    }

    /// <summary>
    /// 创建类对象池，创建玩出以后外面可以保存ClassObjectPool<T>，然后调用Spawn和Recycle来创建和回收类对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maxCount"></param>
    /// <returns></returns>
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxCount) where T : class, new()
    {
        Type type = typeof(T);
        object outObj = null;
        if (!m_ClassPoolDic.TryGetValue(type, out outObj) || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
            m_ClassPoolDic.Add(type, newPool);
            return newPool;
        }

        return outObj as ClassObjectPool<T>;
    }

    /// <summary>
    /// 从对象池取object
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    private ResourceObj GetObjectFromPool(uint crc)
    {
        List<ResourceObj> objList = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out objList) && objList != null && objList.Count > 0)
        {
            ResourceManager.Instance.IncreaseResourceObjRef(crc);
            ResourceObj resObj = objList[0];
            objList.RemoveAt(0);
            GameObject obj = resObj.m_CloneObj;
            if (!System.Object.ReferenceEquals(obj, null))
            {
                // 还原离线数据
                if (!System.Object.ReferenceEquals(resObj.m_OfflineData, null))
                {
                    resObj.m_OfflineData.ResetProp();
                }

                resObj.m_Already = false;
#if UNITY_EDITOR
                // 只在编辑器模式下改名字，改名字操作很耗gc
                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }
            return resObj;
        }
        return null;
    }

    public GameObject InstantiateObject(string path, bool setSceneObj = false, bool bClear = true)
    {
        uint crc = CRC32.GetCRC32(path);
        ResourceObj resObj = GetObjectFromPool(crc);
        // 对象池中没有，创建
        if (resObj == null)
        {
            resObj = m_ResourceObjPool.Spawn(true);
            resObj.m_Crc = crc;
            resObj.m_Clear = bClear;
            // ResourceManager提供加载的方法
            resObj = ResourceManager.Instance.LoadResourceObj(path, resObj);

            if (resObj.m_ResItem.m_Obj != null)
            {
                resObj.m_CloneObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
                resObj.m_OfflineData = resObj.m_CloneObj.GetComponent<OfflineData>();
            }
        }

        // 设置父物体
        if (setSceneObj)
        {
            resObj.m_CloneObj.transform.SetParent(m_SceneTrs, false);
        }
        else
        {
            resObj.m_CloneObj.SetActive(true);
        }

        int tempID = resObj.m_CloneObj.GetInstanceID();
        if (!m_ResourceObjDic.ContainsKey(tempID))
        {
            m_ResourceObjDic.Add(tempID, resObj);
            // TODO，resObj.m_Guid在哪里赋值？？
        }
        return resObj.m_CloneObj;
    }

    /// <summary>
    /// 回收实例化的对象
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="maxCacheCount">-1表示无限制缓存个数</param>
    /// <param name="destroyCache"></param>
    /// <param name="recycleParent"></param>
    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destroyCache = false, bool recycleParent = true)
    {
        if (obj == null) return;

        ResourceObj resObj = null;
        int tempID = obj.GetInstanceID();

        if (!m_ResourceObjDic.TryGetValue(tempID, out resObj))
        {
            Debug.Log(obj.name + " 对象不是ObjectManager创建的!");
            return;
        }

        if (resObj == null)
        {
            Debug.LogError("缓存的ResourceObj为空!");
            return;
        }

        if (resObj.m_Already)
        {
            Debug.LogError("该对象已经放回对象池了，检查自己是否未清除引用!");
            return;
        }

#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif


        // 不缓存，直接销毁
        if (maxCacheCount == 0)
        {
            m_ResourceObjDic.Remove(tempID);
            ResourceManager.Instance.ReleaseResourceObj(resObj, destroyCache);
            // 在这里销毁对象，因为也是在ObjectManage中实例化的
            GameObject.Destroy(obj);
            resObj.Reset();
            m_ResourceObjPool.Recycle(resObj);
        }
        // 回收到对象池
        else
        {
            List<ResourceObj> st = null;
            if (!m_ObjectPoolDic.TryGetValue(resObj.m_Crc, out st) || st == null)
            {
                st = new List<ResourceObj>();
                m_ObjectPoolDic.Add(resObj.m_Crc, st);
            }

            if (resObj.m_CloneObj)
            {
                if (recycleParent)
                {
                    resObj.m_CloneObj.transform.SetParent(m_RecyclePoolTrs);
                }
                else
                {
                    resObj.m_CloneObj.SetActive(false);
                }
            }

            // 可以缓存
            if (maxCacheCount < 0 || st.Count < maxCacheCount)
            {
                st.Add(resObj);
                resObj.m_Already = true;
                // ResourceManager做一个引用计数
                ResourceManager.Instance.DecreaseResourceObjRef(resObj);
            }
            else
            {
                m_ResourceObjDic.Remove(tempID);
                ResourceManager.Instance.ReleaseResourceObj(resObj, destroyCache);
                // 在这里销毁对象，因为也是在ObjectManage中实例化的
                GameObject.Destroy(obj);
                resObj.Reset();
                m_ResourceObjPool.Recycle(resObj);
            }
        }
    }

    /// <summary>
    /// 异步实例化对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="setSceneObject"></param>
    /// <param name="bClear"></param>
    /// <param name="dealFinish"></param>
    /// <param name="priority"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    public long InstantiateObjectAsync(string path, bool setSceneObject, bool bClear,
        OnAsyncObjFinish dealFinish, LoadResPriority priority, object param1 = null, object param2 = null, object param3 = null)
    {
        if (string.IsNullOrEmpty(path)) return 0;

        uint crc = CRC32.GetCRC32(path);

        ResourceObj resObj = GetObjectFromPool(crc);
        if (resObj != null)
        {
            if (setSceneObject)
            {
                resObj.m_CloneObj.transform.SetParent(m_SceneTrs, false);
            }

            if (dealFinish != null)
            {
                dealFinish(path, resObj.m_CloneObj, param1, param2, param3);
            }

            return resObj.m_Guid;
        }

        long guid = ResourceManager.Instance.CreateGuid();
        resObj = m_ResourceObjPool.Spawn(true);
        resObj.m_Crc = crc;
        resObj.m_SetSceneParent = setSceneObject;
        resObj.m_Clear = bClear;
        resObj.m_DealFinish = dealFinish;
        resObj.m_param1 = param1;
        resObj.m_param2 = param2;
        resObj.m_param3 = param3;
        resObj.m_Guid = guid;
        // 调用ResourceManager的异步加载接口
        ResourceManager.Instance.AsyncLoadResourceObj(path, resObj, OnLoadResourceObjFinish, priority);
        return guid;
    }

    /// <summary>
    /// 加载完成回调执行，比资源的异步加载多套了一层回调
    /// </summary>
    /// <param name="path"></param>
    /// <param name="resObj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    private void OnLoadResourceObjFinish(string path, ResourceObj resObj)
    {
        if (resObj == null) return;

        if (resObj.m_ResItem.m_Obj == null)
        {
#if UNITY_EDITOR
            Debug.LogError("异步资源加载的资源为空:" + path);
#endif
        }
        else
        {
            resObj.m_CloneObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
            resObj.m_OfflineData = resObj.m_CloneObj.GetComponent<OfflineData>();
        }

        // 加载完成就从正在加载的异步中移除
        if (m_AsyncResObjDic.ContainsKey(resObj.m_Guid))
        {
            m_AsyncResObjDic.Remove(resObj.m_Guid);
        }

        if (resObj.m_CloneObj != null && resObj.m_SetSceneParent)
        {
            resObj.m_CloneObj.transform.SetParent(m_SceneTrs, false);
        }

        if (resObj.m_DealFinish != null)
        {
            int tempGuid = resObj.m_CloneObj.GetInstanceID();
            if (!m_ResourceObjDic.ContainsKey(tempGuid))
            {
                m_ResourceObjDic.Add(tempGuid, resObj);
            }

            resObj.m_DealFinish(path, resObj.m_CloneObj, resObj.m_param1, resObj.m_param2, resObj.m_param3);
        }
    }

    /// <summary>
    /// 预加载GameObject
    /// </summary>
    /// <param name="path"></param>
    /// <param name="count"></param>
    /// <param name="clear"></param>
    public void PreloadGameObject(string path, int count = 1, bool clear = false)
    {
        List<GameObject> tempList = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject go = InstantiateObject(path, false, clear);
            tempList.Add(go);
        }

        // 需要加载完后再释放，否则可能加载的是同一个对象（因为缓存了）
        for (int i = 0; i < count; i++)
        {
            GameObject obj = tempList[i];
            ReleaseObject(obj);
            obj = null;
        }

        tempList.Clear();
    }

    /// <summary>
    /// 取消异步加载
    /// </summary>
    /// <param name="guid"></param>
    public void CancelLoad(long guid)
    {
        ResourceObj resObj = null;
        if (m_AsyncResObjDic.TryGetValue(guid, out resObj) && ResourceManager.Instance.CancelLoad(resObj))
        {
            m_AsyncResObjDic.Remove(guid);
            resObj.Reset();
            m_ResourceObjPool.Recycle(resObj);
            // TODO 还有其他地方又resObj的引用吧，没去除干净
        }
    }

    /// <summary>
    /// 是否正在异步加载
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool IsAsyncLoading(long guid)
    {
        return m_AsyncResObjDic[guid] != null;
    }

    /// <summary>
    /// 该对象是否是对象池创建的
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool IsObjectManagerCreate(GameObject obj)
    {
        ResourceObj resObj = m_ResourceObjDic[obj.GetInstanceID()];
        return resObj == null ? false : true;
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearCache()
    {
        List<uint> tempList = new List<uint>();
        // TODO，忘记m_ObjectPoolDic这个是存什么了的，需要再看看
        foreach (uint key in m_ObjectPoolDic.Keys)
        {
            List<ResourceObj> st = m_ObjectPoolDic[key];
            for (int i = st.Count - 1; i >= 0; i--)
            {
                ResourceObj resObj = st[i];
                if (!System.Object.ReferenceEquals(resObj.m_CloneObj, null) && resObj.m_Clear)
                {
                    m_ResourceObjDic.Remove(resObj.m_CloneObj.GetInstanceID());
                    GameObject.Destroy(resObj.m_CloneObj);
                    resObj.Reset();
                    m_ResourceObjPool.Recycle(resObj);
                    st.Remove(resObj);
                }
            }
            if (st.Count <= 0)
            {
                tempList.Add(key);
            }
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            uint temp = tempList[i];
            if (m_ObjectPoolDic.ContainsKey(temp))
            {
                m_ObjectPoolDic.Remove(temp);
            }
        }
        tempList.Clear();
    }


    /// <summary>
    /// 清除某个资源在对象池中所有的对象
    /// </summary>
    /// <param name="crc"></param>
    public void ClearPoolObject(uint crc)
    {
        List<ResourceObj> st = null;
        if (!m_ObjectPoolDic.TryGetValue(crc, out st) || st == null)
            return;

        for (int i = st.Count - 1; i >= 0; i--)
        {
            ResourceObj resObj = st[i];
            if (resObj.m_Clear)
            {
                st.Remove(resObj);
                int tempID = resObj.m_CloneObj.GetInstanceID();
                GameObject.Destroy(resObj.m_CloneObj);
                resObj.Reset();
                m_ResourceObjDic.Remove(tempID);
                m_ResourceObjPool.Recycle(resObj);
            }
        }

        if (st.Count <= 0)
        {
            m_ObjectPoolDic.Remove(crc);
        }
    }

    /// <summary>
    /// 根据实例化对象直接获得离线数据，而不是用getCompnent，getComponent也是一个比较耗时的操作
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public OfflineData FindOfflineData(GameObject obj)
    {
        OfflineData data = null;
        ResourceObj resObj = null;
        m_ResourceObjDic.TryGetValue(obj.GetInstanceID(), out resObj);
        if (resObj != null)
        {
            data = resObj.m_OfflineData;
        }
        return data;
    }
}
