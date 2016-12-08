using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Car : MonoBehaviour {

	[HideInInspector]
	public CarSpawner cs;

	private float speed;
	private bool isFromRight;

	// Use this for initialization
	public void Setup () {
		isFromRight = cs.isFromRight;
		if(isFromRight){
			speed = -cs.speedCar;
			transform.eulerAngles = (new Vector3(transform.rotation.x, 180, transform.rotation.z));
		}
		else{
			speed = cs.speedCar;
			transform.eulerAngles = Vector3.zero;
		}
	}
	
	// Update is called once per frame
	void Update () {
		transform.position  = new Vector3(transform.position.x + (speed  * Time.deltaTime), transform.position.y, transform.position.z);
		if(transform.position.x < -(BoardManager.instance.boardWidth/2) || transform.position.x > BoardManager.instance.boardWidth/2){
			gameObject.SetActive(false);
			cs.carBackup = gameObject;
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
