using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor
{
    #region 普通prefab离线数据
    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreateOfflineData()
    {
        GameObject[] objs = Selection.gameObjects;
        int count = objs.Length;
        for (int i = 0; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改：" + objs[i] + "......", 1.0f / count * i);
            CreateOfflineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 创建离线数据
    /// </summary>
    /// <param name="obj"></param>
    private static void CreateOfflineData(GameObject obj)
    {
        OfflineData data = obj.GetComponent<OfflineData>();
        if (data == null)
        {
            data = obj.AddComponent<OfflineData>();
        }
        data.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了：" + obj.name + " prefab!");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
    #endregion

    #region UI prefab离线数据
    [MenuItem("Assets/生成UI离线数据")]
    public static void AssetCreateUIData()
    {
        GameObject[] objs = Selection.gameObjects;
        int count = objs.Length;
        for (int i = 0; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在修改：" + objs[i] + "......", 1.0f / count * i);
            CreateUIOfflineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/离线数据/生成所有UI prefab离线数据")]
    public static void CreateAllUIData()
    {
        string path = "Assets/GameData/Prefabs/UGUI";
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
        for (int i = 0; i < allStr.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("添加UI离线数据", "正在扫描路径:" + prefabPath + "......", 1.0f / allStr.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (obj == null)
                continue;
            CreateUIOfflineData(obj);
        }
        Debug.Log("UI离线数据全部生成完毕!");
        EditorUtility.ClearProgressBar();
    }

    private static void CreateUIOfflineData(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("UI");

        UIOfflineData uiData = obj.GetComponent<UIOfflineData>();
        if (uiData == null)
        {
            uiData = obj.AddComponent<UIOfflineData>();
        }
        uiData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.name + " UI prefab!");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
    #endregion

    #region 特效 prefab离线数据
    [MenuItem("Assets/生成特效离线数据")]
    public static void AssetCreateEffectData()
    {
        GameObject[] objs = Selection.gameObjects;
        int count = objs.Length;
        for (int i = 0; i < count; i++)
        {
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在修改：" + objs[i] + "......", 1.0f / count * i);
            CreateEffectOfflineData(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/离线数据/生成所有特效 prefab离线数据")]
    public static void CreateAllEffectData()
    {
        string path = "Assets/GameData/Prefabs/Effect";
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", new string[] { path });
        for (int i = 0; i < allStr.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
            EditorUtility.DisplayProgressBar("添加特效离线数据", "正在扫描路径:" + prefabPath + "......", 1.0f / allStr.Length * i);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (obj == null)
                continue;
            CreateEffectOfflineData(obj);
        }
        Debug.Log("特效离线数据全部生成完毕!");
        EditorUtility.ClearProgressBar();
    }

    private static void CreateEffectOfflineData(GameObject obj)
    {
        EffectOfflineData effectData = obj.GetComponent<EffectOfflineData>();
        if (effectData == null)
        {
            effectData = obj.AddComponent<EffectOfflineData>();
        }
        effectData.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.name + " 特效 prefab!");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
    #endregion
}
