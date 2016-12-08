using UnityEngine;
using System.Collections;
using System.Collections.Generic;	
public class PlayerController : MonoBehaviour {

	public static PlayerController instance = null;

	private Animation anim {get{return GetComponent<Animation>();}}
	[HideInInspector]
	public Vector3 lastPosition;
	private Transform logFollow;
	private bool isAlive = true;
	enum Direction{
		LEFT,
		RIGTH,
		FORWARD,
		BACK
	}
	Direction dir = Direction.FORWARD;

	private float gapDistanceCheck = .4f;

	void Awake(){
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
	}

	// Use this for initialization
	void Start () {
		lastPosition = transform.position;
		lastCameraPos = Camera.main.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(isAlive)
			Control();
		else if(!isAlive){
			CheckLand();
			
			transform.position = Vector3.MoveTowards(transform.position, lastPosition, .2f);
		}
	}

	void Move(int xStep, int zStep, float yRotation, Direction direction){
		transform.eulerAngles = new Vector3(0, yRotation, 0);
		dir = direction;
		if(CanMove()){
			Move (xStep, zStep);
			if(zStep >= 1 && (int)lastPosition.z > GameManager.instance.score){
				BoardManager.instance.AssignToPoolingSystemLastRow();
				GameManager.instance.score = (int)lastPosition.z;
				CanvasMainGame.instance.UpdateScore(GameManager.instance.score);
				
#if gpgs
				if(GameManager.instance.score > 100)
					GPGSController.instance.UnlockingAchievement(GPGSController.instance.tinyFoot);
				
				if(GameManager.instance.score > 250)
					GPGSController.instance.UnlockingAchievement(GPGSController.instance.hugeJump);
#endif
			}
			
#if gpgs
			GPGSController.instance.IncrementAchievement(GPGSController.instance.jump500Times);
			GPGSController.instance.IncrementAchievement(GPGSController.instance.jump1000Times);
#endif
			//CameraMove();
		}
	}

	void Move(int xStep, int zStep){
		lastPosition.z = Mathf.RoundToInt(lastPosition.z += zStep);
		if(zStep == 1 || zStep == -1)
			lastPosition.x = Mathf.RoundToInt(lastPosition.x += xStep);
		else
			lastPosition.x += xStep;
		SoundManager.instance.PlayJump();
		anim.Play("CharacterJumpUp");
		CameraFollow.CameraMove();
	}
	void Control(){
#if UNITY_EDITOR
		if(Input.GetKeyUp(KeyCode.UpArrow)){
			Move (0,1,0,Direction.FORWARD);
//			if(dir != Direction.FORWARD){
//				transform.eulerAngles = new Vector3(0, 0, 0);
//				dir = Direction.FORWARD;
//			}
//			if(CanMove()){
//				Move (0, 1);
//				BoardManager.instance.AssignToPoolingSystemLastRow();
//				//CameraMove();
//			}
			
		}
		else if(Input.GetKeyUp(KeyCode.LeftArrow)){
			Move (-1,0,270,Direction.LEFT);
//			if(dir != Direction.LEFT){
//				transform.eulerAngles = new Vector3(0, 270, 0);
//
//				dir = Direction.LEFT;
//			}
//			if(CanMove()){
//				Move (-1, 0);
//				
//			}
		}
		else if(Input.GetKeyUp(KeyCode.RightArrow)){
			Move (1,0,90,Direction.RIGTH);
//			if(dir != Direction.RIGTH){
//				transform.eulerAngles = new Vector3(0, 90, 0);
//				
//				dir = Direction.RIGTH;
//			}
//			if(CanMove()){
//				Move (1, 0);
//			}
		}
		else if(Input.GetKeyUp(KeyCode.DownArrow)){
			Move (0,-1,180,Direction.BACK);
//			if(dir != Direction.BACK){
//				transform.eulerAngles = new Vector3(0, 180, 0);
//				dir = Direction.BACK;
//				}
//			if(CanMove()){
//				Move (0, -1);
//			}
		}



#elif UNITY_ANDROID

#endif
		AndroidControl();
		transform.position = Vector3.MoveTowards(transform.position, lastPosition, .2f);
		//Camera.main.transform.localPosition = Vector3.MoveTowards(Camera.main.transform.localPosition, lastCameraPos, .1f);
	}
	void FixedUpdate(){
		if(isAlive){
			CheckLand();
			CheckMovingObject();

		}
	}

	Vector2 touchDown = new Vector2(), touchUp = new Vector2();
	void AndroidControl(){
		if(Input.GetMouseButtonDown(0))
			touchDown = Camera.main.ScreenToViewportPoint(Input.mousePosition);
		else if(Input.GetMouseButtonUp(0)){
			touchUp = Camera.main.ScreenToViewportPoint(Input.mousePosition);
			float lengthX, lengthY;
			lengthX = touchDown.x - touchUp.x;
			lengthY = touchDown.y - touchUp.y;
			if(Vector2.Distance(touchDown, touchUp) < .09f)
				Move (0,1,0,Direction.FORWARD);
			else if(Mathf.Abs(lengthX) > Mathf.Abs(lengthY)){
				//Left or Right
				if(lengthX < 0){
					Move (1,0,90,Direction.RIGTH);

				}
				else
					Move (-1,0,270, Direction.LEFT);
			}
			else if(Mathf.Abs(lengthY) > Mathf.Abs(lengthX)){
				//forward or backward
				if(lengthY < 0)
					Move (0, 1, 0, Direction.FORWARD);
				else
					Move (0, -1, 180, Direction.BACK);
			}
		}

	}

	void CheckLand(){
		float distance = Vector3.Distance(transform.position, lastPosition);
		Transform tr = CheckGroundType();
		if(tr != null){
			if(distance <= .1f){
				if(tr.gameObject.tag == "Log"){
					lastPosition.x = tr.position.x;
					if(isAlive){
						CameraFollow.CameraMoveOnRiver();
						if(tr.GetComponentInParent<LogSpawner>().isFromRight){
							if(lastPosition.x < BoardManager.instance.xMinEffectiveBoard){
								GameOver();
							}
						}
						else{
							if(lastPosition.x > BoardManager.instance.xMaxEffectiveBoard){
								GameOver();
							}
						}
					}
				}
				else if(tr.gameObject.tag == "River"){
					//DIe
					lastPosition.y -= 2;
					SoundManager.instance.PlaySplash();
					GameOver();
				}
				else if(tr.tag.Equals("Coin")){
					SaveLoad.coins = SaveLoad.coins+1;
					List<GameObject> updateList = new List<GameObject>();
					PoolingSystem.instance.poolingDict.TryGetValue(tr.tag, out updateList);
					updateList.Add(tr.gameObject);
					PoolingSystem.instance.poolingDict[tr.tag] = updateList;
					tr.gameObject.SetActive(false);
					CanvasMainGame.instance.UpdateCoin(SaveLoad.coins);
					//if(SoundManager.instance != null)
					SoundManager.instance.PlayCoin();
				}
			}
			else if(tr.tag.Equals("Coin")){
				SaveLoad.coins = SaveLoad.coins+1;
				List<GameObject> updateList = new List<GameObject>();
				PoolingSystem.instance.poolingDict.TryGetValue(tr.tag, out updateList);
				updateList.Add(tr.gameObject);
				PoolingSystem.instance.poolingDict[tr.tag] = updateList;
				tr.gameObject.SetActive(false);
				CanvasMainGame.instance.UpdateCoin(SaveLoad.coins);
				//if(SoundManager.instance != null)
				SoundManager.instance.PlayCoin();
			}
			else if(tr == null){
				
			}
			else if(tr.gameObject.tag == "Log"){
				lastPosition.x = tr.position.x;
				CameraFollow.CameraMoveOnRiver();
			}
			else if(tr.gameObject.tag.Equals("SolidRoad") || tr.gameObject.tag.Equals("Rail") || tr.gameObject.tag.Equals("Grass")){
				
			}
		}
	}
	void CheckMovingObject(){
		//Ketika diam, Raycast Kanan dan Kiri
		//Ketika bergerak, Raycast Ke depan. Seperti waktu jalan
		float distance = Vector3.Distance(transform.position, lastPosition);
		if(distance <= .1f){
			//Diam
			Vector3 originRight = lastPosition;
			Vector3 originLeft = lastPosition;
			originRight.y = 1;
			originRight.x += .25f;
			originLeft.y = 1;
			originLeft.x -= .25f;
			GameObject go = Raycasting(originRight, Vector3.down, 1f);
			if(go != null){
				if(go.tag.Equals("Car") || go.tag.Equals("Train")){
					//Gepeng Kebawah
					SoundManager.instance.PlayHit();
					transform.localScale = new Vector3(1,.1f,1);
					GameOver();
				}
				else if(go.tag.Equals("DeepRiver")){
					SoundManager.instance.PlaySplash();
					GameOver();
				}
			}
			GameObject go1 = Raycasting(originLeft, Vector3.down, 1f);
			if(go1 != null){
				if(go1.tag.Equals("Car") || go1.tag.Equals("Train")){
					//Gepeng Kebawah
					SoundManager.instance.PlayHit();
					transform.localScale = new Vector3(1,.1f,1);
					GameOver();
				}
				else if(go1.tag.Equals("DeepRiver")){
					SoundManager.instance.PlaySplash();
					GameOver();
				}
			}
		}
		else if(distance <= .6f){
			//Gerak
			GameObject go = Raycasting(transform.forward, .3f);
			if(go != null){
				if(go.tag.Equals("Car") || go.tag.Equals("Train")){
					//Gepeng Ngikutin Moving Object
					SoundManager.instance.PlayHit();
					transform.localScale = new Vector3(1,1,.1f);
					GameOver();
				}
			}
		}
	}


	Transform CheckGroundType(){
		float distance = Vector3.Distance(transform.position, lastPosition);
		if(distance <= gapDistanceCheck){
			GameObject go = Raycasting(Vector3.down);
			if(go != null)
				return go.transform;
		}
		return null;
	}

	bool CanMove(){
		Vector3 targetPosition = lastPosition + transform.forward;
		if(targetPosition.x < BoardManager.instance.xMinEffectiveBoard || targetPosition.x > BoardManager.instance.xMaxEffectiveBoard){
			return false;
		}
		else {
			GameObject go = Raycasting(transform.forward);
			if(go != null){
				if(go.tag.Equals("Tree") || go.tag.Equals("DeepRoad") || go.tag.Equals("DeepGrass") || go.tag.Equals("DeepRail") || go.tag.Equals("DeepRiver")){
					return false;
				}
			}
			float distance = Vector3.Distance(transform.position, lastPosition);
			if(distance >= .15f)
				return false;
			return true;
		}
	}

	GameObject Raycasting(Vector3 dir){
		return Raycasting(dir,1);
	}
	GameObject Raycasting(Vector3 dir, float distance){
		return Raycasting(lastPosition, dir, distance);
	}
	GameObject Raycasting(Vector3 origin, Vector3 dir, float distance){
		RaycastHit hit;
		origin.y += .5f;
		Physics.Raycast(origin, dir, out hit, distance);
		if(hit.collider != null)
			return hit.collider.gameObject;
		return null;
	}

	Vector3 lastCameraPos;
	void CameraMove(){
		lastCameraPos.z +=1;
		iTween.MoveTo(Camera.main.gameObject, lastCameraPos, 1);
	}

	void GameOver(){
		if(isAlive){
			isAlive = false;
			CanvasMainGame.instance.GameOverAndVideoIn();
#if admob
			AdmobController.instance.IncrementInterstitial();
#endif

			if(GameManager.instance.score > SaveLoad.score){
				SaveLoad.score = GameManager.instance.score;
				CanvasMainGame.instance.UpdateBestScore();
			}
#if gpgs

			GPGSController.instance.PostScoreLeaderboard(SaveLoad.score);
			GPGSController.instance.FlushAchievement();
#endif
		}
	}
}
