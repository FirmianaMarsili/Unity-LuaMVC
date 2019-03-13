using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class BuildAssetBundles : EditorWindow
{
    #region Tmp_by_lc
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<AssetBundleBuild> maps = new List<AssetBundleBuild>();
    public static string sourcePath = Application.dataPath + "/LuaMVC/Bundle/";
    private static Dictionary<string, int> map;
    private static string AssetBundlesOutputPath = "Assets/BuildBundle/";    

    [MenuItem("Tools/AssetBundle/BuildTmp")]
    public static void BuildAssetBundle()
    {
        //AssetBundlesOutputPath = outputPaht;
        Caching.ClearCache();
        ClearAssetBundlesName();
        Pack(sourcePath,true,"Tmp");
        string outputPath = Path.Combine(AssetBundlesOutputPath, Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget));
        //outputPath = AssetBundlesOutputPath + "/" + Platform.GetPlatformFolder(EditorUserBuildSettings.activeBuildTarget) + "/";
        outputPath = AssetBundlesOutputPath;
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        Debug.Log(outputPath);
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        BuildFileIndex(outputPath, "Tmp");
        AssetDatabase.Refresh();

        Debug.Log("打包完成");

    }
    static void ClearAssetBundlesName()
    {
        string[] oldAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldAssetBundleNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[i], true);
        }
    }
    //Remove  外部调用只需要传入true  str 传入你需要打包的最外层的文件的名字
    static void Pack(string source, bool Remove, string str = null)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        if (folder.Name == ".idea")   //这是为了避免idea的根目录
        {
            return;
        }
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        if (Remove)
        {
            for (int i = 0; i < length; i++)
            {
                if (files[i] is DirectoryInfo)
                {
                    if (files[i].FullName.Contains(str))
                    {
                        Pack(files[i].FullName, false);
                    }
                }
                else
                {
                    if (!files[i].Name.EndsWith(".meta") && !files[i].Name.EndsWith(".DS_Store"))
                    {
                        file(files[i].FullName, files[i].Name);
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                if (files[i] is DirectoryInfo)
                {
                    Pack(files[i].FullName, false);
                }
                else
                {
                    if (!files[i].Name.EndsWith(".meta") && !files[i].Name.EndsWith(".DS_Store"))
                    {
                        file(files[i].FullName, files[i].Name);
                    }
                }
            }
        }

    }

    static void file(string source, string fileName)
    {
        string _source = Replace(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);
        string _assetPath2 = _source.Substring(Application.dataPath.Length + 1);

        AssetImporter assetImporter = AssetImporter.GetAtPath(_assetPath);
        string assetName = _assetPath2.Substring(_assetPath2.IndexOf("/") + 1 + "Bundle/".Length);  //如果想修改打包路径可以修改在这
        assetName = assetName.Replace("/" + fileName, null);
        assetName = assetName + ".unity3d";

        assetImporter.assetBundleName = assetName;
    }

    static string Replace(string s)
    {
        return s.Replace("\\", "/");
    }
    //gameCode 传入下载地址里的游戏名字
    static void BuildFileIndex(string outputPath, string gameCode)
    {
        string resPath = outputPath;

        ///----------------------创建文件列表-----------------------
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear();
        files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);

            string tmpUrl =
#if UNITY_ANDROID
        "http://tmp.com/hotab/Android/" + gameCode + "/";
#endif
#if UNITY_IOS
        "http://tmp.com/hotab/IOS/" + gameCode + "/";
#endif
#if UNITY_STANDALONE_WIN
               "http://tmp.com/hotab/Windows/" + gameCode + "/";
#endif
            sw.WriteLine(tmpUrl);
            EditorUtility.DisplayDialog("警告", "导出的是正式版ab包", "确认", "返回");
            Debug.LogError("正式ab包");
        //}
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.EndsWith(".db") || file.EndsWith(".manifest")|| file.EndsWith(".txt") || file.EndsWith(".DS_Store") ) continue;

            string md5 = LuaMVC.Utils_LuaMVC.md5file(file);
            string tmpFile = file.Remove(0, 7);

            string[] tmpArr = tmpFile.Split('/');
            tmpArr[tmpArr.Length - 1] = md5;
            string tmpName = Application.dataPath;
            for (int j = 0; j < tmpArr.Length; j++)
            {
                if (tmpName != null)
                {
                    tmpName += "/";
                    tmpName += tmpArr[j];
                }
                else
                {
                    tmpName = tmpArr[j];
                }
            }


            resPath = Replace(resPath);
            string value = file.Replace(resPath, string.Empty);
            if (value == ".DS_Store")  //针对mac的布局文件
            {
                continue;
            }
            FileInfo info = new FileInfo(file);
            sw.WriteLine(value + "|" + md5 + "|" + info.Length);
            tmpFile = Application.dataPath + "/" + tmpFile;
            File.Move(tmpFile, tmpName);

        }
        sw.Close();
        fs.Close();
    }
    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }
    static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }
    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }
}
public class Platform
{
    public static string GetPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:
                return "Android";
            case BuildTarget.iOS:
                return "IOS";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
#if UNITY_2017_3 || NEWER
            case BuildTarget.StandaloneOSX:
                return "OSX";
#endif
            default:
                return null;
        }
    }
    #endregion

    #region 便捷打开场景

    private static string LastScenePrefKey = "gameBox_lc";
    [MenuItem("打开场景/打开上一个场景")]
    public static void OpenLastScene()
    {
        var lastScene = EditorPrefs.GetString(LastScenePrefKey);
        Debug.Log("Open Last Game Scene!");
        if (!string.IsNullOrEmpty(lastScene))
        {
            EditorSceneManager.OpenScene(lastScene);
        }
        else
        {
            Debug.LogWarning("Not found last scene!");
        }
    }

    public static void OpenScene(string path)
    {
        var currentScene = EditorSceneManager.GetActiveScene().path;
        var mainScene = path;
        if (mainScene != currentScene)
            EditorPrefs.SetString(LastScenePrefKey, currentScene);
        Debug.Log("Open Scene!");
        EditorSceneManager.OpenScene(mainScene);
    }

    private static string GetSceneName(string sceneName)
    {
        return "Assets/Screen/" + sceneName + ".unity";
    }


    [MenuItem("打开场景/Start")]
    public static void OpenStart()
    {
        OpenScene(GetSceneName("Tmp"));
    }
    #endregion
}