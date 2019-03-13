
using System;
using XLua;
using UnityEngine;

namespace LuaMVC
{
    public class LuaMonobehaviour : MonoBehaviour
    {
        //private ILuaBaseView luaBaseView = null;
        private Action luaStart = null;
        private Action luaUpdate = null;
        private Action luaOnDestroy = null;
        private LuaTable scriptEnv = null;

        public void Init(string scriptName)
        {

            scriptEnv = LuaMVC.Program.luaEnv.NewTable();


            LuaTable meta = Program.luaEnv.NewTable();
            meta.Set("__index", Program.luaEnv.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();
            // 加载lua script                    
            LuaMVC.Program.luaEnv.DoString("require " + "'" + scriptName + "'", scriptName, scriptEnv);
            string[] array = scriptName.Split('/'); //此处是因为加载的lua地址是全路径  而lua脚本名只有第一个
            scriptName = array[array.Length - 1];
            scriptEnv.SetInPath<MonoBehaviour>(scriptName + ".self", this);  //把自身脚本注册进lua脚本中 示例  scriptName.self = this
            if (null != this.transform.Find("Panel"))
                scriptEnv.SetInPath<GameObject>(scriptName + ".panel", this.transform.Find("Panel").gameObject);
            luaStart = scriptEnv.GetInPath<Action>(scriptName + ".start");
            luaUpdate = scriptEnv.GetInPath<Action>(scriptName + ".update");
            luaOnDestroy = scriptEnv.GetInPath<Action>(scriptName + ".ondestroy");
            Action luaAwake = scriptEnv.GetInPath<Action>(scriptName + ".awake");

            if (null != luaAwake)
            {

                luaAwake();


            }

        }

        #region Life cycle functions

        private void Start()
        {
            if (null != luaStart)
                luaStart();
        }
        private void Update()
        {
            if (null != luaUpdate)
                luaUpdate();
        }
        private void OnDestroy()
        {
            if (null != luaOnDestroy)
                luaOnDestroy();
            luaStart = null;
            luaUpdate = null;
            luaOnDestroy = null;
            
            if (scriptEnv != null && Program.luaEnv != null)
            {
                //scriptEnv.Dispose();
                scriptEnv = null;
            }

        }

        #endregion
    }
}
