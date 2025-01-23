using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class BundleEditor
{
    private static string m_BundleTargetPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();
    private static string ABCONFIGPATH = "Assets/RealFrame/Editor/Resource/ABConfig.asset";

    // key是ab包名，value是路径，所有文件夹ab包dic
    private static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();
    // key是ab名，value是所有依赖的路径
    private static Dictionary<string, List<string>> m_AllPrefabDir = new Dictionary<string, List<string>>();
    // 用于过滤的list
    private static List<string> m_AllFileAB = new List<string>();
    // 存储所有有效路径，不需要的资源不用加载
    private static List<string> m_ConfigFile = new List<string>();

    [MenuItem("Tools/打AB包")]
    public static void Build()
    {
        DataEditor.AllXmlToBinary();

        m_AllFileDir.Clear();
        m_AllFileAB.Clear();
        m_AllPrefabDir.Clear();
        m_ConfigFile.Clear();


        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        // 先处理文件夹
        foreach (ABConfig.FileDirABName fileDir in abConfig.m_AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(fileDir.ABName))
            {
                Debug.LogError("AB包配置名字重复，请检查！");
            }
            else
            {
                m_AllFileDir.Add(fileDir.ABName, fileDir.Path);
                m_AllFileAB.Add(fileDir.Path);
                m_ConfigFile.Add(fileDir.Path);
            }
        }

        // 再处理单个文件
        string[] allStr = AssetDatabase.FindAssets("t:Prefab", abConfig.m_AllPrefabPath.ToArray());
        int length = allStr.Length;
        for (int i = 0; i < length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            // Debug.LogError(path);
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab:" + path, i * 1.0f / length);
            m_ConfigFile.Add(path);
            if (!ContainAllFileAB(path))
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string[] allDepends = AssetDatabase.GetDependencies(path);
                List<string> allDependPaths = new List<string>();
                for (int j = 0; j < allDepends.Length; j++)
                {
                    string tempPath = allDepends[j];
                    //Debug.Log(tempPath);
                    if (!ContainAllFileAB(tempPath) && !tempPath.EndsWith(".cs"))
                    {
                        m_AllFileAB.Add(tempPath);
                        allDependPaths.Add(tempPath);
                    }
                }
                if (m_AllPrefabDir.ContainsKey(obj.name))
                {
                    Debug.LogError(" 存在相同名字的Prefab！名字" + obj.name);
                }
                else
                {
                    m_AllPrefabDir.Add(obj.name, allDependPaths);
                }
            }
        }

        // 设置包名
        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }
        foreach (string name in m_AllPrefabDir.Keys)
        {
            SetABName(name, m_AllPrefabDir[name]);
        }

        BuildAssetBundle();


        // 打包结束后，清除包名
        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        int nameLength = oldABNames.Length;
        for (int i = 0; i > nameLength; i++)
        {
            string temp = oldABNames[i];
            AssetDatabase.RemoveAssetBundleName(temp, true);
            EditorUtility.DisplayProgressBar("清除AB包名", "名字：" + temp, i * 1.0f / nameLength);
        }
        //AssetDatabase.SaveAssets();
        //AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 是否是有效路径
    /// </summary>
    /// <param name="path">特别是一个文件夹打成一个AB包，那么该文件夹下的其他资源的路径，都应该返回true</param>
    /// <returns></returns>
    private static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigFile.Count; i++)
        {
            if (path.Contains(m_ConfigFile[i]))
                return true;
        }
        return false;
    }

    private static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileAB.Count; i++)
        {
            string temp = m_AllFileAB[i];
            if (path == temp ||
                // 包含，并且去掉后，第一个值是/，则能充分表名path在temp的文件夹下
                (path.Contains(temp) && path.Replace(temp, "")[0] == '/'))
                return true;
        }
        return false;
    }

    private static void SetABName(string name, string path)
    {
        AssetImporter assetImposter = AssetImporter.GetAtPath(path);
        if (assetImposter == null)
        {
            Debug.LogError("不存在此路径文件：" + path);
        }
        else
        {
            assetImposter.assetBundleName = name;
        }
    }

    private static void SetABName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name, paths[i]);
        }
    }

    private static void BuildAssetBundle()
    {
        string[] allBundles = AssetDatabase.GetAllAssetBundleNames();
        Dictionary<string, string> resPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundles.Length; i++)
        {
            string[] allBundlePath = AssetDatabase.GetAssetPathsFromAssetBundle(allBundles[i]);
            string name = allBundles[i];
            for (int j = 0; j < allBundlePath.Length; j++)
            {
                string tempPath = allBundlePath[j];
                if (tempPath.EndsWith(".cs"))
                    continue;

                Debug.Log("此AB包：" + name + " 下面包含的资源文件路径：" + tempPath);
                resPathDic.Add(tempPath, name);
            }
        }

        // 如果不存在，则创建文件夹
        if (!Directory.Exists(m_BundleTargetPath))
        {
            Directory.CreateDirectory(m_BundleTargetPath);
        }

        DeleteAB();
        // 生成自己的配置表
        WriteData(resPathDic);


        // 生成
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(m_BundleTargetPath, BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);
        if (manifest == null)
        {
            Debug.LogError("AssetBundle 打包失败！");
        }
        else
        {
            Debug.Log("AssetBundle 打包完毕");
        }
    }

    private static bool ContainABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            string abName = strs[i];

            if (name == abName ||
                // 包含，并且去掉后，第一个值是.，则能充分表明name是属于abName的正主、.meta、.manifest、.manifest.meta文件
                (name.Contains(abName) && name.Replace(abName, "")[0] == '.'))
            {
                return true;
            }
        }
        return false;
    }

    private static void DeleteAB()
    {
        string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo direction = new DirectoryInfo(m_BundleTargetPath);
        FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            // TODO，这里不够充分
            if (ContainABName(files[i].Name, allBundleNames))
            {
                continue;
            }
            else
            {
                Debug.Log("此AB包已经被删或者改名了：" + files[i].Name);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                }
                if (File.Exists(files[i].FullName + ".manifest"))
                {
                    File.Delete(files[i].FullName + ".manifest");
                }
            }
        }
    }

    private static void WriteData(Dictionary<string, string> resPathDic)
    {
        AssetBundleConfig config = new AssetBundleConfig();
        config.ABList = new List<ABBase>();
        foreach (string path in resPathDic.Keys)
        {
            if (!ValidPath(path)) continue;

            ABBase abBase = new ABBase();
            abBase.Path = path;
            abBase.Crc = CRC32.GetCRC32(path);
            abBase.ABName = resPathDic[path];
            abBase.AssetName = path.Remove(0, path.LastIndexOf('/') + 1);
            abBase.ABDepends = new List<string>();
            string[] resDepends = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < resDepends.Length; i++)
            {
                string tempPath = resDepends[i];
                if (tempPath == path || path.EndsWith(".cs"))
                    continue;

                string abName = "";
                if (resPathDic.TryGetValue(tempPath, out abName))
                {
                    if (abName == resPathDic[path])
                        continue;

                    if (!abBase.ABDepends.Contains(abName))
                    {
                        abBase.ABDepends.Add(abName);
                    }
                }
            }
            config.ABList.Add(abBase);
        }

        // 写入xml
        string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
        if (File.Exists(xmlPath)) File.Delete(xmlPath);
        FileStream fs = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(config.GetType());
        xs.Serialize(sw, config);
        sw.Close();
        fs.Close();

        // 写入二进制
        foreach (ABBase ab in config.ABList)
        {
            // 二进制中不需要实际的路径，用crc就行了
            ab.Path = "";
        }
        string bytePath = RealConfig.GetConfig().m_ABBytePath;
        if (File.Exists(bytePath)) File.Delete(bytePath);
        FileStream binaryFs = new FileStream(bytePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        binaryFs.Seek(0, SeekOrigin.Begin);
        binaryFs.SetLength(0);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(binaryFs, config);
        binaryFs.Close();
        AssetDatabase.Refresh();

        SetABName("assetbundleconfig", bytePath);
    }
}
