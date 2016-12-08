using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour {

	public static SoundManager instance = null;

	private AudioSource backgroundSource {get{return GetComponent<AudioSource>();}}
	private AudioSource effectSource {get{return transform.GetChild(0).GetComponent<AudioSource>();}}
	private AudioSource coinSource {get{return transform.GetChild(1).GetComponent<AudioSource>();}}
	public AudioClip jumpClip;
	public AudioClip hitClip;
	public AudioClip splashedClip;
	public AudioClip[] coinClips;
	public AudioClip buttonClip;


	void Awake(){
		if(instance == null)
			instance = this;
		else if(instance != this)
			Destroy(gameObject);
		DontDestroyOnLoad(gameObject);
	}

	void Start(){
		backgroundSource.loop = true;
		backgroundSource.playOnAwake = true;
		effectSource.loop = false;
		effectSource.playOnAwake = false;
		coinSource.loop = false;
		coinSource.playOnAwake = false;
	}

	void Update(){
		if(!PlayerPrefsX.GetBool("Sound"))
		{
			if(!backgroundSource.mute)
				backgroundSource.mute = true;
			if(!effectSource.mute)
				effectSource.mute = true;
			if(!coinSource.mute)
				coinSource.mute = true;
		}
		else if(PlayerPrefsX.GetBool("Sound"))
		{
			if(backgroundSource.mute)
				backgroundSource.mute = false;
			if(effectSource.mute)
				effectSource.mute = false;
			if(!coinSource.mute)
				coinSource.mute = false;
		}
	}

	public void PlayClip(AudioClip clip){
		effectSource.clip = clip;
		effectSource.Play();
	}
	public void PlayClip(AudioClip[] clips){
		int randomIndex = Random.Range (0, clips.Length);
		coinSource.clip = clips[randomIndex];
		coinSource.Play();
	}
	public void PlayHit(){
		effectSource.clip = hitClip;
		effectSource.Play();
	}
	public void PlaySplash(){
		effectSource.clip = splashedClip;
		effectSource.Play();
	}
	public void PlayJump(){
		effectSource.clip = jumpClip;
		effectSource.Play();
	}
	public void PlayCoin(){
		PlayClip(coinClips);
	}
	public void PlayButton(){
		PlayClip(buttonClip);
	}
}
