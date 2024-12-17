using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";

    // key是ab包名，value是路径，所有文件夹ab包dic
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    // 用于过滤的list
    private static List<string> m_AllFileAB = new List<string>();

    [MenuItem("Tools/打AB包")]
    public static void Build()
    {
        m_AllFileDir.Clear();
        m_AllFileAB.Clear();


        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        // 先处理文件夹
        foreach(ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包配置名字重复，请检查！");
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
            }
        }

        // 再处理单个文件
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());
        int length = allStr.Length;
        for (int i = 0; i < length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            Debug.LogError(path);
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / length);
        }
        EditorUtility.ClearProgressBar();
    }

    private static bool ContainAllFileAB(string path)
    {
        return false;
    }
}
