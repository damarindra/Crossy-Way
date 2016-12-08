using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoardManager : MonoBehaviour {
	public static BoardManager instance = null;
	
#if UNITY_EDITOR
	[Header("Unity Editor Only (More Info Point Cursor to Variable Name)")]

	[Tooltip("If True, Press 'N' to destroy and create new Row")]
	public bool pressNForNewLine;
	[Tooltip("If True, will create LandPack, RiverPack, RailPack and Train Pack. Hit Play and All assets available at Prefabs/Packed/ ... . Make sure the folder exist")]
	public bool boardCreationMode;
	public enum CreationType{
		GRASS, ROAD, RAIL, RIVER, TRAIN, ALL
	}
	public CreationType creationType;
	public string pathFolder = "Assets/CrossyWay/Prefabs/Packed";
	private bool hasCompleteSetupBoard;
#endif
#if SOFT_PROCESS
	[Header("Ingredients")]
	public GameObject grass;
	public GameObject deepGrass;
	public GameObject road;
	public GameObject roadTopLine, roadBothLine, roadBottomLine, deepRoad;
	public GameObject river;
	public GameObject deepRiver;
	public GameObject rail;
	public GameObject deepRail;
	public GameObject carriage, headTrain;
	[Tooltip("Head Exclusive")]
	public int minLongTrain;
	public int maxLongTrain;
#endif

	[Header("Size")]
	[Tooltip("Width Board, Must higher than boardWidthEffective")]
	public int boardWidth;
	[Tooltip("Width character can move")]
	public int boardWidthEffective;
	[Tooltip("How many row can be spawn per Board, must above 14")]
	public int longPerBoard;
	[HideInInspector]
	public int xMinBoard, xMaxBoard, xMinEffectiveBoard, xMaxEffectiveBoard;
	private int yPos, rows;	//rows using for how many row will spawn each platform (grass/road/river) type

	[Header("Land")]
#if SOFT_PROCESS
	public GameObject grassPacked;
#else
	public GameObject grass;
	public GameObject deepGrass;
#endif
	[Range(0,1)]
	public float grassChance;
	[Tooltip("Must more than 0, not 0")]
	public int minLongGrass;
	public int maxLongGrass;
	public GameObject[] tree;
	[Tooltip("Prefer below half of boardWidthEffective")]
	public int maxTreePerLine;
	private int treeCounter;
	private List<int> xPosTree = new List<int>();
	private int xSafeLand;
	
	[Header("Road")]
#if SOFT_PROCESS
	public GameObject roadPacked;
	public GameObject roadTopLinePacked, roadBothLinePacked, roadBottomLinePacked;
#else
	public GameObject road;
	public GameObject roadTopLine, roadBothLine, roadBottomLine, deepRoad;
#endif
	[Range(0,1)]
	public float roadChance;
	[Tooltip("Must more than 0, not 0")]
	public int minLongRoad;
	public int maxLongRoad;
	public GameObject[] car;
	public float minSpeedCar;
	public float maxSpeedCar;
	public float minSpawnCarTime;
	public float maxSpawnCarTime;
	private enum RoadType{
		NOPE, SINGLE, TOP, MIDDLE, BOTTOM
	}
	private RoadType roadType = RoadType.NOPE;

	[Header("River")]
#if SOFT_PROCESS
	public GameObject riverPacked;
#else
	public GameObject river;
	public GameObject deepRiver;
#endif
	[Range(0,1)]
	public float riverChance;
	[Tooltip("Must more than 0, not 0")]
	public int minLongRiver;
	public int maxLongRiver;
	public GameObject[] log;
	public float minSpeedLog;
	public float maxSpeedLog;
	[Tooltip("Handling if speed between 2 river is very close, need to balancing, at least .5")]
	public float speedLogBalancer;
	public float minLogSpawnTime;
	public float maxLogSpawnTime;
	private float lastSpeedLog;

	[Header("Rail")]
#if SOFT_PROCESS
	public GameObject railPacked;
#else
	public GameObject rail;
	public GameObject deepRail;
#endif
	[Range(0,1)]
	public float railChance;
	[Tooltip("Must more than 0, not 0")]
	public int minLongRail;
	public int maxLongRail;
#if SOFT_PROCESS
	public GameObject[] train;
#else
	public GameObject[] train;
	public GameObject carriage, headTrain;
	[Tooltip("Head Exclusive")]
	public int minLongTrain;
	public int maxLongTrain;
#endif
	public float trainSpeed;
	public float minTrainSpawnTime;
	public float maxTrainSpawnTime;
	[Tooltip("Warning Time Before Train Spawn")]
	public float trainWarningTime;
	public GameObject lampRail;
	private int xLampPos;

	[Header("Coin")]
	public GameObject coin;
	[Range(0,1)]
	public float coinChance;

	public enum BoardState{
		INITIALIZE, DONE, GRASS, ROAD, RIVER, RAIL
	}
	[Header("State (Don't Edit, Read Only)")]
	public BoardState boardState;
	private BoardState boardStateBefore;

	[HideInInspector]
	public static GameObject characterInstance = null;

	private Transform boardParent;
	private List<GameObject> rowBoardPooling = new List<GameObject>();

	void RegisterToPoolingSystem(){
#if SOFT_PROCESS
		PoolingSystem.instance.poolingDict.Add(grassPacked.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(riverPacked.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(railPacked.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(roadPacked.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(roadBothLinePacked.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(roadBottomLinePacked.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(roadTopLinePacked.tag, new List<GameObject>());

#else
		PoolingSystem.instance.poolingDict.Add(grass.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(deepGrass.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(river.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(deepRiver.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(rail.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(deepRail.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(road.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(roadBothLine.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(roadBottomLine.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(roadTopLine.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(deepRoad.tag, new List<GameObject>());
#endif
		PoolingSystem.instance.poolingDict.Add(tree[0].tag, new List<GameObject>());	//Tree Dkk

		RegisterToPoolingSystemTypeArray(car);
		RegisterToPoolingSystemTypeArray(log);
		RegisterToPoolingSystemTypeArray(train);
		PoolingSystem.instance.poolingDict.Add(lampRail.tag, new List<GameObject>());
		PoolingSystem.instance.poolingDict.Add(coin.tag, new List<GameObject>());
	}

	void RegisterToPoolingSystemTypeArray(GameObject[] objs){
		foreach(GameObject go in objs){
			if(go.tag.Equals("Car") || go.tag.Equals("Log") || go.tag.Equals("Train"))
				PoolingSystem.instance.poolingDict.Add(go.name, new List<GameObject>());
			else
				PoolingSystem.instance.poolingDict.Add(go.tag, new List<GameObject>());
				
		}
	}

	void SpawnPlayer(){
		if(characterInstance == null)
			characterInstance = CharacterShop.instance.StringToCSGameObject(SaveLoad.characterRecentUse);
		characterInstance = Instantiate(characterInstance, Vector3.zero, Quaternion.identity) as GameObject;
		characterInstance.AddComponent<PlayerController>();
	}

	void InitializeFirstBoard(){
		SpawnPlayer();
		boardParent = new GameObject("BoardParent").transform;
		xMinBoard = (boardWidth/2) * -1;
		xMaxBoard = boardWidth/2;
		xMinEffectiveBoard = (boardWidthEffective/2) * -1;
		xMaxEffectiveBoard = boardWidthEffective/2;
		//Spawn decoration before boardEffective, -4 all decoration (tree, rock, etc)
		yPos = -7;
		do{
			Transform rowInstance = new GameObject("Row " + yPos.ToString()).transform;
			rowInstance.position = new Vector3(0, 0, yPos);
			rowInstance.SetParent(boardParent);
			int xPos = xMinBoard;
			while(xPos < xMaxBoard){
				if(yPos < -3){
					GameObject treeInstance = Instantiate(tree[UnityEngine.Random.Range(0, tree.Length)], new Vector3(xPos, 0, yPos), Quaternion.identity) as GameObject;
#if !SOFT_PROCESS
					GameObject grassLandInstance = Instantiate(grass, new Vector3(xPos, 0, yPos), Quaternion.identity) as GameObject;
					grassLandInstance.transform.SetParent(rowInstance);
#endif
					treeInstance.transform.SetParent(rowInstance);
				}
#if !SOFT_PROCESS
				else{
					SpawnGrass(xPos, rowInstance);
				}
#endif
				xPos++;
			}
#if SOFT_PROCESS
			SpawnOrMoveObject(rowInstance, grassPacked);
#endif
			treeCounter=0;
			xPosTree.Clear();
			if(yPos > 0)
				RandomTree();
			rowBoardPooling.Add(rowInstance.gameObject);
			yPos++;
		}while(yPos < 6);
		boardState = BoardState.DONE;
	}
	void SpawnBoardPerType(){
		if(rowBoardPooling.Count < longPerBoard){
			if(boardState == BoardState.DONE){
				//Random whats next? Grass/Road/River
				RandomNextBoardType();

				lastSpeedLog = 0;
			}
			Transform rowInstance = new GameObject("Row " + yPos.ToString()).transform;
			rowInstance.position = new Vector3(0, 0, yPos);
			rowInstance.SetParent(boardParent);

			if(boardState == BoardState.GRASS){
				if(rows <= 0){
					rows = UnityEngine.Random.Range(minLongGrass, maxLongGrass + 1);
					xSafeLand = UnityEngine.Random.Range(xMinEffectiveBoard, xMaxEffectiveBoard+1);
				}
				RandomTree();
				
				int xPos = xMinBoard;
				while(xPos < xMaxBoard){
					//GetFromPool

					//Spawning
					SpawnGrass(xPos, rowInstance);
					xPos++;
				}
#if SOFT_PROCESS
				SpawnOrMoveObject(rowInstance, grassPacked);
#endif
			}
			else if(boardState == BoardState.ROAD){
				if(rows <= 0){
					rows = UnityEngine.Random.Range(minLongRoad, maxLongRoad + 1);
					if(rows == 1)
						roadType = RoadType.SINGLE;
					else{
						roadType = RoadType.TOP;
					}
				}
#if SOFT_PROCESS
				if(roadType == RoadType.SINGLE){
					SpawnOrMoveObject(rowInstance, roadPacked);
				}
				else if(roadType == RoadType.TOP){
					SpawnOrMoveObject(rowInstance, roadTopLinePacked);

				}
				else if(roadType == RoadType.MIDDLE)
				{
					SpawnOrMoveObject(rowInstance, roadBothLinePacked);

				}	
				else if(roadType == RoadType.BOTTOM)
				{
					SpawnOrMoveObject(rowInstance, roadBottomLinePacked);

				}	
#else
				int xPos = xMinBoard;
				while(xPos < xMaxBoard){
					SpawnRoad(xPos, rowInstance);
					xPos++;
				}
#endif
				CarSpawner cs = rowInstance.gameObject.AddComponent<CarSpawner>();
				cs.speedCar = UnityEngine.Random.Range(minSpeedCar, maxSpeedCar);
				cs.minSpawnCarTime = minSpawnCarTime;
				cs.maxSpawnCarTime = maxSpawnCarTime;
				cs.car = car[UnityEngine.Random.Range(0, car.Length)];

				if(rows == 2)
					roadType = RoadType.BOTTOM;
				else if(roadType == RoadType.TOP)
					roadType = RoadType.MIDDLE;
			}
			else if(boardState == BoardState.RIVER){
				if(rows <= 0)
					rows = UnityEngine.Random.Range(minLongRiver, maxLongRiver + 1);
#if !SOFT_PROCESS
				int xPos = xMinBoard;
				while(xPos < xMaxBoard){
					SpawnRiver(xPos, rowInstance);
					xPos++;
				}
#elif SOFT_PROCESS
				SpawnOrMoveObject(rowInstance, riverPacked);
#endif
				LogSpawner ls = rowInstance.gameObject.AddComponent<LogSpawner>();
				//do{
				ls.speed = UnityEngine.Random.Range(minSpeedLog, maxSpeedLog);
				//}while(ls.speed - speedLogBalancer <= lastSpeedLog && ls.speed + speedLogBalancer >= lastSpeedLog);
				if(ls.speed <= lastSpeedLog + speedLogBalancer/2 && ls.speed >= lastSpeedLog){
					if(ls.speed - speedLogBalancer <= .8f){
						ls.speed += speedLogBalancer*1.5f;
					}
					else
						ls.speed -= speedLogBalancer;
				}
				else if(ls.speed >= lastSpeedLog - speedLogBalancer/2 && ls.speed <= lastSpeedLog){
					ls.speed += speedLogBalancer;
				}
				ls.minSpawnTime = minLogSpawnTime;
				ls.maxSpawnTime = maxLogSpawnTime;
				ls.log = log[UnityEngine.Random.Range(0, log.Length)];
				lastSpeedLog = ls.speed;
			}
			else if(boardState == BoardState.RAIL){
				if(rows <= 0){
					rows = UnityEngine.Random.Range(minLongRail, maxLongRail + 1);
					xLampPos = UnityEngine.Random.Range(xMinEffectiveBoard + 1, xMaxEffectiveBoard);
				}

#if !SOFT_PROCESS
				int xPos = xMinBoard;
				while(xPos < xMaxBoard){
					SpawnRail(xPos, rowInstance);
					xPos++;
				}
#elif SOFT_PROCESS
				SpawnOrMoveObject(rowInstance, railPacked);
				SpawnRail(xLampPos, rowInstance);
#endif
				TrainSpawner ts = rowInstance.gameObject.AddComponent<TrainSpawner>();
				ts.speed = trainSpeed;
				ts.spawnTrainTime = UnityEngine.Random.Range(minTrainSpawnTime, maxTrainSpawnTime);
				ts.warningTime = trainWarningTime;
				ts.light = rowInstance.GetComponentInChildren<Light>();
			}

			rows--;
			
			if(rows <= 0){
				boardState = BoardState.DONE;
			}

			treeCounter=0;
			xPosTree.Clear();

			rowBoardPooling.Add(rowInstance.gameObject);

			yPos+=1;
		}
	}
	void SpawnOrMoveObject(int x, int y, Transform rowInstance, GameObject obj){
		GameObject goIns;
		if(PoolingSystem.instance.poolingDict.ContainsKey(obj.tag)){
			List<GameObject> listObj = new List<GameObject>();
			PoolingSystem.instance.poolingDict.TryGetValue(obj.tag, out listObj);
			if(listObj.Count > 0){
				goIns = listObj[0];
				goIns.SetActive(true);
				listObj.RemoveAt(0);
				goIns.transform.position = new Vector3(x, 0, y);
			}
			else
				goIns = Instantiate(obj, new Vector3(x, 0, y), Quaternion.identity) as GameObject;
		}
		else{
			goIns = Instantiate(obj, new Vector3(x, 0, y), Quaternion.identity) as GameObject;
		}
		goIns.transform.SetParent(rowInstance);
	}
	void SpawnOrMoveObject(int x, Transform rowInstance, GameObject obj){
		SpawnOrMoveObject(x, yPos, rowInstance, obj);
	}
	void SpawnOrMoveObject(Transform rowInstance, GameObject obj){
		SpawnOrMoveObject(0, rowInstance, obj);
	}
	void SpawnOrMoveObject(int x, int y, Transform rowInstance, GameObject[] objs){
		GameObject goIns;
		if(PoolingSystem.instance.poolingDict.ContainsKey(objs[0].tag)){
			List<GameObject> listObj = new List<GameObject>();
			PoolingSystem.instance.poolingDict.TryGetValue(objs[0].tag, out listObj);
			if(listObj.Count > 0){
				goIns = listObj[0];
				goIns.SetActive(true);
				listObj.RemoveAt(0);
				goIns.transform.position = new Vector3(x, 0, y);
			}
			else
				goIns = Instantiate(objs[UnityEngine.Random.Range(0, objs.Length)], new Vector3(x, 0, y), Quaternion.identity) as GameObject;
			
		}
		else{
			goIns = Instantiate(objs[UnityEngine.Random.Range(0, objs.Length)], new Vector3(x, 0, y), Quaternion.identity) as GameObject;
		}
		goIns.transform.SetParent(rowInstance);
	}
	void SpawnOrMoveObject(int x, Transform rowInstance, GameObject[] objs){
		SpawnOrMoveObject(x, yPos, rowInstance, objs);
	}

	void SpawnGrass(int x, Transform rowInstance){
		//Spawn Deep Grass and Tree
#if !SOFT_PROCESS
		if(x < xMinEffectiveBoard || x > xMaxEffectiveBoard){
			SpawnOrMoveObject(x, rowInstance, tree);
			SpawnOrMoveObject(x, rowInstance, grass);
		}
		else{
			SpawnOrMoveObject(x, rowInstance, grass);
#endif
			int i = 0;
			foreach(int a in xPosTree.ToArray()){
				if(a == xSafeLand){
					break;
				}
				else if(x==a){
					SpawnOrMoveObject(x, rowInstance, tree);
					xPosTree.RemoveAt(i);
					goto end;
				}
				i++;
			}
			SpawnCoin(x, rowInstance);
		end:
			x=x;
			
#if !SOFT_PROCESS
		}
#endif
	}
	void SpawnRoad(int x, Transform rowInstance){
#if !SOFT_PROCESS
		if(x < xMinEffectiveBoard || x > xMaxEffectiveBoard){
			SpawnOrMoveObject(x, rowInstance, deepRoad);
		}
		else{
#endif
			if(roadType == RoadType.SINGLE){
				SpawnOrMoveObject(x, rowInstance, road);
				SpawnCoin(x, rowInstance);
			}
			else{
#if !SOFT_PROCESS
				if(x % 2 == 0){
#elif SOFT_PROCESS
					if(roadType == RoadType.TOP)
						SpawnOrMoveObject(x, rowInstance, roadTopLinePacked);
					else if(roadType == RoadType.MIDDLE)
						SpawnOrMoveObject(x, rowInstance, roadBothLinePacked);
					else if(roadType == RoadType.BOTTOM)
						SpawnOrMoveObject(x, rowInstance, roadBottomLinePacked);
#else
					if(roadType == RoadType.TOP)
						SpawnOrMoveObject(x, rowInstance, roadTopLine);
					else if(roadType == RoadType.MIDDLE)
						SpawnOrMoveObject(x, rowInstance, roadBothLine);
					else if(roadType == RoadType.BOTTOM)
						SpawnOrMoveObject(x, rowInstance, roadBottomLine);
#endif
					SpawnCoin(x, rowInstance);
#if !SOFT_PROCESS
				}
				else{
					SpawnOrMoveObject(x, rowInstance, road);
					SpawnCoin(x, rowInstance);
				}
#endif
			}
#if !SOFT_PROCESS
		}
#endif
	}
	void SpawnRiver(int x, Transform rowInstance){
		if(x < xMinEffectiveBoard || x > xMaxEffectiveBoard){
			SpawnOrMoveObject(x, rowInstance, deepRiver);
		}
		else{
			SpawnOrMoveObject(x, rowInstance, river);

		}
	}

	void SpawnRail(int x, Transform rowInstance){
#if !SOFT_PROCESS
		if(x < xMinEffectiveBoard || x > xMaxEffectiveBoard){
			SpawnOrMoveObject(x, rowInstance, deepRail);
		}
		else{
			SpawnOrMoveObject(x, rowInstance, rail);
			SpawnCoin(x, rowInstance);
#endif
			if(xLampPos == x)
			{
				SpawnOrMoveObject(x, rowInstance, lampRail);
			}
#if !SOFT_PROCESS
		}
#endif
	}

	void SpawnCoin(int x, Transform rowInstance){
		if(x >= xMinEffectiveBoard && x <= xMaxEffectiveBoard){
			float rand = UnityEngine.Random.Range(0.0f, 1.0f);
			if(rand <= coinChance)
				SpawnOrMoveObject(x, rowInstance, coin);
		}
	}
	void RandomTree(){
		treeCounter = UnityEngine.Random.Range(0, maxTreePerLine+1);
		List<int> treePosProbability = new List<int>();
		for(int i = xMinEffectiveBoard; i <= xMaxEffectiveBoard; i++)
		{
			if(i != xSafeLand)
				treePosProbability.Add(i);
		}
		while(treeCounter > 0){
			xPosTree.Add(treePosProbability[UnityEngine.Random.Range(0, treePosProbability.Count)]);
			treePosProbability.Remove(xPosTree[xPosTree.Count-1]);
			treeCounter-=1;
		}
	}
	void RandomNextBoardType(){
		if(boardStateBefore == BoardState.GRASS){
			float rand = UnityEngine.Random.Range(0.0f, roadChance+riverChance+railChance);
			boardState = rand < roadChance ? BoardState.ROAD :
				rand < roadChance+riverChance ? BoardState.RIVER : 
					rand < railChance+roadChance+riverChance ? BoardState.RAIL : BoardState.GRASS;
		}
		else if(boardStateBefore == BoardState.ROAD){
			float rand = UnityEngine.Random.Range(0.0f, grassChance+riverChance+railChance);
			boardState = rand <= grassChance ? BoardState.GRASS :
				rand <= grassChance+riverChance ? BoardState.RIVER : 
					rand <= grassChance+riverChance+railChance ? BoardState.RAIL : BoardState.ROAD;
		}
		else if(boardStateBefore == BoardState.RIVER){
			float rand = UnityEngine.Random.Range(0.0f, grassChance+roadChance+railChance);
			boardState = rand < grassChance ? BoardState.GRASS :
				rand < grassChance+roadChance ? BoardState.ROAD : 
					rand < grassChance+roadChance+railChance ? BoardState.RAIL : BoardState.RIVER;
		}
		else if(boardStateBefore == BoardState.RAIL){
			float rand = UnityEngine.Random.Range(0.0f, grassChance+roadChance+riverChance);
			boardState = rand < grassChance ? BoardState.GRASS :
				rand < grassChance+roadChance ? BoardState.ROAD : 
					rand < grassChance+roadChance+riverChance ? BoardState.RIVER : BoardState.RAIL;
		}
		else{
			float rand = UnityEngine.Random.Range(0.0f, grassChance+roadChance+riverChance+railChance);
			boardState = rand < grassChance ? BoardState.GRASS :
				rand < grassChance+roadChance ? BoardState.ROAD :
					rand < grassChance+roadChance+riverChance ? BoardState.RIVER : BoardState.RAIL;
		}
		boardStateBefore = boardState;
	}

	void CreateTrain(){
#if UNITY_EDITOR
		AssetDatabase.DeleteAsset(pathFolder+"/"+"Train");
		AssetDatabase.CreateFolder(pathFolder, "Train");

		int longTrain = minLongTrain;
		while(longTrain <= maxLongTrain){
			float trainWidth;
			GameObject realTrainInstance = new GameObject("TrainLong_"+longTrain.ToString());
			GameObject trainDummy = Instantiate(carriage, new Vector3(-300, -300, -300), Quaternion.identity) as GameObject;
			trainWidth = trainDummy.transform.GetChild(0).GetComponent<MeshRenderer>().bounds.size.x;
			Destroy(trainDummy);
			GameObject headTr = Instantiate(headTrain, transform.position, Quaternion.identity) as GameObject;
			int i =0;
			while(i < longTrain){
				GameObject trainInstance = Instantiate(carriage, new Vector3(transform.position.x - (i * trainWidth), transform.position.y, transform.position.z), Quaternion.identity) as GameObject;
				trainInstance.transform.SetParent(headTr.transform);
				i++;
			}
			realTrainInstance.transform.position = transform.position;
			headTr.transform.SetParent(realTrainInstance.transform);

			MeshCombine.CreatePrefab(realTrainInstance, pathFolder, "Train", "Train");

//			//This so heavy
//			UnityEngine.Object dummyPrefab = EditorUtility.CreateEmptyPrefab("Assets/CrossyWay/Prefabs/Train/TrainComplete/RealTrain_"+longTrain.ToString()+".prefab");
//			GameObject prefabTrain = PrefabUtility.ReplacePrefab(realTrainInstance, dummyPrefab, ReplacePrefabOptions.ConnectToPrefab) as GameObject;
//			prefabTrain.tag = "Train";
//			//Heavy part end

			Destroy(realTrainInstance);

			longTrain++;
		}

#endif
	}
	void CreateGrassLandAsset(){
#if UNITY_EDITOR
		AssetDatabase.DeleteAsset(pathFolder+"/Grass");
		AssetDatabase.CreateFolder(pathFolder, "Grass");
		GameObject grassPacked = new GameObject("GrassPacked");

		int xGrass = xMinEffectiveBoard;
		while(xGrass <= xMaxEffectiveBoard){
			GameObject nGrass = Instantiate(grass, Vector3.zero, Quaternion.identity)as GameObject;
			nGrass.transform.position = new Vector3(xGrass, 0, 0);
			nGrass.transform.SetParent(grassPacked.transform);
			xGrass++;
		}
		xGrass = xMinBoard;
		while(xGrass < xMinEffectiveBoard){
			GameObject nDGrass = Instantiate(deepGrass, Vector3.zero, Quaternion.identity) as GameObject;
			nDGrass.transform.position = new Vector3(xGrass, 0,0);
			nDGrass.transform.SetParent(grassPacked.transform);
			if(tree.Length == 0){
				Debug.LogError("Tree Array is Null, make sure you have assign it");
				EditorApplication.isPaused = true;
			}
			else{
				GameObject nTree = Instantiate(tree[UnityEngine.Random.Range(0, tree.Length)],new Vector3(xGrass, 0,0), Quaternion.identity)as GameObject;
				nTree.transform.SetParent(grassPacked.transform);

			}
			xGrass++;
		}
		xGrass = xMaxEffectiveBoard+1;
		while(xGrass <= xMaxBoard){
			GameObject nDGrass = Instantiate(deepGrass, Vector3.zero, Quaternion.identity) as GameObject;
			nDGrass.transform.position = new Vector3(xGrass, 0,0);
			nDGrass.transform.SetParent(grassPacked.transform);
			GameObject nTree = Instantiate(tree[UnityEngine.Random.Range(0, tree.Length)],new Vector3(xGrass, 0,0), Quaternion.identity)as GameObject;
			nTree.transform.SetParent(grassPacked.transform);
			xGrass++;
		}
		grassPacked.tag = "Grass";
		MeshCombine.CreatePrefab(grassPacked, pathFolder, "Grass", "Grass");
		
		Destroy(grassPacked);
#endif
	}
	void CreateRiverLandAsset(){
		
		#if UNITY_EDITOR
		AssetDatabase.DeleteAsset(pathFolder+"/River");
		AssetDatabase.CreateFolder(pathFolder, "River");
		GameObject riverPacked = new GameObject("RiverPacked");
		
		int mX = xMinEffectiveBoard;
		while(mX <= xMaxEffectiveBoard){
			GameObject nRiver = Instantiate(river, Vector3.zero, Quaternion.identity)as GameObject;
			nRiver.transform.position = new Vector3(mX, 0, 0);
			nRiver.transform.SetParent(riverPacked.transform);
			mX++;
		}
		mX = xMinBoard;
		while(mX < xMinEffectiveBoard){
			GameObject nRiver = Instantiate(deepRiver, Vector3.zero, Quaternion.identity) as GameObject;
			nRiver.transform.position = new Vector3(mX, 0,0);
			nRiver.transform.SetParent(riverPacked.transform);
			mX++;
		}
		mX = xMaxEffectiveBoard+1;
		while(mX <= xMaxBoard){
			GameObject nRiver = Instantiate(deepRiver, Vector3.zero, Quaternion.identity) as GameObject;
			nRiver.transform.position = new Vector3(mX, 0,0);
			nRiver.transform.SetParent(riverPacked.transform);
			mX++;
		}
		riverPacked.tag = "River";
		MeshCombine.CreatePrefab(riverPacked, pathFolder, "River", "River");
		
		Destroy(riverPacked);
		#endif
	}
	void CreateRailLandAsset(){
		
		#if UNITY_EDITOR
		AssetDatabase.DeleteAsset(pathFolder+"/Rail");
		AssetDatabase.CreateFolder(pathFolder, "Rail");
		GameObject railPacked = new GameObject("RailPacked");
		
		int mX = xMinEffectiveBoard;
		while(mX <= xMaxEffectiveBoard){
			GameObject nRail = Instantiate(rail, Vector3.zero, Quaternion.identity)as GameObject;
			nRail.transform.position = new Vector3(mX, 0, 0);
			nRail.transform.SetParent(railPacked.transform);
			mX++;
		}
		mX = xMinBoard;
		while(mX < xMinEffectiveBoard){
			GameObject nRail = Instantiate(deepRail, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(railPacked.transform);
			mX++;
		}
		mX = xMaxEffectiveBoard+1;
		while(mX <= xMaxBoard){
			GameObject nRail = Instantiate(deepRail, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(railPacked.transform);
			mX++;
		}
		railPacked.tag = "Rail";
		MeshCombine.CreatePrefab(railPacked, pathFolder, "Rail", "Rail");
		
		Destroy(railPacked);
		#endif
	}
	void CreateRoadLandAsset(){
#if UNITY_EDITOR
		AssetDatabase.DeleteAsset(pathFolder+"/Road");
		AssetDatabase.CreateFolder(pathFolder, "Road");

		GameObject solidRoadPacked = new GameObject("SolidRoadPacked");
		int mX = xMinEffectiveBoard;
		while(mX <= xMaxEffectiveBoard){
			GameObject nRail = Instantiate(road, Vector3.zero, Quaternion.identity)as GameObject;
			nRail.transform.position = new Vector3(mX, 0, 0);
			nRail.transform.SetParent(solidRoadPacked.transform);
			mX++;
		}
		mX = xMinBoard;
		while(mX < xMinEffectiveBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(solidRoadPacked.transform);
			mX++;
		}
		mX = xMaxEffectiveBoard+1;
		while(mX <= xMaxBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(solidRoadPacked.transform);
			mX++;
		}
		MeshCombine.CreatePrefab(solidRoadPacked, pathFolder, "Road", "SolidRoad");

		Destroy(solidRoadPacked);
		

		//BREAK =========================================================

		GameObject roadTopLinePacked = new GameObject("roadTopLinePacked");
		mX = xMinEffectiveBoard;
		while(mX <= xMaxEffectiveBoard){
			GameObject nRoad;
			if(mX % 2 == 0){
				nRoad = Instantiate(roadTopLine, Vector3.zero, Quaternion.identity)as GameObject;
			}
			else
				nRoad = Instantiate(road, Vector3.zero, Quaternion.identity)as GameObject;
			nRoad.transform.position = new Vector3(mX, 0, 0);
			nRoad.transform.SetParent(roadTopLinePacked.transform);
			mX++;
		}
		mX = xMinBoard;
		while(mX < xMinEffectiveBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(roadTopLinePacked.transform);
			mX++;
		}
		mX = xMaxEffectiveBoard+1;
		while(mX <= xMaxBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(roadTopLinePacked.transform);
			mX++;
		}
		MeshCombine.CreatePrefab(roadTopLinePacked, pathFolder, "Road", "RoadTopLine");

		Destroy(roadTopLinePacked);

		//BREAK =========================================================
		
		GameObject roadBothLinePacked = new GameObject("roadBothLinePacked");
		mX = xMinEffectiveBoard;
		while(mX <= xMaxEffectiveBoard){
			GameObject nRoad;
			if(mX % 2 == 0){
				nRoad = Instantiate(roadBothLine, Vector3.zero, Quaternion.identity)as GameObject;
			}
			else
				nRoad = Instantiate(road, Vector3.zero, Quaternion.identity)as GameObject;
			nRoad.transform.position = new Vector3(mX, 0, 0);
			nRoad.transform.SetParent(roadBothLinePacked.transform);
			mX++;
		}
		mX = xMinBoard;
		while(mX < xMinEffectiveBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(roadBothLinePacked.transform);
			mX++;
		}
		mX = xMaxEffectiveBoard+1;
		while(mX <= xMaxBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(roadBothLinePacked.transform);
			mX++;
		}
		MeshCombine.CreatePrefab(roadBothLinePacked, pathFolder, "Road", "RoadBothLine");
		
		Destroy(roadBothLinePacked);

		//BREAK =========================================================
		
		GameObject roadBottomLinePacked = new GameObject("roadBottomLinePacked");
		mX = xMinEffectiveBoard;
		while(mX <= xMaxEffectiveBoard){
			GameObject nRoad;
			if(mX % 2 == 0){
				nRoad = Instantiate(roadBottomLine, Vector3.zero, Quaternion.identity)as GameObject;
			}
			else
				nRoad = Instantiate(road, Vector3.zero, Quaternion.identity)as GameObject;
			nRoad.transform.position = new Vector3(mX, 0, 0);
			nRoad.transform.SetParent(roadBottomLinePacked.transform);
			mX++;
		}
		mX = xMinBoard;
		while(mX < xMinEffectiveBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(roadBottomLinePacked.transform);
			mX++;
		}
		mX = xMaxEffectiveBoard+1;
		while(mX <= xMaxBoard){
			GameObject nRail = Instantiate(deepRoad, Vector3.zero, Quaternion.identity) as GameObject;
			nRail.transform.position = new Vector3(mX, 0,0);
			nRail.transform.SetParent(roadBottomLinePacked.transform);
			mX++;
		}
		MeshCombine.CreatePrefab(roadBottomLinePacked, pathFolder, "Road", "RoadBottomLine");

		Destroy(roadBottomLinePacked);
#endif
	}
	void CreateBoardAssets(){
#if UNITY_EDITOR
		
		xMinBoard = (boardWidth/2) * -1;
		xMaxBoard = boardWidth/2;
		xMinEffectiveBoard = (boardWidthEffective/2) * -1;
		xMaxEffectiveBoard = boardWidthEffective/2;

		hasCompleteSetupBoard = false;
		if(creationType == CreationType.GRASS)
			CreateGrassLandAsset();
		else if(creationType == CreationType.TRAIN)
			CreateTrain();
		else if(creationType == CreationType.RIVER)
			CreateRiverLandAsset();
		else if(creationType == CreationType.RAIL)
			CreateRailLandAsset();
		else if(creationType == CreationType.ROAD)
			CreateRoadLandAsset();
		else if(creationType == CreationType.ALL)
		{
			CreateGrassLandAsset();
			CreateTrain();
			CreateRiverLandAsset();
			CreateRailLandAsset();
			CreateRoadLandAsset();
			
		}


		hasCompleteSetupBoard = true;
#endif
	}
	/*
	void AssignRemainGameObjectToPoolingSystem(){
		foreach(GameObject aGo in rowBoardPooling.ToArray()){
			foreach(Transform go in aGo.transform.Cast<Transform>()){
				if(PoolingSystem.instance.poolingDict.ContainsKey(go.tag)
				   || PoolingSystem.instance.poolingDict.ContainsKey(go.name.Remove(go.name.Count() - 7)))
				{
					List<GameObject> updateValue = new List<GameObject>();
					if(PoolingSystem.instance.poolingDict.ContainsKey(go.tag))
						PoolingSystem.instance.poolingDict.TryGetValue(go.tag, out updateValue);
					else if(PoolingSystem.instance.poolingDict.ContainsKey(go.name.Remove(go.name.Count() - 7)))
						PoolingSystem.instance.poolingDict.TryGetValue(go.name.Remove(go.name.Count() - 7), out updateValue);
					updateValue.Add(go.gameObject);
					PoolingSystem.instance.poolingDict[go.tag] = updateValue;
				}
				else{
					Debug.LogError("Can't Find Key at Dictionary Pooling System\nTag : " + go.tag + ", Name : " + go.name.Remove(go.name.Count() - 7));
					#if UNITY_EDITOR
					EditorApplication.isPaused = true;
					#endif
				}
				//go.gameObject.SetActive(false);
			}
			if(rowBoardPooling[0].GetComponent<CarSpawner>() != null){
				rowBoardPooling[0].GetComponent<CarSpawner>().enabled = false;
			}
			else if(rowBoardPooling[0].GetComponent<TrainSpawner>() != null){
				rowBoardPooling[0].GetComponent<TrainSpawner>().enabled = false;
			}
			else if(rowBoardPooling[0].GetComponent<LogSpawner>() != null){
				rowBoardPooling[0].GetComponent<LogSpawner>().enabled = false;
			}
			rowBoardPooling[0].SetActive(false);
			rowBoardPooling.RemoveAt(0);
			List<Vector3> tempTreePos = new List<Vector3>();
			foreach(Transform tr in rowBoardPooling[0].transform.Cast<Transform>()){
				if(tr.tag.Equals("Tree")){
					tempTreePos.Add(tr.position);
				}
			}
		}
	}*/
	public void AssignToPoolingSystemLastRow(){
		foreach(Transform go in rowBoardPooling[0].transform.Cast<Transform>()){
			if(PoolingSystem.instance.poolingDict.ContainsKey(go.tag)
			   || PoolingSystem.instance.poolingDict.ContainsKey(go.name.Remove(go.name.Count() - 7)))
			{
				List<GameObject> updateValue = new List<GameObject>();
				if(PoolingSystem.instance.poolingDict.ContainsKey(go.tag))
					PoolingSystem.instance.poolingDict.TryGetValue(go.tag, out updateValue);
				else if(PoolingSystem.instance.poolingDict.ContainsKey(go.name.Remove(go.name.Count() - 7)))
					PoolingSystem.instance.poolingDict.TryGetValue(go.name.Remove(go.name.Count() - 7), out updateValue);
				updateValue.Add(go.gameObject);
				PoolingSystem.instance.poolingDict[go.tag] = updateValue;
			}
			else{
				Debug.LogError("Can't Find Key at Dictionary Pooling System\nTag : " + go.tag + ", Name : " + go.name.Remove(go.name.Count() - 7));
#if UNITY_EDITOR
				EditorApplication.isPaused = true;
#endif
			}
			go.gameObject.SetActive(false);
		}
		if(rowBoardPooling[0].GetComponent<CarSpawner>() != null){
			rowBoardPooling[0].GetComponent<CarSpawner>().enabled = false;
		}
		else if(rowBoardPooling[0].GetComponent<TrainSpawner>() != null){
			rowBoardPooling[0].GetComponent<TrainSpawner>().enabled = false;
		}
		else if(rowBoardPooling[0].GetComponent<LogSpawner>() != null){
			rowBoardPooling[0].GetComponent<LogSpawner>().enabled = false;
		}
		rowBoardPooling[0].SetActive(false);
		rowBoardPooling.RemoveAt(0);
		List<Vector3> tempTreePos = new List<Vector3>();
		foreach(Transform tr in rowBoardPooling[0].transform.Cast<Transform>()){
			if(tr.tag.Equals("Tree")){
				tempTreePos.Add(tr.position);
			}
		}

		//This is for block the back way
		int noTreePos = xMinEffectiveBoard;
		while(noTreePos < xMaxEffectiveBoard +1){
			foreach(Vector3 treePos in tempTreePos.ToArray()){
				if(treePos.x == noTreePos)
				{
					goto skip;
				}
			}
			SpawnOrMoveObject(noTreePos, (int)rowBoardPooling[0].transform.position.z, rowBoardPooling[0].transform, tree);
		skip:
			noTreePos ++;
		}
	}

	void Awake(){
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
		boardState = BoardState.INITIALIZE;

	}

	// Use this for initialization
	void Start () {
		
#if UNITY_EDITOR
		if(!boardCreationMode)
			RegisterToPoolingSystem();
#elif UNITY_ANDROID
			RegisterToPoolingSystem();
#endif
		
#if UNITY_EDITOR
		if(!boardCreationMode)
#endif
			InitializeFirstBoard();
		
#if UNITY_EDITOR
		if(!boardCreationMode)
#endif
		CanvasMainGame.instance.LoadingGameOverOut();	

#if admob
		AdmobController.instance.ShowBanner();
#endif
#if gpgs
		GPGSController.instance.UnlockingAchievement(GPGSController.instance.firstPlay);
#endif
		
#if UNITY_EDITOR
		if(!boardCreationMode){
#endif
		GameManager.instance.score = 0;
		CanvasMainGame.instance.UpdateBestScore();
			
#if UNITY_EDITOR
		}
#endif

	}

	
#if UNITY_EDITOR
	int frames;
	int skipFrame = 600;
#endif
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
		frames++;
		if(pressNForNewLine){
			if(Input.GetKeyDown(KeyCode.N)){
				//nextPosCamera.z += 1;
				AssignToPoolingSystemLastRow();
				//Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z + 1), 1);
				//iTween.MoveTo(Camera.main.gameObject, nextPosCamera, 1);
			}
			if(Input.GetKeyDown(KeyCode.Quote))
				Application.LoadLevel("Shop");
		}
#endif
#if UNITY_EDITOR
		if(boardCreationMode){
			if(hasCompleteSetupBoard){
				EditorApplication.isPlaying = false;
				return;
			}
			else {
				CreateBoardAssets();
				Debug.LogWarning("!!MUST READ!!After This, you can set false Train Creation Mode and make sure assign all Packed assets at BoardManager\nYou can find all assets at Prefabs/Packed/");
				Debug.LogWarning("!!MUST READ!!After This, you can set false Train Creation Mode and make sure assign all Packed assets at BoardManager\nYou can find all assets at Prefabs/Packed/");
				//Invoke("ExitPlayMode",1);
				return;
			}
		}
#endif
		SpawnBoardPerType();

		if(Time.frameCount % 30 == 0){
			System.GC.Collect();
		}
	}

	void ExitPlayMode(){
		
#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
#endif
	}
}
