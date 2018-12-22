

namespace TD3_Framework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    public class AssetBundleManager
    {

        #region 单例
        private static readonly object synclock = new object();
        private static AssetBundleManager m_instance = null;
        public static AssetBundleManager Instance
        {
            get
            {
                if(m_instance == null)
                {
                    lock(synclock){
                        if(m_instance == null){
                            m_instance = new AssetBundleManager();
                            m_instance.LoadAssetBundleConfig();
                        }
                    }
                }
                return m_instance;
            }
        }
        #endregion

        #region 数据字段和属性
        //资源依赖关系配置表，可以根据crc来找到对应的资源块
        protected Dictionary<uint, ResourceItem> m_resourceItemDict = new Dictionary<uint, ResourceItem>();
        //储存已经加载AB包        
        protected Dictionary<uint, AssetBundleItem> m_assetBundleItemDict = new Dictionary<uint, AssetBundleItem>();
        //AssetBundleItem的对象池
        protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);
        #endregion

        #region	方法
        //加载ABConfig配置表
        public bool LoadAssetBundleConfig()
        {
            m_resourceItemDict.Clear();

            string configPath = GlobalConfigData.DependenceFile4AssetBundle;
            AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
            TextAsset textAsset = configAB.LoadAsset<TextAsset>(GlobalConfigData.DependenceFileName);
            if (textAsset == null)
            {
                Debug.LogError("AssetBundleConfig is not exists!");
                return false;
            }
            MemoryStream ms = new MemoryStream(textAsset.bytes);
            BinaryFormatter bf = new BinaryFormatter();
            AssetBundleConfig abConfig = (AssetBundleConfig)bf.Deserialize(ms);
            ms.Close();

            for (int i = 0; i < abConfig.ABList.Count; i++)
            {
                ResourceItem item = new ResourceItem();
                ABBase abBase = abConfig.ABList[i];
                item.Crc = abBase.Crc;
                item.AssetName = abBase.AssetName;
                item.ABName = abBase.ABName;
                item.DependABList = new List<string>(abBase.DependABList);
                if (m_resourceItemDict.ContainsKey(item.Crc) == false)
                {
                    m_resourceItemDict.Add(item.Crc, item);
                }
                else
                {
                    Debug.Log("重复的Crc, 资源名: " + item.AssetName + ", AB包名: " + item.ABName);
                }
            }
            return true;
        }
        //加载ResourceItem
        public ResourceItem LoadResourceAssetBundle(uint crc)
        {
            ResourceItem resourceItem = null;
            m_resourceItemDict.TryGetValue(crc, out resourceItem);
            if (resourceItem == null)
            {
                Debug.LogError("在AssetBundle中, 没有找到Crc: " + crc.ToString() + "对应的资源!");
                return null;
            }
            if (resourceItem.Ab != null)
            {
                return resourceItem;
            }
            if (resourceItem.DependABList != null)
            {
                for (int i = 0; i < resourceItem.DependABList.Count; i++)
                {
                    LoadAssetBundle(resourceItem.DependABList[i]);
                }
            }
            resourceItem.Ab = LoadAssetBundle(resourceItem.ABName);
            return resourceItem;
        }
        //释放ResourceItem
        public void ReleaseAsset(ResourceItem item)
        {
            if (item == null) return;

            if (item.DependABList != null && item.DependABList.Count > 0)
            {
                for (int i = 0; i < item.DependABList.Count; i++)
                {
                    UnloadAssetBundle(item.DependABList[i]);
                }
            }
            UnloadAssetBundle(item.ABName);
        }
        //加载AssetBundle
        public AssetBundle LoadAssetBundle(string name)
        {
            AssetBundleItem abItem = null;
            uint crc = Crc.StringToCRC32(name);
            if (!m_assetBundleItemDict.TryGetValue(crc, out abItem) || abItem == null)
            {
                AssetBundle assetBundle = null;
                string fullPath = GlobalConfigData.AssetBundleBuildTargetPath + "/" + name;
                if (File.Exists(fullPath))
                {
                    assetBundle = AssetBundle.LoadFromFile(fullPath);
                }
                if (assetBundle == null)
                {
                    Debug.LogError("Load AssetBundle Error: " + fullPath);
                    return null;
                }
                abItem = m_AssetBundleItemPool.Spawn(true);
                abItem.assetBundle = assetBundle;
                abItem.refCount++;

                m_assetBundleItemDict.Add(crc, abItem);
            }
            else
            {
                abItem.refCount++;
            }
            return abItem.assetBundle;
        }
        //卸载AssetBundle
        private void UnloadAssetBundle(string abName)
        {
            AssetBundleItem abItem = null;
            uint crc = Crc.StringToCRC32(abName);
            if (m_assetBundleItemDict.TryGetValue(crc, out abItem) && abItem != null)
            {
                abItem.refCount--;
                if (abItem.refCount <= 0 && abItem.assetBundle != null)
                {
                    abItem.assetBundle.Unload(true);
                    abItem.Reset();
                    m_AssetBundleItemPool.Recycle(abItem);
                    m_assetBundleItemDict.Remove(crc);
                }
            }
        }
        public ResourceItem FindResourceItem(uint crc)
        {
            return m_resourceItemDict[crc];
        }
        #endregion
    }

    public class ResourceItem
    {
        //资源路径的crc
        public uint Crc = 0;
        //资源名字
        public string AssetName = string.Empty;
        //资源所在AssetBundle
        public string ABName = string.Empty;
        //资源依赖的AssetBundle
        public List<string> DependABList = null;
        //资源的加载完的AB包
        public AssetBundle Ab = null;
    }

    public class AssetBundleItem
    {
        public AssetBundle assetBundle = null;
        public int refCount; //引用计数

        public void Reset()
        {
            assetBundle = null;
            refCount = 0;
        }
    }

}

