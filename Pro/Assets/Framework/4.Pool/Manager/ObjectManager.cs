

namespace TD3_Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ObjectManager : Singleton<ObjectManager>
    {
        #region 数据字段和属性
        protected Dictionary<Type, object> m_classObjectPoolDict = new Dictionary<Type, object>();
        #endregion

        /// <summary>
        /// 创建类对象池, 然后就可以在该对象池中Spawn和Recycle了
        /// </summary>
        public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxCount)
            where T : class, new()
        {
            Type type = typeof(T);
            object obj = null;
            if (!m_classObjectPoolDict.TryGetValue(type, out obj) || obj == null)
            {
                ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
                m_classObjectPoolDict.Add(type, newPool);
                return newPool;
            }
            return obj as ClassObjectPool<T>;
        }
    }
}
