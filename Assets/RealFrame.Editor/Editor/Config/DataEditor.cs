using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DataEditor
{
    [MenuItem("Assets/��תxml")]
    public static void AssetsClassToXml()
    {
        Object[] objs = Selection.objects;
        int length = objs.Length;
        for (int i = 0; i < length; i++)
        {
            EditorUtility.DisplayProgressBar("�ļ����µ���ת��xml", "����ɨ��" + objs[i].name + "......", 1.0f / length * i);
            SaveClass(objs[i].name);
        }
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// �ṩ������תxml����
    /// </summary>
    /// <param name="name"></param>
    private static void SaveClass(string name)
    {
        // ��Ҫ��ȡ��ǰ����������г��򼯣�����name�ҵ���Ӧ�������ʵ����
    }
}
