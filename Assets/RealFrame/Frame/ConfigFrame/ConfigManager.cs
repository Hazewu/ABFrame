using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : Singleton<ConfigManager>
{
    // 存储所有已经加载的配置表
    private Dictionary<string, ExcelBase> m_AllExcelData = new Dictionary<string, ExcelBase>();

    /// <summary>
    /// 加载数据表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadData<T>(string path) where T : ExcelBase
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (m_AllExcelData.ContainsKey(path))
        {
            Debug.LogError("重复加载相同配置文件:" + path);
            return m_AllExcelData[path] as T;
        }

        T data = BinarySerializeOpt.BinaryDeserialize<T>(path);

#if UNITY_EDITOR
        if (data == null)
        {
            Debug.LogError(path + "不存在，从xml加载数据了!");
            string xmlPath = path.Replace("Binary", "xml").Replace(".bytes", ".xml");
            data = BinarySerializeOpt.XmlDeserialize<T>(xmlPath);
        }
#endif
        if (data != null)
        {
            data.Init();
        }

        m_AllExcelData.Add(path, data);
        return data;
    }

    /// <summary>
    /// 根据路径查找数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T FindData<T>(string path) where T : ExcelBase
    {
        if (string.IsNullOrEmpty(path))
            return null;

        ExcelBase excelBase = null;
        if (m_AllExcelData.TryGetValue(path, out excelBase))
        {
            return excelBase as T;
        }
        else
        {
            excelBase = LoadData<T>(path);
        }

        return (T)excelBase;
    }
}

/// <summary>
/// 配置表路径
/// </summary>
public class CFG
{
    public const string TABLE_MONSTER = "Assets/GameData/ConfigData/Binary/MonsterData.bytes";
    public const string TABLE_BUFF = "Assets/GameData/ConfigData/Binary/BuffData.bytes";
    public const string TABLE_POETRY = "Assets/GameData/ConfigData/Binary/Poetry.bytes";
}
