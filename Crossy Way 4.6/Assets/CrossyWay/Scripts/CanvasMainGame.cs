using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class CanvasMainGame : MonoBehaviour {

	public static CanvasMainGame instance = null;

	public Text scoreText;
	public Text coinText;
	public Text bestScoreText;
	public Button soundBtn;
	public Sprite soundOn, soundOff;

	private Animation anim {get{return GetComponent<Animation>();}}

	void Awake(){
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
	}

	void Start(){
		UpdateCoin(SaveLoad.coins);
		if(PlayerPrefsX.GetBool("Sound"))
			soundBtn.GetComponent<Image>().sprite = soundOn;
		else
			soundBtn.GetComponent<Image>().sprite = soundOff;
	}
	public void UpdateBestScore(){
		
		bestScoreText.text = SaveLoad.score.ToString();
		bestScoreText.transform.GetChild(0).GetComponent<Text>().text = SaveLoad.score.ToString();
	}
	public void UpdateScore(int score){
		scoreText.text = score.ToString();
		scoreText.transform.GetChild(0).GetComponent<Text>().text = score.ToString();
	}
	public void UpdateCoin(int coin){
		coinText.text = coin.ToString();
		coinText.transform.GetChild(0).GetComponent<Text>().text = coin.ToString();
	}
	public void Share(string packageName){
		SoundManager.instance.PlayButton();
		ShareAndroid.instance.ShareNow(packageName);
	}
	public void SoundButton(){
		PlayerPrefsX.SetBool("Sound", !PlayerPrefsX.GetBool("Sound"));
		if(PlayerPrefsX.GetBool("Sound"))
			soundBtn.GetComponent<Image>().sprite = soundOn;
		else
			soundBtn.GetComponent<Image>().sprite = soundOff;
		SoundManager.instance.PlayButton();
	}
	public void LeaderboardButton(){
#if gpgs
		GPGSController.instance.ShowLeaderboard();
#endif
	}
	public void AchievementButton(){
#if gpgs
		GPGSController.instance.ShowAchievement();
#endif
	}
	public void VideoAdsButton(){
#if unityad
		if(UnityAdsController.instance.isVideoAdReady){
			UnityAdsController.instance.ShowRewardedAd();
		}
#endif
	}

	//ANimation
	public void GameOverIn(){
		anim.Play("GameOverIn");
	}
	public void GameOverOut(){
		anim.Play ("GameOverOut");
	}
	public void WatchVideoIn(){
		anim.Play("WatchVideoIn");
	}
	public void WatchVideoOut(){
#if unityad
		anim.Play("WatchVideoOut");
#endif
	}
	public void GameOverAndVideoIn(){
		anim.Play("GameOverIn");
#if unityad
		if(UnityAdsController.instance.isVideoAdReady){
			anim["WatchVideoIn"].layer=1;
			anim.Play("WatchVideoIn");
			anim["WatchVideoIn"].weight=.5f;
		}
#endif
	}
	public void GameOverAndVideoOut(){
		anim.Play("GameOverOut");
		anim["WatchVideoOut"].layer=1;
		anim.Play("WatchVideoOut");
		anim["WatchVideoOut"].weight=.5f;
	}
	public void LoadingIn(){
		anim.Play("LoadingIn");
	}
	public void LoadingOut(){
		anim.Play("LoadingOut");
	}
	public void LoadingGameOverOut(){
		GameOverAndVideoOut();
		anim["LoadingOut"].layer = 2;
		anim.Play("LoadingOut");
		anim["LoadingOut"].weight = .5f;
	}
}
