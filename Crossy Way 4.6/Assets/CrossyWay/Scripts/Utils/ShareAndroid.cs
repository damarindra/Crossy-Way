using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

public class ShareAndroid : MonoBehaviour {
	public static ShareAndroid instance = null;
	
	void Awake(){
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
	}
	
	private string url = "https://play.google.com/store/apps/details?id=";
	private string packageName;

	public void ShareNow(string packageName){
		this.packageName = packageName;
		StartCoroutine(Share());
	}

	public void TakeScreenshot(){
		string path = System.IO.Path.Combine(Application.persistentDataPath, "share.png");
		Application.CaptureScreenshot("share.png");
	}
	
	public IEnumerator Share(){
		yield return new WaitForEndOfFrame();
		#if UNITY_ANDROID
		TakeScreenshot();
		
		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
		
		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intentObject.Call<AndroidJavaObject>("setType", "image/*");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "I've got " + GameManager.instance.score.ToString() + "!!! Can you beat it?");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), "I've got " + GameManager.instance.score.ToString() + "!!! Can you beat it?");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "Play now at " +url+packageName);
		
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		AndroidJavaClass fileClass = new AndroidJavaClass("java.io.File");
		
		AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", Application.persistentDataPath + "/share.png");// Set Image Path Here
		
		AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);
		
		//			string uriPath =  uriObject.Call<string>("getPath");
		bool fileExist = fileObject.Call<bool>("exists");
		Debug.Log("File exist : " + fileExist);
		if (fileExist)
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
		
		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		currentActivity.Call("startActivity", intentObject);
		#endif
	}
}