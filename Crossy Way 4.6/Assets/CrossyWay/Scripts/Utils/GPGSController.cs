using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#if gpgs
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class GPGSController : MonoBehaviour {
#if gpgs

	public static GPGSController instance = null;

	void Awake(){
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
	}

	void Start(){
		SetupGPGS();
	}

	public string leaderboardID;
	public string firstPlay;
	public string play100Times;
	public string play500Times;
	public string firstBuy;
	public string jump500Times;
	public string jump1000Times;
	public string tinyFoot;
	public string hugeJump;

	private Dictionary<string, int> dictAchievement = new Dictionary<string, int>();
	
	private enum GPGS_STATUS{
		LOGIN, FAILED, AUTHENTICATING
	}
	private GPGS_STATUS gpgsStatus = GPGS_STATUS.FAILED;

	public void SetupGPGS(){
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
		
		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
		Login();
	}
	
	public void Login(){
		// authenticate user:
		gpgsStatus = GPGS_STATUS.AUTHENTICATING;
		Social.localUser.Authenticate((bool success) => {
			// handle success or failure
			if(success)
				gpgsStatus = GPGS_STATUS.LOGIN;
			else
				gpgsStatus = GPGS_STATUS.FAILED;
		});
	}
	
	public void IncrementAchievement(string ids){
		if(dictAchievement.ContainsKey(ids)){
			dictAchievement[ids] += 1;
		}
		else{
			dictAchievement.Add(ids, 1);
		}
	}
	
	public void FlushAchievement(){
		foreach (string ach in dictAchievement.Keys)
		{
			PlayGamesPlatform.Instance.IncrementAchievement(ach, dictAchievement[ach], (bool success) => { });
		}
		dictAchievement.Clear();
	}
	
	public void UnlockingAchievement(string ids){
		// unlock achievement (achievement ID "Cfjewijawiu_QA")
		Social.ReportProgress(ids, 100.0f, (bool success) => {
			// handle success or failure
		});
	}
	
	public void PostScoreLeaderboard(int score){
		// post score 12345 to leaderboard ID "Cfji293fjsie_QA")
		Social.ReportScore(score, leaderboardID, (bool success) => {
			// handle success or failure
		});
	}
	
	public void ShowLeaderboard(){
		if(gpgsStatus == GPGS_STATUS.LOGIN)
			Social.ShowLeaderboardUI();
		else if(gpgsStatus == GPGS_STATUS.FAILED)
			Login();
	}	
	public void ShowAchievement(){
		if(gpgsStatus == GPGS_STATUS.LOGIN)
			Social.ShowAchievementsUI();
		else if(gpgsStatus == GPGS_STATUS.FAILED)
			Login();
	}
#endif
}
