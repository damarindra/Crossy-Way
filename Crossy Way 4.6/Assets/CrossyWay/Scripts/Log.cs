using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Log : MonoBehaviour {

	[HideInInspector]
	public LogSpawner ls;
	
	private float speed;
	private bool isFromRight;
	private float logWidth;

	// Use this for initialization
	public void Setup () {
		
		isFromRight = ls.isFromRight;
		if(isFromRight){
			speed = -ls.speed;
			transform.Rotate(new Vector3(transform.rotation.x, 180, transform.rotation.z));
		}
		else
			speed = ls.speed;
		logWidth = GetComponentInChildren<MeshRenderer>().bounds.size.x;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position  = new Vector3(transform.position.x + (speed  * Time.deltaTime), transform.position.y, transform.position.z);
		if(transform.position.x < -(BoardManager.instance.boardWidth/2 + logWidth/2) || transform.position.x > BoardManager.instance.boardWidth/2 + logWidth/2)
		{
			gameObject.SetActive(false);
			ls.logBackup = gameObject;
		}
		if(!transform.parent.gameObject.activeSelf && gameObject.activeSelf)
			AssignToPoolingSystem();
	}
	
	void AssignToPoolingSystem(){
		List<GameObject> updateValue = new List<GameObject>();
		PoolingSystem.instance.poolingDict.TryGetValue(tag, out updateValue);
		updateValue.Add(gameObject);
		gameObject.SetActive(false);
		PoolingSystem.instance.poolingDict[tag] = updateValue;
	}
}
