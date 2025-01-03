using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";

    // key是ab包名，value是路径，所有文件夹ab包dic
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    // key是ab名，value是所有依赖的路径
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
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
            if (!ContainAllFileAB(path))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDepends = AssetDatabase.GetDependencies(path);
                List<string> allDependPaths = new List<string>();
                for (int j = 0; j < allDepends.Length; j++)
                {
                    string tempPath = allDepends[j];
                    Debug.Log(tempPath);
                    if (!ContainAllFileAB(tempPath) && !tempPath.EndsWith(".cs"))
                    {
                        m_AllFileAB.Add(tempPath);
                        allDependPaths.Add(tempPath);
                    }
                }
                if (m_AllPrefabDir.ContainsKey(obj.name)){
                    Debug.LogError(" 存在相同名字的Prefab！名字" + obj.name);
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPaths);
                }
            }
        }
        EditorUtility.ClearProgressBar();
    }

    private static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            if (path == m_AllFileAB[i] || path.Contains(m_AllFileAB[i]))
                return true;
        }
        return false;
    }
}
