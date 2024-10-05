using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ABConfig", menuName = "CreateABConfig", order = 0)]
public class ABConfig : ScriptableObject
{
    // 单个文件所在文件夹路径，会遍历这个文件夹下面所有prefab，每个prefab是一个AB包，所有的prefab名字不能重复，必须保证名字的唯一性
    public List<string> m_AllPrefabPath = new List<string>();
    // 一整个文件夹作为一个AB包
    public List<FileDirABName> m_AllFileDirAB = new List<FileDirABName>();


    [System.Serializable]
    public struct FileDirABName
    {
        public string ABName;
        public string Path;
    }
}
