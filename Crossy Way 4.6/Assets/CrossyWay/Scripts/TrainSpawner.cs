using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class TrainSpawner : MonoBehaviour {

	public float speed;
	public int longTrain;

	public GameObject realTrainPrefab;

	public float trainWidth;

	public float spawnTrainTime;
	private float spawnTrainTimeBackup;

	public bool isFromRight;
	public bool isTimerStarted = false;

	private bool setupCompleted;

	public float warningTime;
	public Light light;

	private GameObject trainInstance;
	public Vector3 trainSpawnPosition;

	// Use this for initialization
	void Start () {
		setupCompleted = false;
		isFromRight = UnityEngine.Random.Range(0,2) == 0 ? true : false;
		
		longTrain = UnityEngine.Random.Range(BoardManager.instance.minLongTrain, BoardManager.instance.maxLongTrain + 1);

		//realTrainPrefab = prefabTrain;
		//Destroy(realTrainInstance);

		CreateTrain(out realTrainPrefab);

		//if(isFromRight)
		//	realTrainInstance.transform.GetChild(0).Rotate(new Vector3(0,180,0));

		spawnTrainTimeBackup = spawnTrainTime;

		trainSpawnPosition= new Vector3(isFromRight ? BoardManager.instance.boardWidth/2 : -(BoardManager.instance.boardWidth/2), 0, transform.position.z);
		List<GameObject> tempTrain = new List<GameObject>();
		PoolingSystem.instance.poolingDict.TryGetValue(realTrainPrefab.name, out tempTrain);
		if(tempTrain.Count == 0){
			trainInstance = Instantiate(realTrainPrefab, trainSpawnPosition, Quaternion.identity) as GameObject;
			trainInstance.AddComponent<Train>();
		}
		else{
			trainInstance = tempTrain[0];
			tempTrain.RemoveAt(0);
			PoolingSystem.instance.poolingDict[realTrainPrefab.tag] = tempTrain;
			trainInstance.transform.eulerAngles = Vector3.zero;
			trainInstance.transform.position = trainSpawnPosition;
		}
		if(trainInstance.GetComponent<Train>() != null)
			trainInstance.GetComponent<Train>().ts = this;
		else{
			trainInstance.AddComponent<Train>();
			trainInstance.GetComponent<Train>().ts = this;
		}
		if(isFromRight)
			trainInstance.transform.Rotate(new Vector3(transform.rotation.x, 180, transform.rotation.z));
		if(trainWidth == 0)
			trainWidth = trainInstance.transform.GetComponent<MeshRenderer>().bounds.size.x;
		trainInstance.transform.SetParent(transform);
		
		trainInstance.SetActive(false);
	}

	void CreateTrain(out GameObject prefabTrain){
		//BoardManager.instance.trainReady.TryGetValue(longTrain, out prefabTrain);
		prefabTrain = BoardManager.instance.train[longTrain - BoardManager.instance.minLongTrain];
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
		if(BoardManager.instance.boardCreationMode)
			return;
#endif
		if(!isTimerStarted && !light.enabled){
			isTimerStarted = true;
			spawnTrainTime = spawnTrainTimeBackup;
		}
		else if(spawnTrainTime > warningTime){
			spawnTrainTime -= Time.deltaTime;
			//Debug.Log(spawnTrainTime);
			if(spawnTrainTime <= warningTime){
				light.enabled = true;
				trainInstance.transform.position = trainSpawnPosition;
			}
		}
		else //if(transform.GetChild(transform.childCount-1).name.Substring(0,9) != realTrainPrefab.name.Substring(0,9))
		{
			spawnTrainTime -= Time.deltaTime;
			
			if(spawnTrainTime <= 0)
			{
				if(!trainInstance.activeSelf){
					isTimerStarted = false;
					trainInstance.SetActive(true);
					//trainInstance.SetActive(true);
				}
			}
		}
	}
}
