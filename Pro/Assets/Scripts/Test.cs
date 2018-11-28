using System.Collections;
using System.Collections.Generic;
using TD3_Framework;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {


        TD3_Framework.ObjectPool.Instance.OnSpawn(PoolInfo.SubPool1Name);
	}
	
	
}
