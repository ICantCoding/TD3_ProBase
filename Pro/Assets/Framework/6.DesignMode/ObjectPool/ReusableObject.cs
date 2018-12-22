using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TD3_Framework
{
    public class ReusableObject : MonoBehaviour
    {
        #region 字段和属性
        public bool IsIdle { get; set; }   //对象池中的物体是否空闲, true表示空闲, false表示正在被使用
        #endregion
    }
}
