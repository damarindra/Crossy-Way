using UnityEngine;
using System.Collections;
using System;
#if admob
using GoogleMobileAds.Api;
#endif
public class AdmobController : MonoBehaviour {
#if admob
	public static AdmobController instance = null;

	public string bannerID;
	public string interstitialID;

	public int interstitialShowPerGame = 3;
	private int counter=0;
	public AdPosition adPosition;

	[HideInInspector]
	public bool bannerLoaded = false; 
	[HideInInspector]
	public bool bannerShown = false;

	BannerView bannerView;
	InterstitialAd interstitial;

	void Awake(){
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
	}

	void Start(){
		BannerSetup();
		InterstitialSetup();
	}

	public void BannerSetup(){
		// Create a 320x50 banner at the top of the screen.
		bannerView = new BannerView(
			bannerID, AdSize.Banner, adPosition);
		bannerView.AdLoaded += HandleAdLoaded;
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the banner with the request.
		bannerView.LoadAd(request);
		
	}
	
	public void InterstitialSetup(){
		// Initialize an InterstitialAd.
		interstitial = new InterstitialAd(interstitialID);
		interstitial.AdClosed += HandleAdClosed;
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the interstitial with the request.
		interstitial.LoadAd(request);
	}
	
	public void ShowBanner(){
		if(bannerLoaded){
			bannerView.Show();
			bannerShown = true;

		}
	}
	public void IncrementInterstitial(){
		counter++;
		if(counter >= interstitialShowPerGame)
		{
			ShowInterstitial();
			counter = 0;
		}
	}

	public void ShowInterstitial(){
		if (interstitial.IsLoaded())
			interstitial.Show();
		else{
			interstitial.Destroy();
			InterstitialSetup();
		}
	}
	private void HandleAdClosed(object sender, EventArgs args){
		interstitial.Destroy();
		InterstitialSetup();
	}
	private void HandleAdLoaded(object sender, EventArgs args){
		bannerLoaded = true;
	}
#endif
}
