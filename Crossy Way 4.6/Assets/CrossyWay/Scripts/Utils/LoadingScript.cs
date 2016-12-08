using UnityEngine;
using System.Collections;

public class LoadingScript : MonoBehaviour {

	public static LoadingScript instance = null;

	void Awake(){
		if(instance == null)
			instance = this;
	}

	private Animation anim {get{return GetComponent<Animation>();}}

	public void LoadingOut(){
		anim.Play("LoadingOut");
	}

	public void LoadSceneButton(string sceneName){
		SoundManager.instance.PlayButton();
		anim.Play("LoadingIn");
		StartCoroutine(loadScene(sceneName));
#if gpgs
		GPGSController.instance.IncrementAchievement(GPGSController.instance.play100Times);
		GPGSController.instance.IncrementAchievement(GPGSController.instance.play500Times);
#endif
	}
	
	IEnumerator loadScene(string sceneName){
	baleni:
		if(!anim.isPlaying){
#if UNITY_4_6
			Application.LoadLevel(sceneName);
#elif UNITY_5_2
			AsyncOperation asyn = Application.LoadLevelAsync(sceneName);
			yield return asyn;				
#endif
		}
		else{
			yield return new WaitForEndOfFrame();
			goto baleni;
		}
	}
}
