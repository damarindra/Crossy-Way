using UnityEngine;
using System.Collections;

public class SplashScreen : MonoBehaviour {

	public float waitTime;
	private float timer;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(timer >= 0 && timer < waitTime)
			timer += Time.deltaTime;
		else if(timer >= waitTime){
			timer = -1;
#if UNITY_5
			StartCoroutine(LoadScene("Main"));
#elif UNITY_4_6
			Application.LoadLevel("Main");
#endif
		}
	}

	IEnumerator LoadScene(string scene){
		AsyncOperation asyn = Application.LoadLevelAsync(scene);
		yield return asyn;
	}
}
