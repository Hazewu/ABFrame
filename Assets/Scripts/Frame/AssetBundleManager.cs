using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    // 资源关系依赖配表，可以根据crc来找到对应的资源块
    private Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();

    // 存储已加载的AB包，key=crc
    private Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
    // AssetBundleItem类对象池
    private ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(200);

    /// <summary>
    /// 读取AB包配置
    /// </summary>
    /// <returns></returns>
    public bool LoadAssetBundleConfig()
    {
        m_ResourceItemDic.Clear();

        string configPath = Application.streamingAssetsPath + "/assetbundleconfig";
        AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        if (textAsset == null)
        {
            Debug.LogError("AssetBundleConfig is no exist!");
            return false;
        }

        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig abCfg = (AssetBundleConfig)bf.Deserialize(stream);
        stream.Close();

        for (int i = 0; i < abCfg.ABList.Count; i++)
        {
            ABBase abBase = abCfg.ABList[i];
            ResourceItem item = new ResourceItem();
            item.m_Crc = abBase.Crc;
            item.m_AssetName = abBase.AssetName;
            item.m_ABName = abBase.ABName;
            item.m_ABDepends = abBase.ABDepends;
            if (m_ResourceItemDic.ContainsKey(item.m_Crc))
            {
                Debug.LogError("重复的Crc 资源名：" + item.m_AssetName + " ab包名：" + item.m_ABName);
            }
            else
            {
                m_ResourceItemDic.Add(item.m_Crc, item);
                Debug.Log("资源名：" + item.m_AssetName + " ab包名：" + item.m_ABName + "  crc:" + item.m_Crc);
            }
        }



        return true;
    }

    /// <summary>
    /// 加载AB包，这里引用中间类，避免AB包被重复加载
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        // 注意，在AssetBundleConfig中，abBase中的crc是由路径转换来的，不是由包名
        // 这里不关心路径，crc是由包名转换来的
        uint crc = CRC32.GetCRC32(name);

        if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
        {
            AssetBundle assetBundle = null;
            string fullPath = Application.streamingAssetsPath + "/" + name;
            if (File.Exists(fullPath))
            {
                assetBundle = AssetBundle.LoadFromFile(fullPath);
            }

            if (assetBundle == null)
            {
                Debug.LogError("Load AssetBundle Error:" + fullPath);
            }

            item = m_AssetBundleItemPool.Spawn(true);
            item.assetBundle = assetBundle;
            item.RefCount = 1;
            m_AssetBundleItemDic.Add(crc, item);
        }
        else
        {
            item.RefCount++;
        }

        return item.assetBundle;
    }

    /// <summary>
    /// 卸载AB包，如果引用计数为0，才真正卸载
    /// </summary>
    /// <param name="name"></param>
    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);
        if (m_AssetBundleItemDic.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.assetBundle != null)
            {
                item.assetBundle.Unload(true);
                item.Reset();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
            }
        }
    }

    /// <summary>
    /// 对ResourceManager提供，加载资源
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem LoadResourceAssetBundle(uint crc)
    {
        ResourceItem item = null;
        if (!m_ResourceItemDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError(string.Format("LoadResourceAssetBundle error: can not find crc {0} in AssetBundleConfig", crc.ToString()));
            return item;
        }

        if (item.m_AssetBundle != null)
        {
            return item;
        }

        // 未加载AB包，需要加载
        item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
        // 加载依赖
        if (item.m_ABDepends != null)
        {
            for (int i = 0; i < item.m_ABDepends.Count; i++)
            {
                LoadAssetBundle(item.m_ABDepends[i]);
            }
        }
        return item;
    }

    /// <summary>
    /// 对ResourceManager提供，释放资源
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourceItem item)
    {
        if (item == null) return;
        // 释放依赖
        if (item.m_ABDepends != null && item.m_ABDepends.Count > 0)
        {
            for (int i = 0; i < item.m_ABDepends.Count; i++)
            {
                UnLoadAssetBundle(item.m_ABDepends[i]);
            }
        }
        // 加载时用的ABName，释放时也应该用ABName
        UnLoadAssetBundle(item.m_ABName);
        // 应该把ab包的引用清掉吧？？TODO
        item.m_AssetBundle = null;
    }

    /// <summary>
    /// 根据crc找到ResourceItem
    /// </summary>
    /// <param name="crc">由资源路径转换的crc</param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint crc)
    {
        return m_ResourceItemDic[crc];
    }
}


public class ResourceItem
{
    // 资源路径的CRC
    public uint m_Crc = 0;
    // 该资源的文件名
    public string m_AssetName = string.Empty;
    // 该资源所在的AB包名
    public string m_ABName = string.Empty;
    // 该资源所依赖的AB包
    public List<string> m_ABDepends = null;
    // 该资源加载完的AB包
    public AssetBundle m_AssetBundle = null;

    //--------------------------------------
    // 资源对象
    public Object m_Obj = null;
    // 资源唯一标识
    public int m_Guid = 0;
    // 资源最后使用的时间
    public float m_LastUseTime = 0.0f;
    // 引用计数
    private int m_RefCount = 0;

    public int RefCount
    {
        get { return m_RefCount; }
        set
        {
            m_RefCount = value;

            if (m_RefCount < 0)
            {
                Debug.LogError("RefCount < 0" + m_RefCount + " ," + (m_Obj != null ? m_Obj.name : " name is null"));
            }
        }
    }
}

/// <summary>
/// 中间量，如果一个AB包被重复加载，是会报错的，当没有引用了要卸载
/// </summary>
public class AssetBundleItem
{
    public AssetBundle assetBundle = null;
    public int RefCount;

    public void Reset()
    {
        assetBundle = null;
        RefCount = 0;
    }
}