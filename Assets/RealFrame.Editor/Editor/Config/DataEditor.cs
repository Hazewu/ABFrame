using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Unity.VisualScripting;
using System.IO;

public class DataEditor
{
    private static string Xml_Path = "Assets/GameData/ConfigData/Xml/";
    private static string Binary_Path = "Assets/GameData/ConfigData/Binary/";
    private static string Script_Path = "Assets/Scripts/Data";


    [MenuItem("Assets/类转xml")]
    public static void AssetsClassToXml()
    {
        UnityEngine.Object[] objs = Selection.objects;
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("文件夹下的类转成xml", "正在扫描" + objs[i].name + "......", 1.0f / length * i);
            ClassToXml(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 提供单个类转xml方法
    /// </summary>
    /// <param name="name"></param>
    private static void ClassToXml(string name)
    {
        try
        {
            // 需要获取当前主程序的所有程序集，根据name找到对应的类进行实例化
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }

            if (type != null)
            {
                var temp = Activator.CreateInstance(type);
                if (temp is ExcelBase)
                {
                    (temp as ExcelBase).Construction();
                }
                string xmlPath = Xml_Path + name + ".xml";
                BinarySerializeOpt.XmlSerialize(xmlPath, temp);
                Debug.Log(name + "类转xml成功，xml路径为:" + xmlPath);
            }
        }
        catch
        {
            Debug.LogError(name + "类转xml失败!");
        }
    }

    [MenuItem("Assets/Xml转Binary")]
    public static void AssetsXmlToBinary()
    {
        UnityEngine.Object[] objs = Selection.objects;
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("文件下的xml转二进制", "正在扫描" + objs[i].name + "......", 1.0f / length * i);
            XmlToBinary(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/Xml/Xml转二进制")]
    public static void AllXmlToBinary()
    {
        string path = Application.dataPath.Replace("Assets", "") + Xml_Path;
        string[] filesPath = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        int length = filesPath.Length;
        for (int i = 0; i < length; i++)
        {
            string tempPath = filesPath[i];
            EditorUtility.DisplayProgressBar("查找文件夹下面的Xml", "正在扫描" + tempPath + "......", 1.0f / length * i);
            if (tempPath.EndsWith(".xml"))
            {
                string name = tempPath.Substring(tempPath.LastIndexOf("/") + 1);
                name = name.Replace(".xml", "");
                XmlToBinary(name);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// xml转二进制
    /// </summary>
    /// <param name="name"></param>
    private static void XmlToBinary(string name)
    {
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            Type type = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type tempType = asm.GetType(name);
                if (tempType != null)
                {
                    type = tempType;
                    break;
                }
            }
            if (type != null)
            {
                string xmlPath = Xml_Path + name + ".xml";
                string binaryPath = Binary_Path + name + ".bytes";
                object obj = BinarySerializeOpt.XmlDeserialize(xmlPath, type);
                BinarySerializeOpt.BinarySerialize(binaryPath, obj);
                Debug.Log(name + " xml转二进制成功，二进制路径为：" + binaryPath);
            }
        }
        catch
        {
            Debug.LogError(name + " xml转二进制失败!");
        }
    }
}
