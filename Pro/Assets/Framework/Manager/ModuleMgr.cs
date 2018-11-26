using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class ModuleMgr : MonoBehaviour
    {
        #region 单例
        private static ModuleMgr _instance = null;
        public static ModuleMgr Instance{ get { return Util.GetInstance(ref _instance, "_ModuleMgr"); }}
        #endregion

        #region 字段和属性
        private Dictionary<string, Module> _moduleDict = new Dictionary<string, Module>();
        #endregion

        #region 方法
        public Module GetModule(string moduleName)
        {
            Module module = null;
            if (!_moduleDict.TryGetValue(moduleName, out module))
            {
                module = new GameObject(moduleName).AddComponent<Module>();
                module.transform.SetParent(transform);
                _moduleDict.Add(moduleName, module);
            }
            return module;
        }
        #endregion
    }
}
