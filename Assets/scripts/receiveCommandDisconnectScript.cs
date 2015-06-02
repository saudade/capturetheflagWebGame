using UnityEngine;
using System.Collections;

public class receiveCommandDisconnectScript : uLink.MonoBehaviour {

	

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//called only when client disconnects with flag.
	[RPC]
	void clientResetFlag(int flagColor)
	{
		if(GameObject.FindGameObjectWithTag("owner")!=null)
		{
			GameObject.FindGameObjectWithTag("owner").GetComponent<clientBallScript>().clientResetFlag(flagColor,-1,-1);
		}

	}


}
