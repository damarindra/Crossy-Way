using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CharacterShop : MonoBehaviour {
	public static CharacterShop instance = null;

	[System.Serializable]
	public class CharacterStatus{
		[SerializeField]
		public GameObject chara;
		private GameObject charaWorld;
		
		[SerializeField]
		private int price;
		
		public int prices{
			get{return price;}
			set{price = value;}
		}
		public GameObject charaOnWorld{
			set{charaOnWorld = value;}
			get{return charaOnWorld;}
		}
	}
	public CharacterStatus[] characters ;


	private Dictionary<string, List<Color>> bwColor = new Dictionary<string, List<Color>>();
	private Dictionary<string, List<Color>> trueColor = new Dictionary<string, List<Color>>();
	private List<GameObject> characterOnWorld = new List<GameObject>();

	private GameObject characterChoosen = null;

	public Button buyButton;
	public Button chooseButton;
	public Text coinText;
	public Text priceText;
	public Text nameCharacter;

	void ChangeText(Text text, string str){
		text.text = str;
		text.transform.GetChild(0).GetComponent<Text>().text = str;
	}

	// Use this for initialization
	void Awake () {
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
			
	}

	void Start(){
		SaveLoad.characterRecentUse = SaveLoad.characterRecentUse;
		PlayerPrefsX.SetBool(SaveLoad.characterRecentUse, true);
		if(Application.loadedLevelName.Equals("Shop")){
			AssignColorToDict();
			FilterUnlockAndLockCharacter();
			originPosition = transform.parent.position;
			cornerPosition = transform.parent.position + (transform.parent.right * Vector3.Distance(characterOnWorld[0].transform.position, characterOnWorld[characterOnWorld.Count-1].transform.position));
			ChangeText(coinText, SaveLoad.coins.ToString());
		}
		LoadingScript.instance.LoadingOut();
	}

	void AssignColorToDict(){
		int index = 0;
		foreach(CharacterStatus cs in characters){
			GameObject go = Instantiate(cs.chara, Vector3.zero, Quaternion.identity) as GameObject;
			go.transform.position = new Vector3(-index, 0, -index * .5f);
			go.AddComponent<BoxCollider>();
			go.name = cs.chara.name;
			List<Color> trueColors = new List<Color>();
			List<Color> bwColors = new List<Color>();
			Material[] mats = go.GetComponentInChildren<MeshRenderer>().materials;
			foreach(Material mat in mats){
				trueColors.Add(mat.color);
				float grayColor = (mat.color.r + mat.color.g + mat.color.b) /3;
				bwColors.Add(new Color(grayColor, grayColor, grayColor));
			}
			trueColor.Add(cs.chara.name, trueColors);
			bwColor.Add(cs.chara.name, bwColors);
			characterOnWorld.Add(go);
			index++;
		}
	}

	void FilterUnlockAndLockCharacter(){
		int i = 0;
		foreach(GameObject ch in characterOnWorld.ToArray()){
			if(PlayerPrefsX.GetBool(ch.name, false)){
				List<Color> colors = new List<Color>();
				trueColor.TryGetValue(ch.name, out colors);
				Material[] chMats = ch.GetComponentInChildren<MeshRenderer>().materials;
				int index = 0;
				foreach(Color clr in colors.ToArray()){
					chMats[index].color = clr;
					index++;
				}
				ch.GetComponentInChildren<MeshRenderer>().materials = chMats;
			}
			else{
				PlayerPrefsX.SetBool(ch.name, false);
				List<Color> colors = new List<Color>();
				bwColor.TryGetValue(ch.name, out colors);
				Material[] chMats = ch.GetComponentInChildren<MeshRenderer>().materials;
				int index = 0;
				foreach(Color clr in colors.ToArray()){
					chMats[index].color = clr;
					index++;
				}
				ch.GetComponentInChildren<MeshRenderer>().materials = chMats;
			}
			i++;
		}
	}

	void Update(){
		if(Application.loadedLevelName.Equals("Shop")){
			ControlSelection();
			RaycastCharacter();

		}
		if(Application.loadedLevelName.Equals("test")){
			if(Input.GetKeyDown(KeyCode.Quote)){
				Application.LoadLevel("Main");
			}
		}
	}

	GameObject lastCharacterCast = null;
	void RaycastCharacter(){
		RaycastHit hit;
		Physics.Raycast(transform.position, transform.forward, out hit);
		if(hit.collider != null){
			if(hit.collider.gameObject != lastCharacterCast){
				if(lastCharacterCast != null)
					lastCharacterCast.transform.localScale = Vector3.one;
				lastCharacterCast = hit.collider.gameObject;
				lastCharacterCast.transform.localScale *= 1.5f;
				if(PlayerPrefsX.GetBool(lastCharacterCast.name, false)){
					characterChoosen = StringToCSGameObject(lastCharacterCast.name);
					ChangeText(priceText, "");
				}
				else
					ChangeText(priceText, StringToCSPrice(lastCharacterCast.name).ToString());
				ChangeText(nameCharacter, lastCharacterCast.name);
				BuyAndPlaySwitcher();

			}
		}
	}

	public GameObject StringToCSGameObject(string name){
		foreach(CharacterStatus cs in characters){
			if(cs.chara.name == name){
				return cs.chara;
			}
		}
		Debug.LogError("Character Not Found");
		return null;
	}

	int StringToCSPrice(string name){
		foreach(CharacterStatus cs in characters){
			if(cs.chara.name == name){
				return cs.prices;
			}
		}
		Debug.LogError("Character Not Found");
		return 9999;
	}

	Vector2 lastTouchPos = new Vector2();
	Vector3 originPosition = new Vector3(), cornerPosition = new Vector3();
	void ControlSelection(){
		if(Input.GetMouseButtonDown(0))
			lastTouchPos = Input.mousePosition;
		if(Input.GetMouseButton(0)){
			float distanceX = lastTouchPos.x - Input.mousePosition.x;
			Vector3 lastPosition = transform.parent.position + transform.parent.right * (distanceX * .05f);

			if(lastPosition.x > originPosition.x)
				lastPosition = originPosition;
			else if(lastPosition.x < cornerPosition.x)
				lastPosition = cornerPosition;

			transform.parent.position = lastPosition;
			lastTouchPos = Input.mousePosition;
		}
	}

	public void BuyAndPlaySwitcher(){
		if(PlayerPrefsX.GetBool(lastCharacterCast.name)){
			chooseButton.gameObject.SetActive(true);
			buyButton.gameObject.SetActive(false);
		}
		else{
			if(StringToCSPrice(lastCharacterCast.name) <= SaveLoad.coins){
				buyButton.gameObject.SetActive(true);
				chooseButton.gameObject.SetActive(false);
			}
			else if(StringToCSPrice(lastCharacterCast.name) > SaveLoad.coins){
				buyButton.gameObject.SetActive(false);
				chooseButton.gameObject.SetActive(false);
			}
		}
	}

	public void BuyCharacter(){
		if(StringToCSPrice(lastCharacterCast.name) <= SaveLoad.coins){
			//Bisa beli
			SaveLoad.coins -= StringToCSPrice(lastCharacterCast.name);
			ChangeText(coinText, SaveLoad.coins.ToString());
			PlayerPrefsX.SetBool(lastCharacterCast.name, true);
			SaveLoad.characterRecentUse = lastCharacterCast.name;
			characterChoosen = lastCharacterCast;
			BuyAndPlaySwitcher();
			FilterUnlockAndLockCharacter();
			
			#if gpgs
			GPGSController.instance.UnlockingAchievement(GPGSController.instance.firstBuy);
			#endif
		}
	}

	public void ChooseCharacter(){
		characterChoosen.transform.localScale = Vector3.one;
		SaveLoad.characterRecentUse = characterChoosen.name;
		BoardManager.characterInstance = characterChoosen;
		LoadingScript.instance.LoadSceneButton("Main");
	}

	IEnumerator LoadScene(string sceneName){
		AsyncOperation asyn = Application.LoadLevelAsync(sceneName);
		yield return asyn;
	}
}
