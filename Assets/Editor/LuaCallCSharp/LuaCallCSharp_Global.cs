using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using XLua;

public static class LuaCallCSharp_Global
{

    [XLua.CSharpCallLua]
    public static List<Type> my_CSharp_Call_Lua_List
    {
        get
        {
            return new List<Type>
            {
                //typeof(LuaMVC.Program.OnSendNotification),
                typeof(UnityEngine.Events.UnityAction<bool>),
                typeof(System.Action),
                typeof(Action<UnityEngine.Object[]>),
                typeof(System.Collections.IEnumerator),
                typeof(Action<bool>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(Func<double, double, double>),
                typeof(Action<string>),
                typeof(Action<double>),
                typeof(UnityEngine.Events.UnityAction),
                typeof(System.Object),
                typeof(XLua.LuaTable),
                typeof(object),
                //typeof(Asset.Code.Tmp.NetManager._OpenClient),               
                //typeof(Asset.Code.Tmp.NetManager.OnData),
            };
        }

    }
    [XLua.LuaCallCSharp]
    public static List<Type> my_Lua_Call_CSharp_List
    {
        get
        {
            return new List<Type>
            {
                typeof(System.Collections.IEnumerator),
                typeof(UnityEngine.Vector3),
                typeof(UnityEngine.Vector2),
                typeof(WaitForSeconds),
                typeof(WWW),
                typeof(System.Object),
                typeof(UnityEngine.Object),
                typeof(Vector2),
                typeof(Vector3),
                typeof(Vector4),
                typeof(Quaternion),
                typeof(Color),
                typeof(Ray),
                typeof(Bounds),
                typeof(Ray2D),
                typeof(Time),
                typeof(GameObject),
                typeof(Component),
                typeof(Behaviour),
                typeof(Transform),
                typeof(Resources),
                typeof(TextAsset),
                typeof(Keyframe),
                typeof(AnimationCurve),
                typeof(AnimationClip),
                typeof(MonoBehaviour),
                typeof(ParticleSystem),
                typeof(SkinnedMeshRenderer),
                typeof(Renderer),
                typeof(Light),
                typeof(Mathf),
                typeof(System.Collections.Generic.List<int>),
                typeof(Action<string>),
                typeof(UnityEngine.Debug),
                typeof(System.Object),
                typeof(object),               
                //typeof(System.Byte[]),
                typeof(System.Byte),

                typeof(System.Net.Sockets.Socket),
                typeof(System.Net.Sockets.AddressFamily),
                typeof(System.Net.Sockets.SocketType),
                typeof(System.Net.Sockets.ProtocolType),
                typeof(LuaMVC.Loom),                
                typeof(UnityEngine.Sprite),
                typeof(UnityEngine.Rect),
                typeof(UnityEngine.Time),
                typeof(UnityEngine.UI.LayoutRebuilder),
                typeof(UnityEngine.UI.LayoutUtility),
                typeof(UnityEngine.AudioDataLoadState),
                typeof(System.DateTime),
            };
        }

    }


    /// <summary>
    /// dotween的扩展方法在lua中调用
    /// </summary>
    [XLua.LuaCallCSharp]
    [XLua.ReflectionUse]
    public static List<Type> dotween_lua_call_cs_list
    {
        get
        {
            return new List<Type>
            {                    


            };
        }

    }


    //[Hotfix]
    //public static IEnumerable<Type> HotfixInject
    //{
    //    get
    //    {

    //        var tmp = (from type in Assembly.Load("Assembly-CSharp").GetExportedTypes()
    //                   where type.Namespace == null || !type.Namespace.StartsWith("XLua") || type.Namespace.Contains("Assets.Code")
    //                   select type);

    //        return tmp;
    //    }
    //}

    [Hotfix]
    public static List<Type> by_property_has_namespace
    {
        get
        {
            var tmpType = (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                           where ((type.Namespace != null) && (type.Namespace.Contains("Assets")/* || type.Namespace.Contains("TA_IOCP")*/ || type.Namespace.Contains("PureMVC") || type.Namespace.Contains("LuaMVC") || type.Namespace.Contains("Tmp_Sieve_Drag") || type.Namespace == "Game"/*||type.Namespace == "SG"*/))
                           select type).ToList();
            //WindowDll.GetOpenFileName1
            //foreach (var item in tmpType)
            //{
            //    Debug.Log(item.Name);
            //    //if (item.Name == "WindowDll")
            //    //{
            //    //    Debug.LogWarning("WindowDll");
            //    //}
            //}
            FileStream aFile = new FileStream("C:\\Users\\Public\\Desktop\\Log_Namespace.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(aFile);
            foreach (var item in tmpType)
            {
                sw.WriteLine(item.FullName);
            }
            sw.Close();
            sw.Dispose();
            aFile.Close();
            aFile.Dispose();
            return tmpType;
        }
    }


    [Hotfix]
    public static List<Type> by_property
    {
        get
        {
            var tmpType = (from type in Assembly.Load("Assembly-CSharp").GetTypes()
                           where (type.Namespace == null && type.Name != "WindowDll" && type.Name != "FlocksWindow") && type.Name != "LocalNotification"
                           select type).ToList();
            FileStream aFile = new FileStream("C:\\Users\\Public\\Desktop\\Log_NoneNamespace.txt", FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(aFile);
            foreach (var item in tmpType)
            {
                sw.WriteLine(item.FullName);
            }
            sw.Close();
            sw.Dispose();
            aFile.Close();
            aFile.Dispose();
            //WindowDll.GetOpenFileName1
            //foreach (var item in tmpType)
            //{
            //    Debug.Log(item.Name);
            //    //if (item.Name == "WindowDll")
            //    //{
            //    //    Debug.LogWarning("WindowDll");
            //    //}
            //}
            return tmpType;
        }
    }

    //黑名单
    [XLua.BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {

                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
                new List<string>(){ "UnityEngine.Analytics", "AudioSpatializerMicrosoft" },
                //new List<string>(){ "", "Resources" },
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
                new List<string>(){ "System", "Type" },

            };


    /***************如果你全lua编程，可以参考这份自动化配置***************/
    //--------------begin 纯lua编程配置参考----------------------------

    static List<string> exclude = new List<string> {
        "UnityEngine.CanvasRenderer",
        "Func<T>",
        "System.Type",
        "T",
        "UnityEngine.UI.SGDefaultControls",
        "HideInInspector", "ExecuteInEditMode",
        "AddComponentMenu", "ContextMenu",
        "RequireComponent", "DisallowMultipleComponent",
        "SerializeField", "AssemblyIsEditorAssembly",
        "Attribute", "Types",
        "UnitySurrogateSelector", "TrackedReference",
        "TypeInferenceRules", "FFTWindow",
        "RPC", "Network", "MasterServer",
        "BitStream", "HostData",
        "ConnectionTesterStatus", "GUI", "EventType",
        "EventModifiers", "FontStyle", "TextAlignment",
        "TextEditor", "TextEditorDblClickSnapping",
        "TextGenerator", "TextClipping", "Gizmos",
        "ADBannerView", "ADInterstitialAd",
        "Android", "Tizen", "jvalue",
        "iPhone", "iOS", "Windows", "CalendarIdentifier",
        "CalendarUnit", "CalendarUnit",
        "ClusterInput", "FullScreenMovieControlMode",
        "FullScreenMovieScalingMode", "Handheld",
        "LocalNotification", "NotificationServices",
        "RemoteNotificationType", "RemoteNotification",
        "SamsungTV", "TextureCompressionQuality",
        "TouchScreenKeyboardType", "TouchScreenKeyboard",
        "MovieTexture", "UnityEngineInternal",
        "Terrain", "Tree", "SplatPrototype",
        "DetailPrototype", "DetailRenderMode",
        "MeshSubsetCombineUtility", "AOT", "Social", "Enumerator",
        "SendMouseEvents", "Cursor", "Flash", "ActionScript",
        "OnRequestRebuild", "Ping",
        "ShaderVariantCollection", "SimpleJson.Reflection",
        "CoroutineTween", "GraphicRebuildTracker",
        "Advertisements", "UnityEditor", "WSA",
        "EventProvider", "Apple",
        "ClusterInput", "Motion",
        "UnityEngine.UI.ReflectionMethodsCache", "NativeLeakDetection",
        "NativeLeakDetectionMode", "WWWAudioExtensions", "UnityEngine.Experimental","AudioSpatializerMicrosoft","SetNoBackupFlag",
        "Caching",
        "HoloLensInputModule",
        "HoloLensInput",
        "PostProcessing",
        "Purchasing",
        "AnalyticsEventParamListContainerDrawer",
        "AnalyticsEventParam",
        "CustomEnumPopup",
        "UnityEditor",
        "ListContainerDrawer",
        "StandardEventPayloadDrawer",
        "UnityEngine.Analytics",
        "StandardEventPayload",
        "ValueProperty",
        "LocalNotification",
    };

    static List<string> exlude_noneUnityEngine = new List<string>()
    {
        "System.Type",
        "Analytics",
        "FlocksWindow",
        "WindowDll",
        "FluffyUnderware",
        "Curvy",
        "Func<T>",
        "T",
        "AudioSpatializerMicrosoft",
        "SetNoBackupFlag",
        "Caching",
        "HoloLensInputModule",
        "HoloLensInput",
        "PostProcessing",
        "Purchasing",
        "AnalyticsEventParamListContainerDrawer",
        "AnalyticsEventParam",
        "CustomEnumPopup",
        "UnityEditor",
        "ListContainerDrawer",
        "StandardEventPayloadDrawer",
        "UnityEngine.Analytics",
        "StandardEventPayload",
        "ValueProperty",
        "LocalNotification",



    };

    static bool isExcluded(Type type)
    {

        var fullName = type.FullName;
        for (int i = 0; i < exclude.Count; i++)
        {
            if (fullName.Contains(exclude[i]))
            {
                return true;
            }
        }
        return false;
    }

    static bool isExcluded_NonUnityEngine(Type type)
    {
        var fullName = type.FullName;
        for (int i = 0; i < exlude_noneUnityEngine.Count; i++)
        {
            if (fullName.Contains(exlude_noneUnityEngine[i]))
            {
                return true;
            }
        }
        return false;
    }

    [LuaCallCSharp]
    public static IEnumerable<Type> LuaCallCSharp
    {
        get
        {
            var unityTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                              where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                              from type in assembly.GetExportedTypes()
                              where type.Namespace != null && !isExcluded(type) && isExcluded_NonUnityEngine(type)
                                      && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum && (type.Namespace.StartsWith("UnityEngine"))
                              select type);

            string[] customAssemblys = new string[] {
                "Assembly-CSharp",
            };
            var customTypes = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
                               from type in assembly.GetExportedTypes()
                               where type.Namespace != null && !type.Namespace.StartsWith("XLua") && !type.Namespace.StartsWith("UnityEngine") && (type.Namespace.Contains("Assets")/* || type.Namespace.Contains("TA_IOCP")*/ || type.Namespace.Contains("PureMVC") || type.Namespace.Contains("LuaMVC") || type.Namespace.Contains("Tmp_Sieve_Drag") || type.Namespace == "Game"/*||type.Namespace == "SG"*/)
                                       && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
                               select type);
            var custom = (from assembly in customAssemblys.Select(s => Assembly.Load(s))
                          from type in assembly.GetExportedTypes()
                          where type.Namespace == null && !isExcluded_NonUnityEngine(type)
                                  && type.BaseType != typeof(MulticastDelegate) && !type.IsInterface && !type.IsEnum
                          select type);
            //foreach (var item in customTypes)
            //{
            //    if (item.Namespace != null)
            //    {
            //        Debug.Log(item.Namespace);

            //    }
            //    else
            //    {
            //        Debug.Log(" 1111111             " + item.FullName);
            //    }
            //}
            return unityTypes.Concat(customTypes).Concat(custom);
        }
    }



    ////自动把LuaCallCSharp涉及到的delegate加到CSharpCallLua列表，后续可以直接用lua函数做callback
    [CSharpCallLua]
    public static List<Type> CSharpCallLua
    {
        get
        {
            var lua_call_csharp = LuaCallCSharp;
            var delegate_types = new List<Type>();
            var flag = BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly;
            foreach (var field in (from type in lua_call_csharp select type).SelectMany(type => type.GetFields(flag)))
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    delegate_types.Add(field.FieldType);
                }
            }

            foreach (var method in (from type in lua_call_csharp select type).SelectMany(type => type.GetMethods(flag)))
            {
                if (typeof(Delegate).IsAssignableFrom(method.ReturnType))
                {
                    delegate_types.Add(method.ReturnType);
                }
                foreach (var param in method.GetParameters())
                {
                    var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                    if (typeof(Delegate).IsAssignableFrom(paramType))
                    {
                        delegate_types.Add(paramType);
                    }
                }
            }
            return delegate_types.Distinct().ToList();
        }
    }




    //--------------begin 热补丁自动化配置-------------------------
    public static bool hasGenericParameter(Type type)
    {
        if (type.IsGenericTypeDefinition) return true;
        if (type.IsGenericParameter) return true;
        if (type.IsByRef || type.IsArray)
        {
            return hasGenericParameter(type.GetElementType());
        }
        if (type.IsGenericType)
        {
            foreach (var typeArg in type.GetGenericArguments())
            {
                if (hasGenericParameter(typeArg))
                {
                    return true;
                }
            }
        }
        return false;
    }

    //// 配置某Assembly下所有涉及到的delegate到CSharpCallLua下，Hotfix下拿不准那些delegate需要适配到lua function可以这么配置
    //[CSharpCallLua]
    //public static IEnumerable<Type> AllDelegate
    //{
    //    get
    //    {
    //        BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
    //        List<Type> allTypes = new List<Type>();
    //        var allAssemblys = new Assembly[]
    //        {
    //            Assembly.Load("Assembly-CSharp")
    //        };
    //        foreach (var t in (from assembly in allAssemblys from type in assembly.GetTypes() select type))
    //        {
    //            var p = t;
    //            while (p != null)
    //            {
    //                allTypes.Add(p);
    //                p = p.BaseType;
    //            }
    //        }
    //        allTypes = allTypes.Distinct().ToList();
    //        var allMethods = from type in allTypes
    //                         from method in type.GetMethods(flag)
    //                         select method;
    //        var returnTypes = from method in allMethods
    //                          select method.ReturnType;
    //        var paramTypes = allMethods.SelectMany(m => m.GetParameters()).Select(pinfo => pinfo.ParameterType.IsByRef ? pinfo.ParameterType.GetElementType() : pinfo.ParameterType);
    //        var fieldTypes = from type in allTypes
    //                         from field in type.GetFields(flag)
    //                         select field.FieldType;
    //        return (returnTypes.Concat(paramTypes).Concat(fieldTypes)).Where(t => t.BaseType == typeof(MulticastDelegate) && !hasGenericParameter(t)).Distinct();
    //    }
    //}







    #region 





    #endregion






}
