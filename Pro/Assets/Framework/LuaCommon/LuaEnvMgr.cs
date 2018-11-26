using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using XLua;

namespace TD3_Framework
{


    public class LuaEnvMgr : MonoBehaviour
    {

        #region 单例
        private static LuaEnvMgr _instance = null;
        public static LuaEnvMgr Instance
        {
            get
            {
                return Util.GetInstance<LuaEnvMgr>(ref _instance, "_LuaEnvMgr", true);
            }
        }
        #endregion

        #region 字段和属性
        private static LuaEnv luaEnv = new LuaEnv();
        public LuaEnv LuaEnv
        {
            get { return luaEnv; }
        }
        private static float lastGCTime = 0.0f;
        private static float GCInterval = 1.0f;
        #endregion


        #region Unity生命周期方法
        void Awake()
        {
            luaEnv.AddLoader((ref string filepath) =>
            {
                return GetLuaText(filepath);
            });
        }
        void Update()
        {
            if (Time.time - lastGCTime > GCInterval)
            {
                luaEnv.Tick();
                lastGCTime = Time.time;
            }
        }
        #endregion

        #region 方法
        public byte[] GetLuaText(string path)
        {
            string url = path;
            if (File.Exists(url))
            {
                return File.ReadAllBytes(url);
            }
            else
            {
                Debug.LogError(url + "don't exists.");
                return null;
            }
        }
        public void CallLua(string lua)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("require ('");
            sb.Append(lua);
            sb.Append("') Main() ");
            luaEnv.DoString(sb.ToString());
        }
        public void FastTick()
        {
            GCInterval = 0;
        }
        public void RestoreTick()
        {
            GCInterval = 1;
        }
        #endregion


    }
}

