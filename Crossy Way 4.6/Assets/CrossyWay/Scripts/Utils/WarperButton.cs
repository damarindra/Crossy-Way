using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WarperButton : MonoBehaviour {
	//public GameObject[] buttons;
	private Animation anim {get{return GetComponent<Animation>();}}

	private bool isOpened = false;

	// Use this for initialization
	void Start () {
		GetComponent<Button>().onClick.AddListener(()=>OpenWarper());
	}
	public void OpenWarper(){
		if(!isOpened){
			anim.Play("OptionWarpOut");
			isOpened = true;
		}
		else if(isOpened)
		{
			anim.Play("OptionWarpIn");
			isOpened = false;
		}
	}
	
}
