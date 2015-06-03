using UnityEngine;
using System.Collections;

public class powerupScript : MonoBehaviour {

	public Sprite jukeJuiceSprite;
	public Sprite rollingBombSprite;
	public Sprite powerupBlankTile;

	public int powerupNumber = -1;


	public AudioClip pickedUpPowerUpSound;

	
	CircleCollider2D theCircleCollider;

	// Use this for initialization
	void Start () {
		transform.parent = GameObject.Find("Thy Holy See").transform;
		transform.localScale = new Vector3(100,100,100);
		GetComponent<SpriteRenderer>().sortingOrder = 9;

		if(uLink.Network.isServer==true)
		{
			spawnAPowerup();
		}
		theCircleCollider = gameObject.GetComponent<CircleCollider2D>();
	}
	
	// Update is called once per frame
	void Update () {



	}

	public void gotPowerup(int playerID)
	{
			GetComponent<SpriteRenderer>().enabled=false;
			GetComponent<CircleCollider2D>().enabled=false;
			
			transform.uLinkNetworkView().RPC("clientHidePowerupTile", uLink.RPCMode.OthersBuffered, playerID);

			
			
			StartCoroutine(respawnPowerup(60f));
	}

	void spawnAPowerup()
	{


			
		if (Random.value > 0.5f)
		{
			gameObject.tag = "jukeJuice";	
			GetComponent<SpriteRenderer>().sprite = jukeJuiceSprite;
			powerupNumber = 0;
		}
		else
		{
			gameObject.tag = "rollingBomb";
			GetComponent<SpriteRenderer>().sprite = rollingBombSprite;
			powerupNumber = 1;
		}
		GetComponent<SpriteRenderer>().enabled=true;
		GetComponent<CircleCollider2D>().enabled=true;

		transform.uLinkNetworkView().RPC("clientShowPowerupTile", uLink.RPCMode.OthersBuffered, powerupNumber);



	}

	void OnTriggerEnter2D(Collider2D zone)
	{

	}
	void OnTriggerExit2D(Collider2D zone)
	{
		
		checkAndResetRadius();
	}


	//needed for the rare scenario in which a powerup is spawned under two players, but one player decides to leave
	//causes remaining player to immeditately pick up powerup without needing to get 50% of the area covered.
	//checked every 100ms when radius is shrunk
	//better way to do this?
	void checkAndResetRadius()
	{
		Collider2D[] allCollidersOnPowerup;
		
		allCollidersOnPowerup  = Physics2D.OverlapCircleAll(new Vector2(theCircleCollider.transform.position.x-0.004f, theCircleCollider.transform.position.y),0.163f,1 << LayerMask.NameToLayer("blueBallLayer") | 1 << LayerMask.NameToLayer("redBallLayer"));
		
		
		//reset radius if no one there
		if (allCollidersOnPowerup.Length<=1)
		{
			GetComponent<CircleCollider2D>().radius = 0.163f;
			GetComponent<CircleCollider2D>().offset = new Vector2(-0.004f,0);
		}
	}
	



	IEnumerator respawnPowerup(float duration)
	{
		yield return new WaitForSeconds(duration);
				spawnAPowerup();
	}

	[RPC]
	void clientHidePowerupTile(int id)
	{

		GetComponent<CircleCollider2D>().enabled=false;
		GetComponent<SpriteRenderer>().sprite = powerupBlankTile;
		if(uLink.Network.isServer==false && id==uLink.Network.player.id)
		{
			AudioSource.PlayClipAtPoint(pickedUpPowerUpSound, this.transform.position);
		}
	}
	[RPC]
	void clientShowPowerupTile(int number)
	{
		if(number==0)
		{
			gameObject.tag = "jukeJuice";	
			GetComponent<SpriteRenderer>().sprite = jukeJuiceSprite;
			powerupNumber = 0;
		}
		else if (number==1)
		{
			gameObject.tag = "rollingBomb";
			GetComponent<SpriteRenderer>().sprite = rollingBombSprite;
			powerupNumber = 1;
		}
		;
		GetComponent<CircleCollider2D>().enabled=true;
	}
}
