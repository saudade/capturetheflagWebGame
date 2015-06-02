using UnityEngine;
using System.Collections;

public class boostScript : uLink.MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void resetBoost()
	{
		transform.GetComponentInChildren<MeshRenderer>().enabled=false;
		StartCoroutine(respawnBoost(20f, transform.GetComponentInChildren<MeshRenderer>()));
		GameObject.FindGameObjectWithTag("boostControl").uLinkNetworkView().RPC("hideBoost", uLink.RPCMode.OthersBuffered, gameObject.name);
	}
	IEnumerator respawnBoost(float duration, MeshRenderer boost)
	{
		yield return new WaitForSeconds(duration);
			GameObject.FindGameObjectWithTag("boostControl").uLinkNetworkView().RPC("enableBoost", uLink.RPCMode.OthersBuffered, boost.gameObject.name);
			boost.enabled=true;
	}

	[RPC]
	void enableBoost(string nameObject)
	{
		GameObject.Find(nameObject).GetComponentInChildren<MeshRenderer>().enabled=true;
	}
	[RPC]
	void hideBoost(string nameObject)
	{
		GameObject.Find(nameObject).GetComponentInChildren<MeshRenderer>().enabled=false;
	}
}
