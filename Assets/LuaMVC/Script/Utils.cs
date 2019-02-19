using System;
/********************************************************************************
desc:一些需要用到工具

Update: 2018.8.2
*********************************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
namespace LuaMVC
{
    /// <summary>
    /// Utils：
    /// </summary>
    public class Utils_LuaMVC : MonoBehaviour
    {
        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string md5(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++)
            {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string md5file(string file)
        {
            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(fs);
                fs.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("md5file() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public static void ClearChild(Transform go)
        {
            if (go == null) return;
            for (int i = go.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(go.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 取得数据存放目录
        /// </summary>
        public static string DataPath
        {
            get
            {
                string game = AppConst.AppName.ToLower();
                if (Application.isMobilePlatform)
                {
                    return Application.persistentDataPath + "/" + game + "/";
                }
                if (/*AppConst.DebugMode*/Application.isEditor)
                {
                    return /*Application.dataPath + "/" + AppConst.AssetDir + "/";*/Application.persistentDataPath + "/" + "tmpTest" + "/";
                }
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    int i = Application.dataPath.LastIndexOf('/');
                    return Application.dataPath.Substring(0, i + 1) + game + "/";
                }
                return Application.persistentDataPath + "/" + game + "/";
            }
        }

        /// <summary>
        /// 获取资源加载路径
        /// </summary>
        /// <returns></returns>
        public static string GetRelativePath()
        {
            //if (Application.isEditor)
            //    return "file://" + Application.streamingAssetsPath + "/";
            //else if (Application.isMobilePlatform || Application.isConsolePlatform)
            //    return "file:///" + DataPath;
            //else // For standalone player.
            //    return "file://" + Application.streamingAssetsPath + "/";
            return "file://" + DataPath;
        }

        /// <summary>
        /// 取得行文本
        /// </summary>
        public static string GetFileText(string path)
        {
            return File.ReadAllText(path);
        }

        /// <summary>
        /// 网络可用
        /// </summary>
        public static bool NetAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public static bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        /// <summary>
        /// 应用程序内容路径
        /// </summary>
        public static string AppContentPath()
        {
            string filePath =
#if UNITY_ANDROID && !UNITY_EDITOR
        "jar:file://" + Application.dataPath + "!/assets/Script/";  
#elif UNITY_IPHONE && !UNITY_EDITOR
        "file://" + Application.dataPath + "/Raw/Script/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
 "file://" + Application.dataPath + "/StreamingAssets" + "/Script/";
#endif
            return filePath;
        }
        /// <summary>
        /// 重置加载的资源的shader，解决加载资源shader丢失的问题
        /// </summary>
        /// <param name="obj"></param>
        public static void ResetShader(UnityEngine.Object obj)
        {            
            List<Material> listMat = new List<Material>();
            if (obj is Material)
            {

                Material m = obj as Material;

                listMat.Add(m);

            }
            else if (obj is GameObject)
            {
                GameObject go = obj as GameObject;
                UnityEngine.UI.Image[] rends = go.GetComponentsInChildren<UnityEngine.UI.Image>();
                if (null != rends)
                {
                    foreach (var item in rends)
                    {
                        Material materialsArr = item.material;                      
                            listMat.Add(materialsArr);
                    }
                }
            }
            for (int i = 0; i < listMat.Count; i++)
            {
                Material m = listMat[i];
                if (null == m)
                    continue;
                var shaderName = m.shader.name;
                var newShader = Shader.Find(shaderName);
                if (newShader != null)
                    m.shader = newShader;
            }
        }
    }
}
