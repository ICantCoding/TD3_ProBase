using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TD3_Framework
{
    [System.Serializable]
    public class SubPool
    {
        #region 字段和属性
        public string m_poolName;                           //对象池名称
        public string m_prefabName;                         //对象预制体路径
        private GameObject m_prefab;                        //预制体
        private List<ReusableObject> m_goList;              //对象池中的物体持有的脚本
        #endregion


        #region 构造方法
        public SubPool(string poolName, string prefabName)
        {
            this.m_poolName = poolName;
            this.m_prefabName = prefabName;
            InitSubPool();
        }
        #endregion


        #region 公有方法
        //对象池-取游戏物体
        public GameObject OnSpawn()
        {
            GameObject go = null;
            go = GetIdleReuasbleGo();
            if (go == null) //如果对象池中没有空闲的物体，则需要创建新物体对象
            {
                go = GameObject.Instantiate(m_prefab);
                ReusableObject reusableCom = go.GetComponent<ReusableObject>();
                if (reusableCom != null)
                {
                    m_goList.Add(reusableCom);
                    reusableCom.IsIdle = false;  //将物体置于正在被使用的状态
                }
                else
                {
                    GameObject.Destroy(go); //如果物体不具有ReusableObject则销毁创建出来的物体对象
                    go = null;
                }
            }
            return go;
        }
        //对象池-放回游戏物体
        public void OnUnSpawn(GameObject go)
        {
            if (go == null) return;
            ReusableObject reusableCom = go.GetComponent<ReusableObject>();
            if (reusableCom != null)
            {
                reusableCom.IsIdle = true;  //将物体置于空闲状态
            }
        }
        #endregion


        #region 私有方法
        //初始化对象池
        public void InitSubPool()
        {
            if (string.IsNullOrEmpty(m_prefabName))
            {
                Debug.LogWarning(m_poolName + "对象池没有设置预制体路径");
                return;
            }
            m_goList = new List<ReusableObject>();
            StringBuilder prefabPathStr = new StringBuilder();
            prefabPathStr.Append("ObjectPool/").Append(m_prefabName);
            Debug.Log("Path: " + prefabPathStr);
            m_prefab = Resources.Load<GameObject>(prefabPathStr.ToString());
        }
        //获取对象池中空闲还没有使用的物体
        private GameObject GetIdleReuasbleGo() 
        {
            foreach (ReusableObject com in m_goList)
            {
                if (com.IsIdle == true)
                {
                    return com.gameObject;
                }
            }
            return null;
        }
        #endregion

    }
}
