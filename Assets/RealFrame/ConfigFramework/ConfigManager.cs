using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : Singleton<ConfigManager>
{
    // �洢�����Ѿ����ص����ñ�
    private Dictionary<string, ExcelBase> m_AllExcelData = new Dictionary<string, ExcelBase>();

    /// <summary>
    /// �������ݱ�
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
            Debug.LogError("�ظ�������ͬ�����ļ�:" + path);
            return m_AllExcelData[path] as T;
        }

        T data = BinarySerializeOpt.BinaryDeserialize<T>(path);

#if UNITY_EDITOR
        if (data == null)
        {
            Debug.LogError(path + "�����ڣ���xml����������!");
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
    /// ����·����������
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

        return null;
    }
}

/// <summary>
/// ���ñ�·��
/// </summary>
public class CFG
{

}
