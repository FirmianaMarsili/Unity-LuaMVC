/********************************************************************************
** desc      ： 配置信息
** Update: 2018.8.2
*********************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AppConst：
/// </summary>
public class AppConst {

    public const string AssetDir = "BuildBundle";           //素材主目录 && 依赖信息文件名 
    //public static bool DebugMode = false; //设置为true则从Asset下获取资源地址  不过他会优先判断是否为移动平台
    public static string AppName = "Chat";
    public static string WebUrl_HotUpdate
    {
        get
        {
            return "";
//            if (WebClient.APIServer != WebClient.APIServer_ForHot)
//            {
//#if UNITY_ANDROID
//                return "https://mcnetgamedownload.oss-cn-shenzhen.aliyuncs.com/testhotab/Android/";
//#endif
//#if UNITY_IOS
//                return "https://mcnetgamedownload.oss-cn-shenzhen.aliyuncs.com/testhotab/IOS/";
//#endif
//#if UNITY_STANDALONE_WIN
//                return "https://mcnetgamedownload.oss-cn-shenzhen.aliyuncs.com/testhotab/Windows/";
//#endif
//            }
//            else
//            {
//#if UNITY_ANDROID
//                return "https://mcnetgamedownload.oss-cn-shenzhen.aliyuncs.com/hotab/Android/";
//#endif
//#if UNITY_IOS                
//                return "https://mcnetgamedownload.oss-cn-shenzhen.aliyuncs.com/hotab/IOS/";
//#endif
//#if UNITY_STANDALONE_WIN
//                return "https://mcnetgamedownload.oss-cn-shenzhen.aliyuncs.com/hotab/Windows/";
//#endif
            //}            
        }
    }
    public const bool UpdateMode = false;  //设置为true 则从服务器下载
    public const string ExtName = ".unity3d";
    public const bool isLoadFromResources = !UpdateMode; //判断从何处加载lua脚本和资源  
                                                   
}
