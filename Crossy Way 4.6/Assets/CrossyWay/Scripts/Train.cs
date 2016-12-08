using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Train : MonoBehaviour {

	[HideInInspector]
	public TrainSpawner ts;

	private float speed;
	private bool isFromRight;
	private Vector3 destination;

	// Use this for initialization
	void Start () {
		if(ts == null)
			ts = GetComponentInParent<TrainSpawner>();
		isFromRight= ts.isFromRight;
		destination = ts.trainSpawnPosition;
		if(isFromRight){
			speed = -ts.speed;
			destination.x -= ts.trainWidth - BoardManager.instance.boardWidth; 
		}
		else{
			destination.x += ts.trainWidth + BoardManager.instance.boardWidth; 
			speed = ts.speed;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.parent.gameObject.activeSelf){
			transform.position = new Vector3(transform.position.x + (speed *Time.deltaTime), transform.position.y, transform.position.z);

			//transform.position = Vector3.MoveTowards(transform.position, destination, speed);
			if((transform.position.x + ts.trainWidth < -(BoardManager.instance.boardWidth/2) && isFromRight) || (transform.position.x - ts.trainWidth> BoardManager.instance.boardWidth/2 && !isFromRight)){
				//Destroy(transform.parent.gameObject);
				transform.gameObject.SetActive(false);
				ts.light.enabled = false;
				ts.isTimerStarted = false;
			}
			if(!transform.parent.gameObject.activeSelf && gameObject.activeSelf)
				AssignToPoolingSystem();
		}
	}
	void AssignToPoolingSystem(){
		List<GameObject> updateValue = new List<GameObject>();
		PoolingSystem.instance.poolingDict.TryGetValue(tag, out updateValue);
		updateValue.Add(gameObject);
		gameObject.SetActive(false);
		PoolingSystem.instance.poolingDict[tag] = updateValue;
	}
}
