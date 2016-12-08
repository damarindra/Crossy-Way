using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolingSystem : MonoBehaviour {

	public static PoolingSystem instance = null;

	public Dictionary<string, List<GameObject>> poolingDict = new Dictionary<string, List<GameObject>>();

	void Awake(){
		if (instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);

	}
}
