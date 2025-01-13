using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DataEditor
{
    [MenuItem("Assets/类转xml")]
    public static void AssetsClassToXml()
    {
        Object[] objs = Selection.objects;
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("文件夹下的类转成xml", "正在扫描" + objs[i].name + "......", 1.0f / length * i);
            SaveClass(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 提供单个类转xml方法
    /// </summary>
    /// <param name="name"></param>
    private static void SaveClass(string name)
    {
        // 需要获取当前主程序的所有程序集，根据name找到对应的类进行实例化
    }
}
