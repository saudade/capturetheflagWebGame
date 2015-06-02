using UnityEngine;
using System.Collections;

public class cameraFollowBallScript : MonoBehaviour {

	GameObject mainCamera;
	public bool follow=true;

	// Use this for initialization
	void Start () {
		mainCamera = GameObject.Find("Main Camera");
	}
	
	// Update is called once per frame
	void LateUpdate () {
		centerOnBall();
	}
	float newX;
	float newY;
	
	void centerOnBall () {
		if (follow==true)
		{
			newX =  transform.position.x;
			newY =  transform.position.y;

			mainCamera.gameObject.transform.position = new Vector3(newX, newY, -10);   
		}
	}

}
		                                                  