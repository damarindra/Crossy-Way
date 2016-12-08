using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
#if UNITY_EDITOR
	public bool deleteAllGameSaved = false;
#endif
	
	[HideInInspector]
	public int score=0;

	void Awake(){
		if(instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
	}

	// Use this for initialization
	void Start () {
		
#if UNITY_EDITOR
		if(deleteAllGameSaved){
			PlayerPrefs.DeleteAll();
		}
#endif
		if(!PlayerPrefsX.GetBool("Initialize",false)){
			PlayerPrefsX.SetBool("Initialize",true);
			PlayerPrefsX.SetBool("Sound", true);
		}
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR
		if(deleteAllGameSaved){
			Debug.LogWarning("DELETE SAVED GAME COMPLETE");
			Debug.LogWarning("DELETE SAVED GAME COMPLETE");
			EditorApplication.isPlaying = false;
		}
#endif
#if admob
		//if(AdmobController.instance.bannerLoaded && !AdmobController.instance.bannerShown)
		//	AdmobController.instance.ShowBanner();
#endif
	}
}
