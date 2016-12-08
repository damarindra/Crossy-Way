using UnityEngine;
using System.Collections;
#if unityad
using UnityEngine.Advertisements;
#endif
public class UnityAdsController : MonoBehaviour {
#if unityad
	public static UnityAdsController instance = null;


	public string unityAdId = "";
#if UNITY_4_6
	public string rewardAdId = "rewardedVideoZone";
#elif UNITY_5_2
	private string rewardAdId = "rewardedZone";
#endif
	public int rewardAmount = 50;

	void Awake(){
		if(instance == null)
			instance = this;
	}

	void Start(){
#if UNITY_4_6
		Advertisement.Initialize (unityAdId);
#endif
	}

	public bool isVideoAdReady{
		get{return Advertisement.IsReady();}
	}

	public void ShowRegularAd(){
		if(isVideoAdReady)
			Advertisement.Show();
	}
	public void ShowRewardedAd()
	{
		if (Advertisement.IsReady(rewardAdId))
		{
			var options = new ShowOptions { resultCallback = HandleShowResult };
			Advertisement.Show(rewardAdId, options);
		}
	}
	
	private void HandleShowResult(ShowResult result)
	{
		switch (result)
		{
		case ShowResult.Finished:
			Debug.Log("The ad was successfully shown.");
			SaveLoad.coins+=rewardAmount;
			CanvasMainGame.instance.UpdateCoin(SaveLoad.coins);
			CanvasMainGame.instance.WatchVideoOut();
			break;
		case ShowResult.Skipped:
			Debug.Log("The ad was skipped before reaching the end.");
			break;
		case ShowResult.Failed:
			Debug.LogError("The ad failed to be shown.");
			break;
		}
	}
#endif
}
