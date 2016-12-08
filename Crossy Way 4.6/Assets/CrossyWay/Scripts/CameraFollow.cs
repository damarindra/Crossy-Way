using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	private static Vector3 lastPosition;

	// Use this for initialization
	void Start () {
		lastPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = Vector3.Lerp(transform.position, lastPosition, .05f);
	}

	public static void CameraMove(){
		float gapZ = PlayerController.instance.transform.position.z - lastPosition.z;
		float gapX = PlayerController.instance.transform.position.x - lastPosition.x;
		lastPosition.z = PlayerController.instance.lastPosition.z - gapZ;
		lastPosition.x = Mathf.Lerp(lastPosition.x, PlayerController.instance.lastPosition.x - gapX, .4f);
	}
	public static void CameraMoveOnRiver(){
		float gapX = Mathf.Lerp(PlayerController.instance.transform.position.x - lastPosition.x, lastPosition.x, .4f);
		lastPosition.x = PlayerController.instance.lastPosition.x - gapX;//Mathf.Lerp(lastPosition.x, PlayerController.instance.lastPosition.x - gapX, .4f);
	}
}
