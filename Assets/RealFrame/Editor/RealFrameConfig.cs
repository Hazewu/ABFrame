using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CreateAssetMenu(fileName = "RealFrameConfig", menuName = "CreateRealFrameConfig", order = 0)]
public class RealFrameConfig : ScriptableObject
{
    // 打包时生成AB包配置表的二进制路径
    public string m_ABBytePath;
    // xml文件夹路径
    public string m_XmlPath;
    // 二进制文件夹路径
    public string m_BinaryPath;
    // 脚本文件夹路径
    public string m_ScriptsPath;
}

public class RealConfig
{
    private const string RealFramePath = "Assets/RealFrame/Editor/RealFrameConfig.asset";

    private static RealFrameConfig m_config = null;

    public static RealFrameConfig GetConfig()
    {
        if (m_config == null)
        {
            m_config = AssetDatabase.LoadAssetAtPath<RealFrameConfig>(RealFramePath);
        }
        return m_config;
    }
}

[CustomEditor(typeof(RealFrameConfig))]
public class RealFrameConfigInspector : Editor
{
    public SerializedProperty m_ABBytePath;
    public SerializedProperty m_XmlPath;
    public SerializedProperty m_BinaryPath;
    public SerializedProperty m_ScriptsPath;

    private void OnEnable()
    {
        m_ABBytePath = serializedObject.FindProperty("m_ABBytePath");
        m_XmlPath = serializedObject.FindProperty("m_XmlPath");
        m_BinaryPath = serializedObject.FindProperty("m_BinaryPath");
        m_ScriptsPath = serializedObject.FindProperty("m_ScriptsPath");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_ABBytePath, new GUIContent("ab包二进制路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_XmlPath, new GUIContent("配置表xml路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_BinaryPath, new GUIContent("配置表二进制路径"));
        GUILayout.Space(5);
        EditorGUILayout.PropertyField(m_ScriptsPath, new GUIContent("配置表脚本路径"));
        GUILayout.Space(5);
        serializedObject.ApplyModifiedProperties();
    }
}
