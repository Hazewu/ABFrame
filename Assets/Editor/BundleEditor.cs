using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";
    [MenuItem("Tools/打AB包")]
    public static void Build()
    {
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        foreach( string str in abConfig.m_AllPrefabPath)
        {
            Debug.Log(str);
        }

        foreach(ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            Debug.Log(fileDir.ABName);
            Debug.Log(fileDir.Path);
        }
    }
}
