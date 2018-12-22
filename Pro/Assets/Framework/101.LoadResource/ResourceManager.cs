/*
    游戏资源管理器
*/

namespace TD3_Framework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ResourceManager : Singleton<ResourceManager>
    {
        
    }

    //双向链表节点
    public class DoubleLinkedListNode<T> where T : class, new()
    {
        //前一个节点
        public DoubleLinkedListNode<T> prev = null;
        //后一个节点
        public DoubleLinkedListNode<T> next = null;
        //当前节点
        public T t = null;
    }

    //双向链表结构
    public class DoubleLinkedList<T> where T : class, new()
    {
        //表头
        public DoubleLinkedListNode<T> Head = null;
        //表尾
        public DoubleLinkedListNode<T> Tail = null;
        //双向连接结构类对象池
        protected ClassObjectPool<DoubleLinkedListNode<T>> m_doubleLinkedListNodePool = 
            ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkedListNode<T>>(500);
        //节点个数
        protected int m_Count = 0;
        public int Count
        {
            get { return m_Count; }
        }

        /// <summary>
        /// 添加一个节点到头部
        /// </summary>
        public DoubleLinkedListNode<T> AddToHeader(T t)
        {
            DoubleLinkedListNode<T> node = m_doubleLinkedListNodePool.Spawn(true);
            node.next = null;
            node.prev = null;
            node.t = t;
            return AddToHeader(node);
        }
        /// <summary>
        /// 添加一个节点到头部
        /// </summary>
        public DoubleLinkedListNode<T> AddToHeader(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null) return null;

            pNode.prev = null;

            if (Head == null)
            {
                Head = Tail = pNode;
            }
            else
            {
                pNode.next = Head;
                Head.prev = pNode.next;
                Head = pNode;
            }
            m_Count++;
            return pNode;
        }
        /// <summary>
        /// 添加节点到尾部
        /// </summary>
        public DoubleLinkedListNode<T> AddToTail(T t)
        {
            DoubleLinkedListNode<T> node = m_doubleLinkedListNodePool.Spawn(true);
            node.next = null;
            node.prev = null;
            node.t = t;
            return AddToTail(node);
        }
        /// <summary>
        /// 添加节点到尾部
        /// </summary>
        public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null) return null;

            pNode.next = null;

            if (Tail == null)
            {
                Head = Tail = pNode;
            }
            else
            {
                pNode.prev = Tail;
                Tail.next = pNode;
                Tail = pNode;
            }
            m_Count++;
            return pNode;
        }
        /// <summary>
        /// 移除某个节点
        /// </summary>
        public void RemoveNode(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null) return;
            if (pNode == Head)
            {
                Head = pNode.next;
            }
            if (pNode == Tail)
            {
                Tail = pNode.prev;
            }
            if (pNode.prev != null)
            {
                pNode.prev.next = pNode.next;
            }
            if (pNode.next != null)
            {
                pNode.next.prev = pNode.prev;
            }
            pNode.prev = pNode.next = null;
            pNode.t = null;
            m_doubleLinkedListNodePool.Recycle(pNode);
            m_Count--;
        }
        /// <summary>
        /// 把某个链表中的节点移动到头部
        /// </summary>
        /// <param name="pNode">P node.</param>
        public void MoveToHead(DoubleLinkedListNode<T> pNode)
        {
            if (pNode == null) return;
            if (pNode == Head) return;
            if (pNode.prev == null && pNode.next == null) return;

            if (pNode == Tail)
            {
                Tail = pNode.prev;
            }
            if (pNode.prev != null)
            {
                pNode.prev.next = pNode.next;
            }
            if (pNode.next != null)
            {
                pNode.next.prev = pNode.prev;
            }
            pNode.prev = null;
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;

            if (Tail == null)
            {
                Tail = Head;
            }
        }
    }

    public class CMapList<T> where T : class, new()
    {
        private DoubleLinkedList<T> m_doubleLinkedList = new DoubleLinkedList<T>();
        private Dictionary<T, DoubleLinkedListNode<T>> m_doubleLinkedNodeDict = new Dictionary<T, DoubleLinkedListNode<T>>(); //用于缓存

        ~CMapList()
        {
            Clear();
        }

        //清空列表
        public void Clear()
        {
            while (m_doubleLinkedList.Tail != null)
            {
                m_doubleLinkedList.RemoveNode(m_doubleLinkedList.Tail);
                //m_doubleLinkedNodeDict.Remove(m_doubleLinkedList.Tail.t);
            }
        }
        //插入
        public void Insert(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (m_doubleLinkedNodeDict.TryGetValue(t, out node) && node != null)
            {
                m_doubleLinkedList.AddToHeader(node);
            }
            else
            {
                m_doubleLinkedList.AddToHeader(t);
                m_doubleLinkedNodeDict.Add(t, m_doubleLinkedList.Head);
            }
        }
        //删除某个节点
        public void Remove(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (m_doubleLinkedNodeDict.TryGetValue(t, out node) && node != null)
            {
                m_doubleLinkedList.RemoveNode(node);
                m_doubleLinkedNodeDict.Remove(t);
            }
        }
        //从表尾部删除一个节点
        public void Pop()
        {
            if (m_doubleLinkedList.Tail != null)
            {
                Remove(m_doubleLinkedList.Tail.t);
            }
        }
        //获取尾部节点
        public T Back()
        {
            return m_doubleLinkedList.Tail == null ? null : m_doubleLinkedList.Tail.t;
        }
        //返回节点个数
        public int Size()
        {
            return m_doubleLinkedList.Count;
        }
        //查找是否有某个节点
        public bool Find(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (m_doubleLinkedNodeDict.TryGetValue(t, out node) && node != null)
            {
                return true;
            }
            return false;
        }
        //刷新某个节点，把节点移动到头部
        public bool Refresh(T t)
        {
            DoubleLinkedListNode<T> node = null;
            if (m_doubleLinkedNodeDict.TryGetValue(t, out node) && node != null)
            {
                m_doubleLinkedList.MoveToHead(node);
                return true;
            }
            return false;
        }
    }

}





