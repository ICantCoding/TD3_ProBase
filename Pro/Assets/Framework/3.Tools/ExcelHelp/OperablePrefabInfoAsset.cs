using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OperablePrefabInfo
{
    public int objectId;                //设备ID
    public int objectType;              //设备类型
    public string objectDescription;    //设备类型描述
    public Vector3 objectPosition;      //设备世界坐标位置
    public Vector3 objectEulerAngle;	//设备世界旋转角度

    public override string ToString(){
        return objectId.ToString() + ", " + objectType.ToString() + ", " + objectDescription.ToString();
    }
}

public class OperablePrefabInfoAsset : ScriptableObject
{
	public List<OperablePrefabInfo> operablePrefabInfoList = new List<OperablePrefabInfo>();
}
