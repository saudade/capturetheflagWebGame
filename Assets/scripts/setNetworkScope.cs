using UnityEngine;
using System.Collections;
//don't always send updates to clients about other balls unless they are within our view. 
//Two reasons for this: save bandwidth and prevent clients from cheating by enlarging their view
public class setNetworkScope : uLink.MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void LateUpdate()
	{
		transform.position = transform.parent.GetChild(0).position;
	}

	//if ball hits box trigger, set updates client ball recieves to true
	void OnTriggerEnter2D(Collider2D col) {
		serverBallScript collideServerScript = col.GetComponent<serverBallScript>();
		showPlayer(collideServerScript.player,collideServerScript.playerID,collideServerScript.hasJukeJuice,collideServerScript.hasRollingBomb,collideServerScript.isHonking,collideServerScript.hasFlag);
	}
	//if ball hits leaves trigger, set updates client ball recieves to false
	void OnTriggerExit2D(Collider2D col) {
		hidePlayer(col.GetComponent<serverBallScript>().player,col.GetComponent<serverBallScript>().playerID);
	}

	void OnTriggerStay2D (Collider2D col) {
		if(transform.parent.uLinkNetworkView().GetScope(col.GetComponent<serverBallScript>().player)==false)
		{
			serverBallScript collideServerScript = col.GetComponent<serverBallScript>();
			showPlayer(collideServerScript.player,collideServerScript.playerID,collideServerScript.hasJukeJuice,collideServerScript.hasRollingBomb,collideServerScript.isHonking,collideServerScript.hasFlag);
		}
	}

	public void hidePlayer(uLink.NetworkPlayer player, int id)
	{

		//dont hide self
		if(player!=transform.parent.GetComponentInChildren<serverBallScript>().player)
		{

			transform.parent.uLinkNetworkView().RPC("hideBall", uLink.RPCMode.Owner,id);

			transform.parent.uLinkNetworkView().SetScope(player,false);
		}

	}

	public void showPlayer(uLink.NetworkPlayer player, int id, bool hasJukeJuice, bool hasRollingBomb, bool isHonking, bool hasFlag)
	{

		transform.parent.uLinkNetworkView().RPC("showBall", uLink.RPCMode.Owner,id,hasJukeJuice, hasRollingBomb, isHonking, hasFlag);
		transform.parent.uLinkNetworkView().SetScope(player,true);
	}
}
