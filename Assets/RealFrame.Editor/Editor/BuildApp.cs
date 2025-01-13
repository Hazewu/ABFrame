using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class BuildApp
{
    private static string m_AppName = "RealFrame";
    private static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
    private static string m_MacOSPath = Application.dataPath + "/../BuildTarget/MacOS/";
    private static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";

    [MenuItem("Build/标准包")]
    public static void Build()
    {
        // 打ab包
        BundleEditor.Build();
        // 生成可执行程序
        string abPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        Copy(abPath, Application.streamingAssetsPath);
        string savePath = "";

        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.Android:
                savePath = m_AndroidPath + m_AppName + "_" + BuildTarget.Android + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now) + ".apk";
                break;
            case BuildTarget.iOS:
                savePath = m_MacOSPath + m_AppName + "_" + BuildTarget.iOS + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                savePath = m_WindowsPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget +
                    string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now, m_AppName);
                break;
            default:
                break;
        }
        BuildPipeline.BuildPlayer(FindEnableEditorScenes(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);

        //DeleteDir(Application.streamingAssetsPath);
    }

    private static string[] FindEnableEditorScenes()
    {
        List<string> editorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            editorScenes.Add(scene.path);
        }
        return editorScenes.ToArray();
    }

    private static void Copy(string srcPath, string targetPath)
    {
        try
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            string srcDir = Path.Combine(targetPath, Path.GetFileName(srcPath));
            if (Directory.Exists(srcPath))
            {
                srcDir += Path.DirectorySeparatorChar;
            }
            if (!Directory.Exists(srcDir))
            {
                Directory.CreateDirectory(srcDir);
            }

            string[] files = Directory.GetFileSystemEntries(srcPath);
            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    // 递归文件夹
                    Copy(file, targetPath);
                }
                else
                {
                    File.Copy(file, srcDir + Path.GetFileName(file), true);
                }
            }
        }
        catch
        {
            Debug.LogError("无法复制：" + srcPath + " 到" + targetPath);
        }
    }

    /// <summary>
    /// 打包结束后，删掉工程中AB包所在的文件
    /// </summary>
    /// <param name="srcPath"></param>
    private static void DeleteDir(string srcPath)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileInfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo info in fileInfo)
            {
                if (info is DirectoryInfo)
                {
                    DirectoryInfo subDir = new DirectoryInfo(info.FullName);
                    subDir.Delete(true);
                }
                else
                {
                    File.Delete(info.FullName);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}
