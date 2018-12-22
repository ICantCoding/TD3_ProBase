﻿
namespace TD3_Framework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ClassObjectPool<T>
        where T : class, new()
    {
        //池
        protected Stack<T> m_pool = new Stack<T>();
        //池中最大类对象个数， <=0 表示不限制对象个数
        protected int m_maxCount = 0;
        //没有回收的类对象个数
        protected int m_noRecycleCount = 0;

        public ClassObjectPool(int maxCount)
        {
            m_maxCount = maxCount;
            for (int i = 0; i < m_maxCount; i++)
            {
                m_pool.Push(new T());
            }
        }

        /// <summary>
        /// 从池中取出一个类对象
        /// </summary>
        /// <param name="isCreate">isCreate表示池中没有类对象了，是否主动创建, 主动创建的类对象不会被Pool管理</param>
        public T Spawn(bool isCreate)
        {
            if (m_pool.Count > 0)
            {
                T rtn = m_pool.Pop();
                if (rtn == null && isCreate == true)
                {
                    rtn = new T();
                }
                m_noRecycleCount++;
                return rtn;
            }
            else
            {
                if (isCreate == true)
                {
                    T rtn = new T();
                    m_noRecycleCount++;
                    return rtn;
                }
            }
            return null;
        }

        /// <summary>
        /// 从池中回收
        /// </summary>
        /// <returns>回收是否成功</returns>
        /// <param name="obj">回收类对象</param>
        public bool Recycle(T obj)
        {
            if (obj == null) return false;

            m_noRecycleCount--;

            if (m_pool.Count >= m_maxCount && m_maxCount > 0)
            {
                obj = null;
                return true;
            }
            else
            {
                m_pool.Push(obj);
            }
            return true;
        }
    }

}


