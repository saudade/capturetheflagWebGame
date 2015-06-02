using UnityEngine;
using System.Collections;
using uLink;



//script that runs for each ball on server. independent of client ball script.

public class serverBallScript : uLink.MonoBehaviour {

	public Rigidbody2D ballphysics;

	public bool hasFlag = false;
	bool hasBlueFlag = false;
	bool hasRedFlag = false;

	bool isInJail = false;
	public string playerName;
	public uLink.NetworkPlayer player;

	public bool hasRollingBomb = false;
	public bool hasJukeJuice = false;

	public bool doubleRolling = false;


	float acceleration = 1f;
	float jukeJuiceMultipler;

	private bool leftKeyDown = false;
	private bool rightKeyDown = false;
	private bool downKeyDown = false;
	private bool upKeyDown = false;
	
	
	[HideInInspector]
	public int playerID;

	bool gamePause = false;

	float currentSpeedX;
	float currentSpeedY;

	public int teamColor = 1;


	float boostSpeed = 7.5f;
	float maxRegSpeed = 2.5f;

	//these variables used for flash when ball has rolling bomb
	float lerpTime = 0.35f;
	float currentLerpTime;
	
	bool flashBall=false;
	bool lerpFlashUp=true;


	private Vector2 mobileMovement;
	private bool onMobile=false;


	//server needs to receieve a MOVE command in 40 seconds or else it will kick you.
	//will not reset b/c you are typing or sent a message. this aint no chat room
	public bool kickAFKers = true;
	float noActionAfkKicktime = 35;
	float afkTimer = 0.0f;

	serverStart.InstantiateOnConnected serverInstantiate = new serverStart.InstantiateOnConnected();


	public int maxPausesPerPlayer = 3;
	private Vector2 beforePauseVelocity;
	private float beforePauseRotationVelocity;
	private bool processOfBeingPaused = false;


	private Vector2 beforeRollingCollideVelocity;
	private float beforeRollingCollideRotation;


	public bool isHonking = false;

	// Use this for initialization
	void Start () {

		ballphysics =  transform.GetComponent<Rigidbody2D>(); //get ball 2drigiddbody
		Time.timeScale=1;

		//if we join game and its already paused
		if(GameObject.FindGameObjectWithTag("server").GetComponent<serverStart>().gamePaused==true)
		{
			transform.parent.uLinkNetworkView().RPC("pauseGame", uLink.RPCMode.Owner);
			pauseGameNow();
			processOfBeingPaused=false;
		}

	
	}
	
	// Update is called once per frame
	void Update () {
		rollingBombFlash();
		checkIfAfk();
	}
	void LateUpdate()
	{
		setJukeJuiceToBall();
		setHonkingSpriteToBall();


	}

	void FixedUpdate()
	{
		serverDoMovement();

		if(hasRollingBomb==true)
		{
			beforeRollingCollideVelocity = ballphysics.velocity;
			beforeRollingCollideRotation = ballphysics.angularVelocity;
		}
	}
	void setHonkingSpriteToBall()
	{
		if(isHonking==true)
		{
			transform.parent.GetChild(3).position=new Vector3(transform.parent.GetChild(0).position.x+0.015f	,transform.parent.GetChild(0).position.y+0.28f, transform.parent.GetChild(3).position.z);
		}
	}
	void setJukeJuiceToBall()
	{

		transform.parent.FindChild("jukejuicepower").position = new Vector3(transform.position.x-0.111f	,transform.position.y-0.141f, transform.position.z);
		
	}






	void checkIfAfk()
	{
		if(kickAFKers==true && gamePause==false)
		{
			afkTimer+=Time.deltaTime;
			if(afkTimer>noActionAfkKicktime)
			{
				uLink.Network.CloseConnection(player, true);
			}
		}
	}
	//show flash effect on rolling bomb
	void rollingBombFlash()
	{
		if (flashBall==true)
		{
			//increment timer once per frame
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > lerpTime) {
				currentLerpTime = lerpTime;
			}
			
			//lerp!	
			float perc = currentLerpTime / lerpTime;
			
			if(lerpFlashUp==true)
			{
				if(transform.parent.GetChild(0).GetComponent<SpriteRenderer>()!=null)
				{
					transform.parent.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount",Mathf.Lerp(0f, 0.27f, perc));
				}
					if (perc>=1f)
				{
					currentLerpTime = 0f;
					lerpFlashUp=false;
				}
			}
			else if(lerpFlashUp==false)
			{
				if(transform.parent.GetChild(0).GetComponent<SpriteRenderer>()!=null)
				{
					transform.parent.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount",Mathf.Lerp(0.27f, 0f, perc));
				}
					if (perc>=1f)
				{
					currentLerpTime = 0f;
					lerpFlashUp = true;
				}
			}
		}
		
	}
	//used to transfer position and rotation data
	void uLink_OnSerializeNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{

		if (stream.isWriting)
		{
			// This is performed on serialization.
			stream.Write(transform.position);
			stream.Write(transform.rotation);
			stream.Write(ballphysics.velocity);
			stream.Write(ballphysics.angularVelocity);


		}
	}





	//returns true if ball is passed mid line
	bool checkIfBallgoesJail()
	{
		RaycastHit2D hit;
		Vector2 startPos;
		if(teamColor==0)//if blue
		{
			//start ray at top left of blue ball and then make the direction of the vector go towards the bottom right

			startPos= new Vector2(transform.position.x+GetComponent<CircleCollider2D>().bounds.size.x/2*Mathf.Cos(37f * Mathf.Deg2Rad), transform.position.y-GetComponent<CircleCollider2D>().bounds.size.y/2*Mathf.Sin(37f * Mathf.Deg2Rad));
			hit = Physics2D.Raycast(startPos, new Vector2(0.5f, -0.5f), 20, 1 << LayerMask.NameToLayer("dividerLayer"));
		}
		else//if red
		{
			//start ray at the bottom right of red ball and then make the direction of the vector go towards the top left

			startPos= new Vector2(transform.position.x-GetComponent<CircleCollider2D>().bounds.size.x/2*Mathf.Cos(37f * Mathf.Deg2Rad), transform.position.y+GetComponent<CircleCollider2D>().bounds.size.y/2*Mathf.Sin(37f * Mathf.Deg2Rad));
			hit = Physics2D.Raycast(startPos, new Vector2(-0.5f, 0.5f), 20, 1 << LayerMask.NameToLayer("dividerLayer"));

		}

		//if ball behind divider line
		if ((hit != null) && (hit.transform != null) &&(hit.collider.tag == "dividerLine"))
		{
			Debug.Log("DIVIDER DED");
			return true;
		}

		return false;
	}

	//applys force based on clients key movement or mobile  joystick

	void serverDoMovement(){
		if (gamePause == false) 
		{
			if(hasJukeJuice==true)
			{
				jukeJuiceMultipler=1.2f;
			}
			else
			{
				jukeJuiceMultipler=1f;
			}
			//if not mobile
			if(onMobile==false)
			{

				if (leftKeyDown == true)
				{
					ballphysics.AddForce(new Vector2(-acceleration*jukeJuiceMultipler,0),ForceMode2D.Force);

				}
				if (rightKeyDown == true)
				{
					ballphysics.AddForce(new Vector2(acceleration*jukeJuiceMultipler,0),ForceMode2D.Force);

				}
				if (downKeyDown == true)
				{
					ballphysics.AddForce(new Vector2(0,-acceleration*jukeJuiceMultipler),ForceMode2D.Force);

				}
				if (upKeyDown == true)
				{
					ballphysics.AddForce(new Vector2(0,acceleration*jukeJuiceMultipler),ForceMode2D.Force);

				}
			}
			else //if mobile
			{
				ballphysics.AddForce(new Vector2(acceleration*jukeJuiceMultipler*mobileMovement.x,acceleration*jukeJuiceMultipler*mobileMovement.y),ForceMode2D.Force);
			}


			//if speed is over limit, make them slow down faster!
			currentSpeedX = ballphysics.velocity.x;  // test current object speed
			if (Mathf.Abs (currentSpeedX) > maxRegSpeed)
				
			{
				ballphysics.AddForce(new Vector2(-currentSpeedX/6f, 0));  // apply opposing force

			}

			currentSpeedY = ballphysics.velocity.y;  // test current object speed
			if (Mathf.Abs (currentSpeedY) > maxRegSpeed)
			{
				ballphysics.AddForce(new Vector2(0,-currentSpeedY/6f));  // apply opposing force

			}
			
		}
	}

	//recieve joystick values from mobile client
	[RPC]
	void mobileInput(Vector2 movement)
	{
		mobileMovement = movement;
		onMobile=true;
		afkTimer = 0;

	}

	//recieve key presses from client and set variables
	[RPC]
	void receiveMovement(int direction)
	{
		onMobile=false;
		//change variable of key to down
		if (direction==0)//left
		{
			leftKeyDown = true;
			afkTimer = 0;
		}
		else if (direction==1)//right
		{
			rightKeyDown = true;
			afkTimer = 0;
		}
		else if (direction ==2)//up
		{
			upKeyDown = true;
			afkTimer = 0;
		}
		else if (direction ==3)//down
		{
			downKeyDown = true;
			afkTimer = 0;
		}
		else if (direction ==4)//space
		{
			isHonking = true;
			transform.parent.uLinkNetworkView().RPC("playerHonkPress", uLink.RPCMode.OthersExceptOwnerBuffered, playerID);
			afkTimer = 0;
		}	
	}

	//recieve key is released from client and set values
	[RPC]
	void releaseKey(int key)
	{
		//chaneg variable of key to up
		if(key==0)
		{
			leftKeyDown = false;
			afkTimer = 0;
		}
		else if(key==1)
		{
			rightKeyDown = false;
			afkTimer = 0;
		}
		else if(key==2)
		{
			upKeyDown = false;
			afkTimer = 0;
		}
		else if(key==3)
		{
			downKeyDown = false;
			afkTimer = 0;
		}
		else if (key ==4)//space
		{
			isHonking = false;
			transform.parent.uLinkNetworkView().RPC("playerHonkRelease", uLink.RPCMode.OthersExceptOwnerBuffered, playerID);
			afkTimer = 0;
		}
	}


	//collisions
	//rolling bomb colliding with another rolling bomb might be iffy right now, tested a bit and looked fine but not sure
	void OnCollisionEnter2D(Collision2D coll)
	{

		//if one ball collides with another not on the same team
		if (coll.gameObject.tag == "Player" && teamColor!=coll.gameObject.GetComponent<serverBallScript>().teamColor)
		{


			if(hasRollingBomb==true)
			{



				//if other ball has rolling bomb, change property of server ball's script so we don't restore velocity and rotation
				if(coll.gameObject.GetComponent<serverBallScript>().hasRollingBomb==true)
				{
					coll.gameObject.GetComponent<serverBallScript>().doubleRolling = true;
				}
				else if (doubleRolling==false) //if other ball does not have rolling bomb, restore velocity and rotation
				{
					ballphysics.velocity = beforeRollingCollideVelocity;
					ballphysics.angularVelocity = beforeRollingCollideRotation;
				}
				
				float radiusOfExplosion = 2.0f;
				float forceOfExplosion = 3.5f;
				//get all balls in the radius of explosion
				Collider2D[] explosionObjects = Physics2D.OverlapCircleAll(coll.contacts[0].point,radiusOfExplosion,1 << LayerMask.NameToLayer("blueBallLayer") | 1 << LayerMask.NameToLayer("redBallLayer"));

				//do explosion on all gameobject within radius
				for(int i=0;i<explosionObjects.Length;i++)
				{
					if(explosionObjects[i].gameObject!=gameObject)
					{
						explosionObjects[i].transform.GetComponent<serverBallScript>().AddExplosionForce(forceOfExplosion, coll.contacts[0].point, radiusOfExplosion);
					}
				}


				doubleRolling=false;
				hasRollingBomb = false;
				flashBall = false;
				transform.parent.uLinkNetworkView().RPC("hidePowerupOnPlayer", uLink.RPCMode.OthersBuffered, 1, playerID, 1);



				
			}
			else{
				bool ballJail = checkIfBallgoesJail();
				//if he is on red team and has blue flag
				if(teamColor==1 && hasBlueFlag==true)
				{
					//apply recoil to other balls
					applyBallPopRecoil(coll.contacts[0].point);

					if (ballJail==true)
					{
						destroyBall(coll.gameObject.transform.parent.uLinkNetworkView().owner.id);
						respawnBallInJail();
					}
					else
					{
						destroyBall(coll.gameObject.transform.parent.uLinkNetworkView().owner.id);
						respawnBall();
					}

				}
				//if on blue team and has red flag
				else if(teamColor==0 && hasRedFlag==true)
				{
					//apply recoil to other balls
					applyBallPopRecoil(coll.contacts[0].point);

					if (ballJail==true)
					{
						destroyBall(coll.gameObject.transform.parent.uLinkNetworkView().owner.id);
						respawnBallInJail();
					}
					else
					{
						destroyBall(coll.gameObject.transform.parent.uLinkNetworkView().owner.id);
						respawnBall();
					}

				}
				else if(ballJail==true)
				{
					//if other team does not have flag
					if(coll.gameObject.GetComponent<serverBallScript>().hasRedFlag==false && coll.gameObject.GetComponent<serverBallScript>().hasBlueFlag==false)
					{
					//apply recoil to other balls
					applyBallPopRecoil(coll.contacts[0].point);

					destroyBall(coll.gameObject.transform.parent.uLinkNetworkView().owner.id);
					respawnBallInJail();
					}
				}
			}
		}
	}

	//applys bounce back to other balls within a certain radius
	void applyBallPopRecoil(Vector2 colliPoint)
	{
		float radiusOfExplosion = 0.85f;
		float forceOfExplosion = 1.5f;
		//get all balls in the radius of explosion
		Collider2D[] explosionObjects = Physics2D.OverlapCircleAll(colliPoint,radiusOfExplosion,1 << LayerMask.NameToLayer("blueBallLayer") | 1 << LayerMask.NameToLayer("redBallLayer"));
		
		//do explosion on all gameobject within radius
		for(int i=0;i<explosionObjects.Length;i++)
		{
			if(explosionObjects[i].gameObject!=gameObject)
			{
				explosionObjects[i].transform.GetComponent<serverBallScript>().AddExplosionForce(forceOfExplosion, colliPoint, radiusOfExplosion);
			}
		}
	}

	//leaving trigger
	//only applicable to buttons right now
	void OnTriggerExit2D(Collider2D zone)
	{
		//if we exit button, play button sound on client
		if(zone.gameObject.layer == LayerMask.NameToLayer("button"))
		{
			transform.parent.uLinkNetworkView().RPC("buttonSoundPlay", uLink.RPCMode.Owner);
		}
	}

	//function that automatically runs after 20 seconds to disable juke juice powerup
	IEnumerator disablePowerupJukeJuice(float duration)
	{
		yield return new WaitForSeconds(duration);
				hasJukeJuice = false;
		transform.parent.FindChild("jukejuicepower").GetComponent<SpriteRenderer>().enabled = false;
		transform.parent.uLinkNetworkView().RPC("hidePowerupOnPlayer", uLink.RPCMode.OthersBuffered, 0, playerID,0);

	}

	//function that automatically runs after 20 seconds to disable rolling bomb powerup
	IEnumerator disablePowerupRollingBomb(float duration)
	{
		yield return new WaitForSeconds(duration);
		hasRollingBomb = false;
		flashBall = false;
		transform.parent.uLinkNetworkView().RPC("hidePowerupOnPlayer", uLink.RPCMode.OthersBuffered, 1, playerID,0);
	}

	//function that controls what to do when ball touches spikes, powerups, buttons, score zones
	void OnTriggerEnter2D(Collider2D zone)
	{
		//if ball hits powerup
		if(zone.gameObject.layer == LayerMask.NameToLayer("powerups"))
		{

			powerupScript gameobjectPowerScript = zone.gameObject.GetComponent<powerupScript>();

			//check to see if more than one ball on top of powerup
			CircleCollider2D theCircleCollider = zone.gameObject.GetComponent<CircleCollider2D>();
			Collider2D[] allCollidersOnPowerup;

			allCollidersOnPowerup  = Physics2D.OverlapCircleAll(new Vector2(theCircleCollider.transform.position.x+theCircleCollider.offset.x, theCircleCollider.transform.position.y+theCircleCollider.offset.y),theCircleCollider.radius,1 << LayerMask.NameToLayer("blueBallLayer") | 1 << LayerMask.NameToLayer("redBallLayer"));


			Debug.Log(allCollidersOnPowerup.Length);
			if(allCollidersOnPowerup.Length==1)
			{
				Debug.Log("Picked up a powerup!");
				if(gameobjectPowerScript.powerupNumber==0)//jukejuice
				{
					hasJukeJuice = true;
					transform.parent.FindChild("jukejuicepower").GetComponent<SpriteRenderer>().enabled = true;
					//stop coroutine first if existing
					StopCoroutine("disablePowerupJukeJuice");
					StartCoroutine(disablePowerupJukeJuice(20f));
				}
				else if(gameobjectPowerScript.powerupNumber==1)//rollingbomb
				{
					hasRollingBomb = true;
					
					currentLerpTime = 0f;
					flashBall = true;
					lerpFlashUp=true;
					
					//stop coroutine first if existing
					StopCoroutine("disablePowerupRollingBomb");
					StartCoroutine(disablePowerupRollingBomb(20f));
				}

				gameobjectPowerScript.GetComponent<powerupScript>().CancelInvoke("checkAndResetRadius");

				transform.parent.uLinkNetworkView().RPC("showPowerupOnPlayer", uLink.RPCMode.OthersBuffered, gameobjectPowerScript.powerupNumber, playerID);
				gameobjectPowerScript.gotPowerup(playerID);

				gameobjectPowerScript.GetComponent<CircleCollider2D>().radius = 0.163f;
				gameobjectPowerScript.GetComponent<CircleCollider2D>().offset = new Vector2(-0.004f,0);
			}
			else if(allCollidersOnPowerup.Length>1)
			{
				Debug.Log("shrunk powerupradius");
				//shrink radius
				gameobjectPowerScript.GetComponent<CircleCollider2D>().radius = 0.006f;
				gameobjectPowerScript.GetComponent<CircleCollider2D>().offset = new Vector2(0,0);

				gameobjectPowerScript.GetComponent<powerupScript>().InvokeRepeating("checkAndResetRadius",0,0.1f);



			}



		}

		//if what we hit is on the button layer play button sound on client
		if(zone.gameObject.layer == LayerMask.NameToLayer("button"))
		{
			transform.parent.uLinkNetworkView().RPC("buttonSoundPlay", uLink.RPCMode.Owner);
		}

		// on this one we're checking if the ball has flag and is in its score zone

		if(zone.transform.parent!=null && zone.transform.parent.tag=="redScoreZone" && hasBlueFlag==true)
		{
			Debug.Log ("Score for Red! Resetting Game.");
			//add one to red score
			GameObject.FindGameObjectWithTag("redScoreText").GetComponent<UnityEngine.UI.Text>().text = (int.Parse(GameObject.FindGameObjectWithTag("redScoreText").GetComponent<UnityEngine.UI.Text>().text)+1).ToString();
			restartGame(1);
		}

		if(zone.transform.parent!=null && zone.transform.parent.tag=="blueScoreZone" && hasRedFlag==true)
		{
			Debug.Log ("Score for Blue! Resetting Game.");
			//add one to blue score
			GameObject.FindGameObjectWithTag("blueScoreText").GetComponent<UnityEngine.UI.Text>().text = (int.Parse(GameObject.FindGameObjectWithTag("blueScoreText").GetComponent<UnityEngine.UI.Text>().text)+1).ToString();

			restartGame(0);
		}

		//if red ball touches blue flag, pick it up!

		if(zone.transform.parent!=null && zone.transform.parent.tag=="redFlag" && teamColor==0 && zone.transform.parent.GetComponentInChildren<MeshRenderer>().enabled==true)
		{
			player.SetLocalData(4);
			hasRedFlag=true;
			hasFlag=true;

			transform.parent.uLinkNetworkView().RPC("showFlagOnPlayer", uLink.RPCMode.Buffered, playerID, teamColor);
			//send rpc directly to player so even if scope is disabled, they will receive it. 
			for(int i=0;i<uLink.Network.connections.Length;i++)
			{
				transform.parent.uLinkNetworkView().RPC("showFlagOnPlayer",uLink.Network.connections[i], playerID, teamColor);
			}

			Debug.Log ("Picked up Red flag");

			zone.transform.parent.GetComponentInChildren<MeshRenderer>().enabled=false;
			transform.parent.FindChild("redFlag").GetComponent<SpriteRenderer>().enabled = true;
			
		}
		//if blue ball toches red flag, pick it up
		if(zone.transform.parent!=null && zone.transform.parent.tag=="blueFlag" && teamColor==1 && zone.transform.parent.GetComponentInChildren<MeshRenderer>().enabled==true)
		{
			player.SetLocalData(3);
			hasBlueFlag=true;
			hasFlag=true;

			transform.parent.uLinkNetworkView().RPC("showFlagOnPlayer", uLink.RPCMode.Buffered, playerID, teamColor);
			//send rpc directly to player so even if scope is disabled, they will receive it
			for(int i=0;i<uLink.Network.connections.Length;i++)
			{
				transform.parent.uLinkNetworkView().RPC("showFlagOnPlayer",uLink.Network.connections[i], playerID, teamColor);
			}
			Debug.Log ("Picked up blue flag");

			zone.transform.parent.GetComponentInChildren<MeshRenderer>().enabled=false;
			transform.parent.FindChild("blueFlag").GetComponent<SpriteRenderer>().enabled = true;

		}
		//if red ball not in jail hits blue jail button
		if(zone.transform.parent!=null && zone.transform.parent.tag=="blueJail" && teamColor==1)
		{
			transform.parent.uLinkNetworkView().RPC("buttonSoundPlay", uLink.RPCMode.Owner);
			serverBallScript[] allServerScripts = FindObjectsOfType(typeof(serverBallScript)) as serverBallScript[];
			int i;
			for(i=0;i<allServerScripts.Length;i++)
			{
				//check all server scripts if a ball is in jail
				allServerScripts[i].respawnBallIfInJailRed();
			}

		}

		//if blue ball not in jail hits red jail button
		if(zone.transform.parent!=null && zone.transform.parent.tag=="redJail" && teamColor==0)
		{
			serverBallScript[] allServerScripts = FindObjectsOfType(typeof(serverBallScript)) as serverBallScript[];
			int i;
			for(i=0;i<allServerScripts.Length;i++)
			{
				//check all server scripts if ball is in jail
				allServerScripts[i].respawnBallIfInJailBlue();
			}

		}

		//if ball hits a boost
		//logic to reload boost is on boostScript attached to the boosts
		if(zone.transform.parent!=null && zone.transform.parent.tag=="boosts" && zone.transform.parent.GetComponentInChildren<MeshRenderer>().enabled==true)
		{
			//play sound on player that hit boost
			transform.parent.uLinkNetworkView().RPC("boostSoundPlay", uLink.RPCMode.Owner, transform.position);


			//qeue reset boost. can't be here cause if the player dissconnects, it won't run
			boostScript daBoost = zone.transform.parent.GetComponent<boostScript>();
			daBoost.resetBoost();





			//add force on normalized velocitys
		
			ballphysics.velocity = new Vector2(ballphysics.velocity.normalized.x*boostSpeed, ballphysics.velocity.normalized.y*boostSpeed);




		}


		//if ball hits a stars --- meant spike but realized this when I was already too far ahead in
		if(zone.transform.parent!=null && zone.transform.parent.tag=="stars")
		{
			//uLink.NetworkView.Destroy(transform.parent.gameObject);

			//apply recoil to other balls
			applyBallPopRecoil((Vector2)transform.position);

			destroyBall();
			respawnBall();
		}

	}


	//function to respawn after waiting two secnods
	//picks random area to spawn and sends to clients and proxy to show ghost ball
	IEnumerator respawnAfterWait(float duration)
	{
		Vector3 startPosition;
		if(teamColor==1)
		{
			//start position set to hardcoded value + random value right now. if custom tile system is built, the ball can be put into a tile and an object can be designiated as the start position 
			//but since theres no tile system, we cant spawn inside tile sprites
			float randomValueX = UnityEngine.Random.Range(-3.3f,1);
			float randomValueY = UnityEngine.Random.Range(-0.4f,0.1f);
			startPosition = new Vector3(-5+randomValueX, 7.2f+randomValueY, 0);
			if(isInJail==true)
			{
				//ugly hardcoded values
				randomValueX = UnityEngine.Random.Range(0f,1.1f);
				randomValueY = UnityEngine.Random.Range(-0.3f,0f);
				startPosition = new Vector3(6.606f+randomValueX, -2.418f+randomValueY, 0);
			}
		}
		else
		{
			//ugly hardcoded values
			float randomValueX = UnityEngine.Random.Range(-1f,3.3f);
			float randomValueY = UnityEngine.Random.Range(-0.1f,0.4f);
			startPosition = new Vector3(5+randomValueX, -7.2f+randomValueY, 0);
			if(isInJail==true)
			{
				//ugly hardcoded values
				randomValueX = UnityEngine.Random.Range(-1.1f,0f);
				randomValueY = UnityEngine.Random.Range(-0f,0.3f);
				startPosition = new Vector3(-6.606f+randomValueX, 2.418f+randomValueY, 0);
			}
			
		}

		showSpawnLocationTransperant(startPosition);

		yield return new WaitForSeconds(duration);
		//serverInstantiate.Instantiate(player, teamColor, isInJail, playerName);
		//uLink.NetworkView.Destroy(transform.parent.gameObject);

		transform.parent.uLinkNetworkView().RPC("respawnBall", uLink.RPCMode.Others, playerID);
		transform.position=startPosition;
		transform.GetComponent<Rigidbody2D>().velocity=new Vector2(0,0);
		transform.GetComponent<Rigidbody2D>().angularVelocity = 0;
		transform.rotation = new Quaternion(0,0,0,0);
		transform.GetComponent<CircleCollider2D>().enabled=true;
		transform.GetComponent<SpriteRenderer>().enabled=true;

	}


	//check if we're in jail and if so respawn if someone on our team pressed the button

	void respawnBallIfInJailRed()
	{
		Debug.Log("Blue Jail Button Triggered!");
		Debug.Log(isInJail);
		if(isInJail==true && teamColor==1)
		{
			isInJail = false;
			destroyBall();
			respawnBall();
		}
	}
	//check if we're in jail and if so respawn
	void respawnBallIfInJailBlue()
	{
		Debug.Log("Red Jail Button Triggered!");
		if(isInJail==true && teamColor==0)
		{
			isInJail = false;
			destroyBall();
			respawnBall();
		}
	}

	//if we died over the mid line
	void respawnBallInJail()
	{
		isInJail=true;
		StartCoroutine(respawnAfterWait(2));


	}

	//show where ball will be spawning
	void showSpawnLocationTransperant(Vector3 spawnLocation)
	{
		GameObject spawnTransperant;
		if(teamColor==0)
		{
			spawnTransperant = (GameObject)Instantiate((GameObject)Resources.Load("transperantBlueBall", typeof(GameObject)),spawnLocation, Quaternion.identity);
		}
		else
		{
			spawnTransperant= (GameObject)Instantiate((GameObject)Resources.Load("transperantRedBall", typeof(GameObject)),spawnLocation, Quaternion.identity);
		}

		//send to every player regardless if within scope or not
		for(int i=0;i<uLink.Network.connections.Length;i++)
		{
			transform.parent.uLinkNetworkView().RPC("showSpawnLocation", uLink.Network.connections[i], spawnLocation, teamColor, playerID);
		}


		ballphysics.isKinematic=false;


		spawnTransperant.GetComponent<SpriteRenderer>().sortingOrder=5;
		StartCoroutine(removeSpawnLocationTransperant(2,spawnTransperant));
	}

	//after we spawned get rid of the ghost ball

	IEnumerator removeSpawnLocationTransperant(float duration, GameObject spawn)
	{
		yield return new WaitForSeconds(duration);
		Destroy(spawn);
		
	}

	void respawnBall()
	{

		StartCoroutine(respawnAfterWait(2));
	}

	//function to "destroy" ball. does not really destroy, just makes hidden and makes rigidbody2d kinematic
	void destroyBall(int collidingID = -1)
	{

		if(resetFlag(collidingID) == false)
		{
			transform.parent.uLinkNetworkView().RPC("popSoundPlay", uLink.RPCMode.Others, collidingID, playerID);
		}
		//uLink.NetworkView.Destroy(transform.GetComponent<CircleCollider2D>());
		//uLink.NetworkView.Destroy(transform.GetComponent<SpriteRenderer>());
		transform.GetComponent<CircleCollider2D>().enabled=false;
		transform.GetComponent<SpriteRenderer>().enabled=false;

		//show splat regardless of scope
		for(int i=0;i<uLink.Network.connections.Length;i++)
		{
			transform.parent.uLinkNetworkView().RPC("showSplatOnLocation", uLink.Network.connections[i], transform.position);
		}

		transform.parent.uLinkNetworkView().RPC("destroyPartial", uLink.RPCMode.Others, playerID);

		ballphysics.isKinematic=true;
		ballphysics.velocity = Vector2.zero;

	}

	//hides flag sprite on ball and reenables flag sprite on map
	bool resetFlag(int collidingID = -1)
	{
		if(hasRedFlag==true)
		{
			player.SetLocalData(0);
			hasRedFlag = false;
			hasFlag=false;
			GameObject.FindGameObjectWithTag("redFlag").GetComponentInChildren<MeshRenderer>().enabled=true;
			gameObject.transform.parent.GetChild(1).GetComponentInChildren<SpriteRenderer>().enabled=false;

			transform.parent.uLinkNetworkView().RPC("clientResetFlag", uLink.RPCMode.Buffered, 1, collidingID, playerID);
			for(int i=0;i<uLink.Network.connections.Length;i++)
			{
				transform.parent.uLinkNetworkView().RPC("clientResetFlag", uLink.Network.connections[i], 1, collidingID, playerID);
			}

			return true;
		}
		if(hasBlueFlag==true)
		{
			player.SetLocalData(1);
			hasBlueFlag = false;
			hasFlag =false;
			GameObject.FindGameObjectWithTag("blueFlag").GetComponentInChildren<MeshRenderer>().enabled=true;
			gameObject.transform.parent.GetChild(1).GetComponentInChildren<SpriteRenderer>().enabled=false;
			transform.parent.uLinkNetworkView().RPC("clientResetFlag", uLink.RPCMode.Buffered, 0, collidingID, playerID);
			for(int i=0;i<uLink.Network.connections.Length;i++)
			{
				transform.parent.uLinkNetworkView().RPC("clientResetFlag", uLink.Network.connections[i], 0, collidingID, playerID);
			}

			return true;
		}
		return false;
	}

	//tells clients that the end ended
	public void sendEndGame(int teamWonInt)
	{
		//send rpc to play sound on clients
		transform.parent.uLinkNetworkView().RPC("endGame", uLink.RPCMode.Owner, teamWonInt);
	}


	//after 10 seconds since game ended, this function will run
	//makes balls unable to move and reloads map from scratch
	public void restartGame(int teamWonInt)
	{


		//make all server scripts run function pausegame()
		serverBallScript[] allServerScripts = FindObjectsOfType(typeof(serverBallScript)) as serverBallScript[];
		int i;
		for(i=0;i<allServerScripts.Length;i++)
		{
			//psend endgame rpc to clients
			allServerScripts[i].sendEndGame(teamWonInt);
			//pause game on all server scripts
			allServerScripts[i].pauseGameNow();
		}


		//we need to run the function that restarts server after 10 seconds on main server script b/c if player dissconnects it won't run
		serverStart serverScript = GameObject.FindGameObjectWithTag("server").GetComponent<serverStart>();
		//no longer paused
		GameObject.FindGameObjectWithTag("server").GetComponent<serverStart>().gamePaused=false;
		serverScript.restartGame();
	}





	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo msg)
	{
		uLink.NetworkPlayer myPlayer = uLink.Network.player;
		myPlayer.localData = this;



		teamColor = msg.networkView.initialData.Read<int>();
		isInJail = msg.networkView.initialData.Read<bool>();
		playerName = msg.networkView.initialData.Read<string>();

		Debug.Log(teamColor+playerName+isInJail);


		player = msg.networkView.owner;
		playerID = msg.networkView.owner.id;

		//set label text
		gameObject.GetComponent<uLinkObjectLabel_custom>().instantiatedLabel.text = playerName;

		//say to everyone that we joined!
		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, playerName+" has joined the game.");

		transform.parent.GetChild(4).GetComponent<BoxCollider2D>().enabled=true;

		//tell all other balls on server to not transmit our state
		//if we are in AOI of another ball, they will reverse this

		setNetworkScope[] allNetworkScopeScripts = FindObjectsOfType(typeof(setNetworkScope)) as setNetworkScope[];
		for(int i=0;i<allNetworkScopeScripts.Length;i++)
		{
			for(int g=0;g<allNetworkScopeScripts.Length;g++)
			{
				allNetworkScopeScripts[i].hidePlayer(allNetworkScopeScripts[g].transform.parent.GetComponentInChildren<serverBallScript>().player, allNetworkScopeScripts[g].transform.parent.GetComponentInChildren<serverBallScript>().playerID);
			}

		}

	}

	public void AddExplosionForce (float expForce, Vector2 expPosition, float expRadius)
	{
		
		Debug.Log("explosion");
		Rigidbody2D body = GetComponent<Rigidbody2D>();

		Vector2 dir = (Vector2)body.transform.position - expPosition;

		float calc = 1 - (dir.magnitude / expRadius);
		if (calc <= 0) {
			calc = 0;		
		}

		body.AddForce (dir.normalized * expForce * calc,ForceMode2D.Impulse);
	}
	//imediately pause game
	public void pauseGameNow()
	{
		beforePauseVelocity = ballphysics.velocity;
		beforePauseRotationVelocity = ballphysics.angularVelocity;
		
		ballphysics.velocity = new Vector2(0,0);
		ballphysics.angularVelocity = 0;

		gamePause = true;
		GameObject.FindGameObjectWithTag("server").GetComponent<serverStart>().gamePaused=true;
		processOfBeingPaused=false;

		
		
	}

	public void pauseGame()
	{
		beforePauseVelocity = ballphysics.velocity;
		beforePauseRotationVelocity = ballphysics.angularVelocity;
		
		ballphysics.velocity = new Vector2(0,0);
		ballphysics.angularVelocity = 0;



	}
	IEnumerator sendPausingChat(float duration)
	{
		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Pausing In 3");
		yield return new WaitForSeconds(duration);
		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Pausing In 2");
		yield return new WaitForSeconds(duration);
		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Pausing In 1");
		yield return new WaitForSeconds(duration);
		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Game Is Paused");

		GameObject.FindGameObjectWithTag("timer").GetComponent<timerScript>().pauseTimer();
	}

	IEnumerator pause(float duration)
	{

		transform.parent.uLinkNetworkView().RPC("playPingSound", uLink.RPCMode.Owner);
		yield return new WaitForSeconds(duration);
		transform.parent.uLinkNetworkView().RPC("playPingSound", uLink.RPCMode.Owner);
		yield return new WaitForSeconds(duration);
		transform.parent.uLinkNetworkView().RPC("playPingSound", uLink.RPCMode.Owner);
		yield return new WaitForSeconds(duration);

		transform.parent.uLinkNetworkView().RPC("pauseGame", uLink.RPCMode.Owner);
		pauseGame();

		GameObject.FindGameObjectWithTag("server").GetComponent<serverStart>().gamePaused=true;
		gamePause = true;
		
		Time.timeScale=0;

		processOfBeingPaused=false;

	}

	IEnumerator sendUnPausingChat(float duration)
	{
		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Unpausing In 3");
		//wait for seconds that works with unscaled time
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < duration) {
			yield return null;
		}

		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Unpausing In 2");
		//wait for seconds that works with unscaled time
		startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < duration) {
			yield return null;
		}

		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Unpausing In 1");
		//wait for seconds that works with unscaled time
		startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < duration) {
			yield return null;
		}

		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, "Game Resumed");

		GameObject.FindGameObjectWithTag("timer").GetComponent<timerScript>().unPauseTimer();
	}

	IEnumerator unPause(float duration)
	{

		transform.parent.uLinkNetworkView().RPC("playPingSound", uLink.RPCMode.Owner);
		//wait for seconds that works with unscaled time
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < duration) {
			yield return null;
		
		}
		transform.parent.uLinkNetworkView().RPC("playPingSound", uLink.RPCMode.Owner);

		//wait for seconds that works with unscaled time
		startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < duration) {
			yield return null;
		
		}
		transform.parent.uLinkNetworkView().RPC("playPingSound", uLink.RPCMode.Owner);

		//wait for seconds that works with unscaled time
		startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < duration) {
			yield return null;
		
		}

		transform.parent.uLinkNetworkView().RPC("unPauseGame", uLink.RPCMode.Owner);

		Time.timeScale=1;
		GameObject.FindGameObjectWithTag("server").GetComponent<serverStart>().gamePaused=false;
		gamePause=false;
		ballphysics.velocity = beforePauseVelocity;
		ballphysics.angularVelocity = beforePauseRotationVelocity;
		processOfBeingPaused = false;
	

	}

	public void showPauseScreen()
	{
		processOfBeingPaused= true;
		StartCoroutine(pause(1f));
	}
	
	void unPauseGame()
	{
		processOfBeingPaused=true;
		StartCoroutine(unPause(1f));
	}
	//if client pressed f9
	[RPC]
	void clientPressedF9()
	{


		if (GameObject.FindGameObjectWithTag("server").GetComponent<serverStart>().allowGamePauses==true && maxPausesPerPlayer>=0 && processOfBeingPaused==false)
		{
			Debug.Log("Pressed Pause Button");
			maxPausesPerPlayer--;
			if(gamePause==false)
			{
				StartCoroutine(sendPausingChat(1f));

				//make all server scripts run function pauseGame and showPauseScreen()
				serverBallScript[] allServerScripts = FindObjectsOfType(typeof(serverBallScript)) as serverBallScript[];
				int i;
				for(i=0;i<allServerScripts.Length;i++)
				{
					//show pause screen all players
					allServerScripts[i].showPauseScreen();
				}
			}
			else
			{
				StartCoroutine(sendUnPausingChat(1f));

				//unpause game on all balls
				serverBallScript[] allServerScripts = FindObjectsOfType(typeof(serverBallScript)) as serverBallScript[];
				int i;
				for(i=0;i<allServerScripts.Length;i++)
				{
					//pause game on all server scripts
					allServerScripts[i].unPauseGame();
				}
			}
		}

	}




}
