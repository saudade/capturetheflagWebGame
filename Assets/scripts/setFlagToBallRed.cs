using UnityEngine;
using System.Collections;

public class setFlagToBallRed : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void LateUpdate()
	{
		//set position of flag to ball position plus hardcoded values
		transform.position = new Vector3(transform.parent.Find("redBall").position.x+0.104f	,transform.parent.Find("redBall").position.y+0.279f, transform.parent.Find("redBall").position.z);
			
	}
}
