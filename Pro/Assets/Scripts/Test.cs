using System.Collections;
using System.Collections.Generic;
using TD3_Framework;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
        TD3_Framework.ResMgr.Instance.InitAssetBundle("Common", null, null);

	    GameObject go = GameObject.Instantiate(ResMgr.Instance.GetPrefab("Common", "Cube", "cube"));
	    go.transform.localPosition = Vector3.zero;
	}
	
	
}
