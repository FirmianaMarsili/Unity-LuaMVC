/*******************************************
 * desc:assetBundle加载和卸载 可以自动加载卸载依赖
 * 
 * Update: 2018.8.2
 * 
********************************************/


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using XLua;
using UObject = UnityEngine.Object;
namespace LuaMVC
{
    [LuaCallCSharp]
    public class AssetBundleInfo
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        public AssetBundleInfo(AssetBundle assetBundle)
        {
            m_AssetBundle = assetBundle;
            m_ReferencedCount = 0;
        }
    }
    [LuaCallCSharp]
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager _instance;
        public static ResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ResourceManager");
                    _instance = go.AddComponent<ResourceManager>();
                    //DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }



        string m_BaseDownloadingURL = "";
        /// <summary>
        /// 所有bundle名字
        /// </summary>
        string[] m_AllManifest = null;
        AssetBundleManifest m_AssetBundleManifest = null;
        Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>>();

        class LoadAssetRequest
        {
            public Type assetType;
            public string[] assetNames;
            public Action<UObject[]> sharpFunc;
            public Action<string> luaFunc;

        }

        //void Awake()
        //{
        //    Instance = this;
        //}
        // Load AssetBundleManifest.
        public void Initialize(string manifestName, Action initOK)
        {
            m_BaseDownloadingURL = Utils_LuaMVC.GetRelativePath() + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "/";
            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate (UObject[] objs)
            {
                if (objs.Length > 0)
                {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                    m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();
                }
                if (initOK != null)
                {
                    initOK();
                }
            });
        }
        //public void LoadUI<T>(string abName, string assetName, Action<UObject[]> func)
        //{

        //    LoadAsset<type>(abName, new string[] { assetName }, func);

        //}

        public void LoadLuaScript(string abName, string assetName, Action<string> func)
        {
            if (AppConst.isLoadFromResources)
            {
                LoadLuaScriptFromResources(abName, assetName, func);
                return;
            }

            if (abName.StartsWith("LuaScripts"))
            {
                LoadLuaFramwork(abName, assetName, func);
            }
            else
            {
                if (!assetName.Contains(".lua.txt"))
                {
                    assetName += ".lua.txt";
                }
                LoadLuaAsset<TextAsset>(abName, new string[] { assetName }, func);
            }
        }

        private void LoadLuaFramwork(string abName, string assetNames, Action<string> action = null)
        {
            string url = Resources.Load<TextAsset>(abName + "/" + assetNames + ".lua").text;
            //string text = File.ReadAllText(url);
            if (url != null)
            {
                action(url);
            }
            else
            {
                throw new Exception("load luaframwork error:" + abName + "/" + assetNames + ".lua");
            }
        }
        private string Recursive(string path, string assetName)
        {
            if (!Directory.Exists(path)) return null;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                var info = files[i];
                if (info.Name.EndsWith(".meta")) continue;
                if (Path.GetFileNameWithoutExtension(info.Name) == assetName)
                    return info.FullName;
            }
            return null;

        }

        void LoadLuaScriptFromResources(string abName, string assetName, Action<string> func)
        {
            if (abName.StartsWith("LuaScripts"))
            {
                string streamingUrl = null;
                try
                {
                    streamingUrl = Resources.Load<TextAsset>(abName + "/" + assetName + ".lua").text;
                }
                catch (Exception)
                {
                    Debug.LogError("resource error : " + abName + "/" + assetName + ".lua");
                    throw;
                }
                     
                if (string.IsNullOrEmpty(streamingUrl))
                {
                    throw new Exception("load lua stream error :" + abName + "/" + assetName + ".lua");
                }
                else
                {
                    if (func != null)
                    {
                        func(streamingUrl);
                    }
                }
                return;

            }
            if (!assetName.Contains(".lua.txt"))
            {
                assetName += ".lua";
            }
            string url = Recursive(Application.dataPath + "/LuaMVC/Bundle/" + abName, assetName);
            if (url == null)
                throw new Exception("load lua error : " + abName + "/" + assetName);
            url = url.Replace("\\", "/");
            url = url.Replace(Application.dataPath, string.Empty);
            url = string.Format("Assets{0}", url);
            TextAsset textAsset = null;
#if UNITY_EDITOR
            textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(url) as TextAsset;
#endif
            if (textAsset == null)
                throw new Exception("load lua error 1  : " + abName + "/" + assetName);
            if (func != null)
            {
                func(textAsset.text);
            }

        }

        public void LoadAssetBundle<T>(string name, string assetName, Action<UObject[]> func) where T : UObject
        {
            LoadAsset<T>(name, new string[] { assetName }, func);
        }
        public void LoadAssetBundle<T>(string name, string[] assetName, Action<UObject[]> func) where T : UObject
        {
            LoadAsset<T>(name, assetName, func);
        }
        public void LoadSprite(string name, string[] assetName, Action<UObject[]> func)
        {
            LoadAsset<Sprite>(name, assetName, func);
        }
        public void LoadSprite(string name, string assetName, Action<UObject[]> func)
        {
            LoadAsset<Sprite>(name, new string[] { assetName }, func);
        }

        public void LoadPrefab(string abName, string assetName, Action<UObject[]> func)
        {
            LoadAsset<GameObject>(abName, new string[] { assetName }, func);
        }

        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func)
        {
            LoadAsset<GameObject>(abName, assetNames, func);
        }

        public void LoadAudio(string abName, string[] assetNames, Action<UObject[]> func)
        {
            LoadAsset<AudioClip>(abName, assetNames, func);
        }
        public void LoadFont(string abName, string[] assetNames, Action<UObject[]> func)
        {
            LoadAsset<Font>(abName, assetNames, func);
        }



        string GetRealAssetPath(string abName)
        {
            if (abName.Equals(AppConst.AssetDir))
            {
                return abName;
            }
            abName = abName.ToLower();
            if (!abName.EndsWith(AppConst.ExtName))
            {
                abName += AppConst.ExtName;
            }
            if (abName.Contains("/"))
            {
                return abName;
            }
            //string[] paths = m_AssetBundleManifest.GetAllAssetBundles();  产生GC，需要缓存结果
            for (int i = 0; i < m_AllManifest.Length; i++)
            {
                int index = m_AllManifest[i].LastIndexOf('/');
                string path = m_AllManifest[i].Remove(0, index + 1);    //字符串操作函数都会产生GC
                if (path.Equals(abName))
                {
                    return m_AllManifest[i];
                }
            }
            Debug.LogError("GetRealAssetPath Error:>>" + abName);
            return null;
        }
        #region 异步加载资源
        /// <summary>
        /// 载入素材
        /// </summary>
        void LoadAsset<T>(string abName, string[] assetNames, Action<UObject[]> action = null) where T : UObject
        {
            if (AppConst.isLoadFromResources)
            {
                if (abName == AppConst.AssetDir)
                {
                    return;
                }
                LoadPrefabFromResources<T>(abName, assetNames, action);
                return;

            }
            abName = GetRealAssetPath(abName);
            if (typeof(T) != typeof(AssetBundleManifest))
            {
                AddDependencies(abName, true);
            }
            LoadAssetRequest request = new LoadAssetRequest();
            {
                request.assetType = typeof(T);
                request.assetNames = assetNames;
                request.sharpFunc = action;
            }

            List<LoadAssetRequest> requests = null;
            if (!m_LoadRequests.TryGetValue(abName, out requests))
            {
                requests = new List<LoadAssetRequest>
                {
                    request
                };
                m_LoadRequests.Add(abName, requests);
                StartCoroutine(OnLoadAsset<T>(abName));
            }
            else
            {
                requests.Add(request);
                m_LoadRequests[abName] = requests;
            }
        }
        void AddDependencies(string abName, bool isFirst = false)
        {
            string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
            if (dependencies.Length > 0)
            {
                for (int i = 0; i < dependencies.Length; i++)
                {
                    AddDependencies(dependencies[i]);
                }
            }
            if (!isFirst)
            {
                List<LoadAssetRequest> requests = null;
                if (!m_LoadRequests.TryGetValue(abName, out requests))
                {
                    requests = new List<LoadAssetRequest>()
                    {
                        null
                    };
                    m_LoadRequests.Add(abName, requests);
                }
                else
                {
                    requests.Add(null);
                    m_LoadRequests[abName] = requests;
                }
            }
        }

        IEnumerator OnLoadAsset<T>(string abName) where T : UObject
        {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
            if (bundleInfo == null)
            {
                yield return StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));
            }
            else
            {
                List<LoadAssetRequest> list = null;
                if (!m_LoadRequests.TryGetValue(abName, out list))
                {
                    m_LoadRequests.Remove(abName);
                    yield break;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        string[] assetNames = list[i].assetNames;
                        List<UObject> result = new List<UObject>();

                        AssetBundle ab = bundleInfo.m_AssetBundle;
                        for (int j = 0; j < assetNames.Length; j++)
                        {
                            string assetPath = assetNames[j];
                            AssetBundleRequest request = ab.LoadAssetAsync(assetPath, list[i].assetType);
                            yield return request;
                            result.Add(request.asset);
                            ///重置资源的shader
#if UNITY_EDITOR
                            LuaMVC.Utils_LuaMVC.ResetShader(request.asset);
#endif
                        }
                        if (list[i].sharpFunc != null)
                        {
                            //list[i].sharpFunc(result.ToArray());
                            //list[i].sharpFunc = null;
                            StartCoroutine(DoAction(list[i].sharpFunc, result.ToArray()));
                        }
                        bundleInfo.m_ReferencedCount++;
                    }
                }
                m_LoadRequests.Remove(abName);
            }

        }

        IEnumerator OnLoadAssetBundle(string abName, Type type)
        {
            string url = m_BaseDownloadingURL + abName;

            WWW download = null;
            if (type == typeof(AssetBundleManifest))
                download = new WWW(url);
            else
            {
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                if (dependencies.Length > 0)
                {
                    if (!m_Dependencies.ContainsKey(abName))
                    {
                        m_Dependencies.Add(abName, dependencies);
                    }
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string depName = dependencies[i];
                        AssetBundleInfo bundleInfo = null;
                        if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
                        {
                            bundleInfo.m_ReferencedCount++;
                        }
                        else/* if (!m_LoadRequests.ContainsKey(depName))*/
                        {
                            yield return StartCoroutine(OnLoadAssetBundle(depName, type));
                        }
                    }
                }
                download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(abName), 0);
            }
            yield return download;
            AssetBundle assetObj = null;
            try
            {
                assetObj = download.assetBundle;
            }
            catch (Exception)
            {
                Debug.Log("");
                throw;
            }

            if (assetObj != null)
            {
                var bundleInfo = new AssetBundleInfo(assetObj);
                m_LoadedAssetBundles.Add(abName, bundleInfo);


                List<LoadAssetRequest> list = null;
                if (!m_LoadRequests.TryGetValue(abName, out list))
                {
                    m_LoadRequests.Remove(abName);
                    yield break;
                }
                else
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] != null)
                        {
                            string[] assetNames = list[i].assetNames;
                            List<UObject> result = new List<UObject>();

                            AssetBundle ab = bundleInfo.m_AssetBundle;
                            for (int j = 0; j < assetNames.Length; j++)
                            {
                                string assetPath = assetNames[j];
                                AssetBundleRequest request = ab.LoadAssetAsync(assetPath, list[i].assetType);
                                yield return request;
                                result.Add(request.asset);
                                ///重置资源的shader
#if UNITY_EDITOR
                                LuaMVC.Utils_LuaMVC.ResetShader(request.asset);
#endif
                            }
                            if (list[i].sharpFunc != null)
                            {
                                StartCoroutine(DoAction(list[i].sharpFunc, result.ToArray()));
                            }
                            bundleInfo.m_ReferencedCount++;
                        }
                    }
                    m_LoadRequests.Remove(abName);
                }
            }
        }
        #endregion

        IEnumerator DoAction(Action<UObject[]> sharpFunc, UObject[] result)
        {
            sharpFunc(result);
            yield return sharpFunc = null;
        }

        AssetBundleInfo GetLoadedAssetBundle(string abName)
        {
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
            if (bundle == null) return null;

            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return bundle;

            foreach (var dependency in dependencies)
            {
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null) return null;
            }
            return bundle;
        }


        public void OnlyLoadLoadingAsset(string abName, string assetName, Action<UObject[]> action = null)
        {
            string url = m_BaseDownloadingURL + abName;
            if (url.Contains("file://"))   //File.ReadAllBytes  不支持url格式
            {
                url = url.Replace("file://", null);
            }
            AssetBundle abLoading = AssetBundle.LoadFromMemory(File.ReadAllBytes(url));
            if (abLoading != null)
            {
                UObject uObject = abLoading.LoadAsset(assetName, typeof(GameObject));
                if (action != null)
                {
                    action(new UObject[] { uObject });
                    uObject = null;
                    abLoading.Unload(false);
                }
            }
            else
            {
                Debug.LogError("bundle is null : " + abName);
            }
        }

        #region 卸载资源
        /// <summary>
        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnloadAssetBundle(string abName, bool isThorough = false)
        {
            if (AppConst.isLoadFromResources)
            {
                return;
            }
            abName = GetRealAssetPath(abName);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
            UnloadAssetBundleInternal(abName, isThorough);
            UnloadDependencies(abName, isThorough);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
            //Resources.UnloadUnusedAssets();
        }
        public void UnloadAssetBundle(string[] abName, bool isThorough = false)
        {
            if (AppConst.isLoadFromResources)
            {
                return;
            }
            for (int i = 0; i < abName.Length; i++)
            {
                string tmpName = abName[i];
                tmpName = GetRealAssetPath(tmpName);
                //Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName[i]);
                UnloadAssetBundleInternal(tmpName, isThorough);
                UnloadDependencies(tmpName, isThorough);
                //Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName[i]);
            }
        }


        void UnloadDependencies(string abName, bool isThorough)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return;

            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency, isThorough);
            }
            m_Dependencies.Remove(abName);
        }

        void UnloadAssetBundleInternal(string abName, bool isThorough)
        {
            AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
            if (bundle == null) return;

            //if (--bundle.m_ReferencedCount <= 0)  //目前记录次数的功能不太适合 先注释
            //{
            if (m_LoadRequests.ContainsKey(abName))
            {
                return;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
            }
            bundle.m_AssetBundle.Unload(isThorough);
            bundle.m_AssetBundle = null;
            m_LoadedAssetBundles.Remove(abName);
            Debug.Log(abName + " has been unloaded successfully");
            //}
        }

        public void UnLoadAllAssetBundleOnChangeScene(bool isTrue)
        {
            var tmp = new Dictionary<string, AssetBundleInfo>(m_LoadedAssetBundles);
            foreach (var item in tmp.Keys)
            {
                UnloadAssetBundle(item, true);
            }

            m_AllManifest = null;
            m_AssetBundleManifest = null;
            m_BaseDownloadingURL = "";
            m_Dependencies.Clear();
            m_LoadedAssetBundles.Clear();
            m_LoadRequests.Clear();
            Resources.UnloadUnusedAssets();
        }
        #endregion


        #region 同步加载lua脚本
        void LoadLuaAsset<T>(string abName, string[] assetNames, Action<string> action = null) where T : UObject
        {
            abName = GetRealAssetPath(abName);
            LoadAssetRequest request = new LoadAssetRequest();
            {
                request.assetType = typeof(T);
                request.assetNames = assetNames;
                request.luaFunc = action;
            }
            List<LoadAssetRequest> requests = null;
            if (!m_LoadRequests.TryGetValue(abName, out requests))
            {
                requests = new List<LoadAssetRequest>
                {
                    request
                };
                m_LoadRequests.Add(abName, requests);
                OnLoadLuaAsset<T>(abName);
            }
            else
            {
                requests.Add(request);
            }
        }
        void OnLoadLuaAsset<T>(string abName) where T : UObject
        {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
            if (bundleInfo == null)
            {
                OnLoadLuaAssetBundle(abName, typeof(T));
                bundleInfo = GetLoadedAssetBundle(abName);
                if (bundleInfo == null)
                {
                    m_LoadedAssetBundles.Remove(abName);
                    Debug.LogError("OnLoad LuaScript Error: " + abName);
                    return;
                }
            }
            List<LoadAssetRequest> list = null;
            if (!m_LoadRequests.TryGetValue(abName, out list))
            {
                m_LoadRequests.Remove(abName);
                return;
            }
            for (int i = 0; i < list.Count; i++)
            {
                string[] assetNames = list[i].assetNames;
                //List<UObject> result = new List<UObject>();

                AssetBundle ab = bundleInfo.m_AssetBundle;
                //for (int j = 0; j < assetNames.Length; j++)
                //{
                string assetPath = assetNames[0];
                UObject uObject = ab.LoadAsset(assetPath, list[i].assetType);
                //yield return request;
                //result.Add(uObject);
                //}
                if (list[i].luaFunc != null)
                {
                    list[i].luaFunc((uObject as TextAsset).text);
                    list[i].luaFunc = null;
                }
                bundleInfo.m_ReferencedCount++;
            }
            m_LoadRequests.Remove(abName);
        }
        void OnLoadLuaAssetBundle(string abName, Type type)
        {
            string url = m_BaseDownloadingURL + abName;
            if (url.Contains("file://"))   //File.ReadAllBytes  不支持url格式
            {
                url = url.Replace("file://", null);
                //Debug.Log("url: " + url);
            }
            AssetBundle abLuaScript = AssetBundle.LoadFromMemory(File.ReadAllBytes(url));
            if (abLuaScript != null)
            {
                m_LoadedAssetBundles.Add(abName, new AssetBundleInfo(abLuaScript));
            }
            else
            {
                Debug.LogError("bundle is null : " + abName);
            }
        }
        //public UObject[] GetObjects()
        //{
        //    //Debug.Log("Test");
        //    return new UObject[] { };
        //}

        void LoadPrefabFromResources<T>(string abName, string[] assetNames, Action<UObject[]> action = null) where T : UObject
        {
            List<UObject> result = new List<UObject>();
            for (int i = 0; i < assetNames.Length; i++)
            {
                string assetName = assetNames[i];
                string url = Recursive(Application.dataPath + "/LuaMVC/Bundle/" + abName, assetName);
                if (url == null)
                    throw new Exception("load asset error : " + abName + "/" + assetName);
                url = url.Replace("\\", "/");
                url = url.Replace(Application.dataPath, string.Empty);
                url = string.Format("Assets{0}", url);
                T textAsset = null;
#if UNITY_EDITOR
                textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(url) as T;
#endif

                if (textAsset == null)
                    throw new Exception("load asset error 1  : " + abName + "/" + assetName);
                result.Add(textAsset);
            }
            if (action != null)
            {
                action(result.ToArray());
            }
            /*

            List<UObject> result = new List<UObject>();            
            for (int i = 0; i < assetNames.Length; i++)
            {
                UObject obj = Resources.Load<T>(abName + "/" + assetNames[i]);
                if (obj == null)
                {
                    Debug.LogError("not contain this res: " + abName + "/" + assetNames[i]);
                    continue;
                }
                result.Add(obj);

            }
            if (action != null)
            {
                action(result.ToArray());
                action = null;
            }
            */

        }
        #endregion




    }
}
