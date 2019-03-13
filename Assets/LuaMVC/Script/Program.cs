using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;
namespace LuaMVC

{
    public class Program : MonoBehaviour
    {
        public static LuaEnv luaEnv = new LuaEnv();
        public delegate void OnSendNotification(object notificationName, object body, object type);
        private LuaTable scriptEnv = null;
        private static OnSendNotification SendNotificationHandle;
        public LuaFunction LuaFunction;

        // Use this for initialization

        void Start()
        {
            luaEnv.AddLoader(LoadLuaFromAssetbundle);
            this.scriptEnv = luaEnv.NewTable();
            LuaTable meta = luaEnv.NewTable();
            meta.Set("__index", luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();
            //luaEnv.AddBuildin("rapidjson", XLua.LuaDLL.Lua.LoadRapidJson);
            //luaEnv.AddLoader(LoadLua);
            luaEnv.DoString(GetChunkAddress(), "First");
            if (SendNotificationHandle == null)
            {
                luaEnv.DoString("local Facade=require('LuaScripts/PureMVC/Patterns/Facade/Facade')  local facade = Facade.Instance()    function CS_SendNotification(name,body,type) if Facade then facade:SendNotification(name,body,tyoe)  end end", "GetFunc");
                SendNotificationHandle = scriptEnv.Get<OnSendNotification>("CS_SendNotification");
            }

        }
        private byte[] LoadLuaFromAssetbundle(ref string filePath)
        {
            string url = filePath;
            string[] scriptInfo = filePath.Split('/');
            LuaMVC.ResourceManager.Instance.LoadLuaScript(filePath.Substring(0, filePath.LastIndexOf("/")), scriptInfo[scriptInfo.Length - 1], (string objects) =>
            {
                if (!string.IsNullOrEmpty(objects))
                {

                    url = objects;

                }
                else
                {
                    Debug.LogError("not find this lua script: " + url);
                }
            });
            if (url == filePath)
            {
                Debug.LogError("require luaScript Error: " + url);
            }            
            return System.Text.Encoding.UTF8.GetBytes(url);            


        }
        private string GetChunkAddress()
        {
            switch (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                case "Tmp":
                    return "require('Scene_Tmp/LuaScripts/Main');   Main()   ";              
                default:
                    Debug.LogError("未对当前游戏场景配置入口lua文件");
                    return null;
            }
        }
        public static void SendNotification(string name, object body = null, object type = null)
        {
            if (SendNotificationHandle != null)
            {
                try
                {
                    SendNotificationHandle(name, body, type);
                }
                catch (Exception ex)
                {
                    Debug.LogError("SendNotificationHandle error :" + ex.ToString());
                    //throw;
                }
            }
            else
            {
                Debug.LogError("SendNotificationHandle 为null");
            }
        }
        public byte[] LoadLua(ref string path)
        {
            path = path.Replace("\\", "/");
            path = path.Replace("//", "/");

            var tmp = path.Split('/');
            string abName = null;
            for (int i = 0; i < tmp.Length; i++)
            {
                if (i == tmp.Length - 1)
                {
                    continue;
                }
                abName += (tmp[i] + "/");
            }

            string assetName = tmp[tmp.Length - 1];
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
            return System.Text.Encoding.UTF8.GetBytes(textAsset.text);
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
        void Update()
        {
            luaEnv.Tick();
        }

        private void OnApplicationQuit()
        {
            LuaMVC.Program.SendNotification(NotificationType.ReleaseDelegate);
            luaEnv.DoString("  if Facade  then    local cs_facade = Facade.Instance  cs_facade:Clean() end");
            LuaMVC.Program.luaEnv.FullGc();
            luaEnv.Dispose();
        }

    }
}
