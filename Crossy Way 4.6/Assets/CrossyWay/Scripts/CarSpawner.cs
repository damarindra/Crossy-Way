using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CarSpawner : MonoBehaviour {

	public float speedCar;
	public float minSpawnCarTime;
	public float maxSpawnCarTime;
	private float spawnCarTime;
	public GameObject car;

	public bool isFromRight;
	private bool isTimerStarted = false;

	public GameObject carBackup;
	private GameObject carInstance;
	private Vector3 carSpawnPosition;


	void Start(){
		isFromRight = Random.Range(0, 2) == 0 ? true : false;

		carSpawnPosition =new Vector3(isFromRight ? BoardManager.instance.boardWidth/2 : -(BoardManager.instance.boardWidth/2), 0, transform.position.z);
	}

	// Update is called once per frame
	void Update () {
		if(!isTimerStarted){
			spawnCarTime = Random.Range(minSpawnCarTime, maxSpawnCarTime);
			isTimerStarted = true;
		}
		else{
			spawnCarTime -= Time.deltaTime;
			if(spawnCarTime <= 0)
			{
				if(carBackup != null){
					carInstance = carBackup;
					carInstance.transform.position = carSpawnPosition;
					carInstance.SetActive(true);
					carBackup = null;
				}
				else{
					List<GameObject> tempCar = new List<GameObject>();
					PoolingSystem.instance.poolingDict.TryGetValue(car.name, out tempCar);
					if(tempCar.Count == 0)
						carInstance = Instantiate(car, carSpawnPosition, Quaternion.identity) as GameObject;
					else{
						carInstance = tempCar[0];
						tempCar.RemoveAt(0);
						PoolingSystem.instance.poolingDict[car.tag] = tempCar;
						carInstance.SetActive(true);
						carInstance.transform.position = carSpawnPosition;
					}
					carInstance.transform.SetParent(transform);
					carInstance.gameObject.GetComponent<Car>().cs = this;
					carInstance.GetComponent<Car>().Setup();
				}
				isTimerStarted = false;
			}
		}
	}


}
