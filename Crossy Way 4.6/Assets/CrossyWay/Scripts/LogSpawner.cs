using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LogSpawner : MonoBehaviour {

	public float speed;
	public float minSpawnTime;
	public float maxSpawnTime;
	private float spawnLogTime;
	public GameObject log;

	public bool isFromRight;
	private bool isTimerStarted = false;
	public GameObject logBackup = null;
	private Vector3 logSpawnPosition;

	// Use this for initialization
	void Start () {
		isFromRight = Random.Range(0, 2) == 0 ? true : false;
		logSpawnPosition = new Vector3(isFromRight ? BoardManager.instance.boardWidth/2 : -(BoardManager.instance.boardWidth/2), 0, transform.position.z);
	}
	
	// Update is called once per frame
	void Update () {
		if(!isTimerStarted){
			spawnLogTime = Random.Range(minSpawnTime, maxSpawnTime);
			isTimerStarted = true;
		}
		else{
			spawnLogTime -= Time.deltaTime;
			if(spawnLogTime <= 0)
			{
				GameObject logInstance;
				if(logBackup != null){
					logInstance = logBackup;
					logInstance.transform.position = logSpawnPosition;
					logInstance.SetActive(true);
					logBackup = null;
				}
				else{
					List<GameObject> tempLog = new List<GameObject>();
					PoolingSystem.instance.poolingDict.TryGetValue(log.name, out tempLog);
					if(tempLog.Count == 0)
						logInstance = Instantiate(log, logSpawnPosition, Quaternion.identity) as GameObject;
					else{
						logInstance = tempLog[0];
						tempLog.RemoveAt(0);
						PoolingSystem.instance.poolingDict[log.tag] = tempLog;
						logInstance.SetActive(true);
						logInstance.transform.position = logSpawnPosition;
					}
					logInstance.transform.SetParent(transform);
					logInstance.gameObject.GetComponent<Log>().ls = this;
					logInstance.GetComponent<Log>().Setup();
				}
				isTimerStarted = false;
			}
		}
	}
}
