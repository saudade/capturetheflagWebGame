using UnityEngine;
using System.Collections;

public class setFlagToBallBlue : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void LateUpdate(){
		transform.position = new Vector3(transform.parent.Find("blueBall").position.x+0.104f	,transform.parent.Find("blueBall").position.y+0.279f, transform.parent.Find("blueBall").position.z);

	}
}
