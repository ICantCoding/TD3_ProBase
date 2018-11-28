using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class ObjectPool : MonoSingleton<ObjectPool>
    {
        #region 字段和属性
        public Dictionary<string, SubPool> m_subPoolDict = new Dictionary<string, SubPool>();
        #endregion


        #region Unity生命周期
        void Awake()
        {
            AddSubPool(PoolInfo.SubPool1Name, PoolInfo.SubPool1PrefabName); //添加某个子对象池
            //AddSubPool(PoolInfo.SubPool2Name, PoolInfo.SubPool2PrefabName); //添加某个子对象池
        }
        #endregion


        #region 公有方法
        public GameObject OnSpawn(string subPoolName)
        {
            if (string.IsNullOrEmpty(subPoolName)) return null;
            if (m_subPoolDict.ContainsKey(subPoolName))
            {
                return m_subPoolDict[subPoolName].OnSpawn();
            }
            return null;
        }
        public void OnUnSpawn(string subPoolName, GameObject go)
        {
            if (go == null) return;
            if (m_subPoolDict.ContainsKey(subPoolName))
            {
                m_subPoolDict[subPoolName].OnUnSpawn(go);
            }
        }
        #endregion
        

        #region 私有方法
        private void AddSubPool(string subPoolName, string prefabName)
        {
            if (string.IsNullOrEmpty(subPoolName)) return;
            SubPool subPool = new SubPool(subPoolName, prefabName);
            if (subPool != null)
            {
                m_subPoolDict.Add(subPoolName, subPool);
            }
        }
        #endregion
    }
}

