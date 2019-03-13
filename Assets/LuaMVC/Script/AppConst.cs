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
#if UNITY_ANDROID
                return "https://tmp.com/Android/";
#endif
#if UNITY_IOS
                return "https://tmp.com/IOS/";
#endif
#if UNITY_STANDALONE_WIN
            return "https://tmp.com/Windows/";
#endif          
        }
    }
    public const bool UpdateMode = false;  //设置为true 则从服务器下载
    public const string ExtName = ".unity3d";
    public const bool isLoadFromResources = !UpdateMode; //判断从何处加载lua脚本和资源  
                                                   
}
