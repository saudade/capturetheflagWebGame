using UnityEngine;
using System.Collections;
using uLink;
using System.Collections.Generic;


public class clientBallScript : uLink.MonoBehaviour {

	public AudioClip boostSound;
	public AudioClip popSound;
	public AudioClip friendlyFlagDropSound;
	public AudioClip flagDropSound;
	public AudioClip friendlyFlagPickUpSound;
	public AudioClip flagPickUpSound;
	public AudioClip winSound;
	public AudioClip loseSound;
	public AudioClip buttonSound;
	public AudioClip bombExploded;
	public AudioClip pingSound;
	public AudioClip honkSound;
	
	public AudioSource audioPlayer;

	public GameObject splatPrefab;

	public Rigidbody2D ballphysics;

	bool hasJukeJuice = false;

	Transform flagSprite;



	private float lastBoostTime;
	private float lastBombTime;
	
	[HideInInspector]
	int teamColor;
	string playerName;
	bool isInJail;
	

	float lerpTime = 0.35f;
	float currentLerpTime;

	bool flashBall=false;
	bool lerpFlashUp=true;


	private Animator ballAnimation;	


	public Sprite[] spriteSplats;


	private CNJoystick MovementJoystick;



	// Use this for initialization
	void Start () {
		#if MOBILE_INPUT

		MovementJoystick = GameObject.FindWithTag("joystick").GetComponent<CNJoystick>();
		MovementJoystick.ControllerMovedEvent += mobileSendMovement;
		MovementJoystick.FingerLiftedEvent += mobileLiftedFinger;
		GameObject.FindWithTag("joystick").GetComponentInParent<Camera>().enabled=true;

		//make camera size bigger
		Camera.main.orthographicSize=3.5f;


		#endif

		GameObject.FindGameObjectWithTag("gameEndText").GetComponent<UnityEngine.UI.Text>().enabled=false;
		audioPlayer = transform.GetChild(0).GetComponent<AudioSource>();
		ballAnimation = transform.GetChild(0).GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
			//we have event for joystick movement to function mobileSendMovement()

			sendMovement();


		//clientPrediction();

		rollingBombFlash();
	}

	void LateUpdate()
	{
		setJukeJuiceToBall();
		setHonkingSpriteToBall();
	}
	void setHonkingSpriteToBall()
	{
			transform.GetChild(3).position=new Vector3(transform.GetChild(0).position.x+0.015f	,transform.GetChild(0).position.y+0.28f, transform.GetChild(3).position.z);


	}

	void uLink_OnSerializeNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.isWriting==false)
		{
			// This is performed on deserialization.

			ballphysics.transform.position = stream.Read<Vector3>();
			ballphysics.transform.rotation = stream.Read<Quaternion>();
			ballphysics.velocity = stream.Read<Vector2>();
			ballphysics.angularVelocity = stream.Read<float>();



		}
	}

	//use this for client prediction when ball with flag scores to stop the stutter associated with high velocity
	void OnTriggerEnter2D(Collider2D zone)
	{
		if(zone.transform.parent!=null && zone.transform.parent.tag=="redScoreZone" && transform.GetChild(1).GetComponent<SpriteRenderer>().enabled==true)
		{
			ballphysics.velocity = new Vector2(0,0);
		}
		
		if(zone.transform.parent!=null && zone.transform.parent.tag=="blueScoreZone" && transform.GetChild(1).GetComponent<SpriteRenderer>().enabled==true)
		{
			ballphysics.velocity = new Vector2(0,0);
		}
	}

	void uLink_OnPreBufferedRPCs(uLink.NetworkBufferedRPC[] bufferedArray) 
	{

	}

	void setJukeJuiceToBall()
	{

		transform.FindChild("jukejuicepower").position = new Vector3(transform.GetChild(0).position.x-0.111f	,transform.GetChild(0).position.y-0.141f, transform.GetChild(0).position.z);

	}
	//mobile input to server
	private void mobileSendMovement(Vector3 movement, CNAbstractController cnAbstractController)
	{
		//movement.Normalize();


		//translating joystick input to arrow input on server -- high bandwidth non optimizaed usage way
		//testing found it was really hard and awkward to control ball this way...
		//decided to send raw inputs to server and have server apply force*input.x, force*input.y
		//less fair this way but no one would play mobile with these awful controls

		/*
		transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 0);
		transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 1);
		transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 2);
		transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 3);
		if(movement.x>0.3)
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 1);
		}
		else if(movement.x < -0.3)
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 0);
		}
		if(movement.y>0.3)
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 2);
		}
		else if(movement.y < -0.3)
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 3);
		}
		*/

		transform.uLinkNetworkView().RPC("mobileInput", uLink.NetworkPlayer.server, (Vector2)movement.normalized);


	}
	private void mobileLiftedFinger(CNAbstractController cnAbstractController)
	{

		transform.uLinkNetworkView().RPC("mobileInput", uLink.NetworkPlayer.server, new Vector2(0,0));
	}


	//send arrow keys to server
	void sendMovement(){

		if (Input.GetKeyUp(KeyCode.LeftArrow))
		{

			transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 0);
		}
		if (Input.GetKeyUp(KeyCode.RightArrow))
		{
			transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 1);
		}
		if (Input.GetKeyUp(KeyCode.UpArrow))
		{
			transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 2);
		}
		if (Input.GetKeyUp(KeyCode.DownArrow))
		{
			transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 3);
		}
		if (Input.GetKeyUp(KeyCode.Space)|| Input.GetKeyUp(KeyCode.F))
		{
			transform.uLinkNetworkView().RPC("releaseKey", uLink.NetworkPlayer.server, 4);

			transform.GetChild(3).GetComponent<SpriteRenderer>().enabled=false;
			audioPlayer.Stop();
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 0);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 1);
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 2);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 3);
		}
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F))
		{

			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 4);


			transform.GetChild(3).GetComponent<SpriteRenderer>().enabled=true;
			audioPlayer.Play();
		}

		if(Input.GetKeyDown(KeyCode.F9))
		{
			transform.uLinkNetworkView().RPC("clientPressedF9", uLink.NetworkPlayer.server,1);
		}

		

	}

	void clientPrediction()
	{


		/*if (Input.GetKey(KeyCode.LeftArrow))
		{
			ballphysics.AddForce(new Vector2(-1,0),ForceMode2D.Force);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			ballphysics.AddForce(new Vector2(1,0),ForceMode2D.Force);
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			ballphysics.AddForce(new Vector2(0,1),ForceMode2D.Force);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			ballphysics.AddForce(new Vector2(0,-1),ForceMode2D.Force);
		}*/



	}


	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo msg)
	{



		teamColor = msg.networkView.initialData.Read<int>();
		isInJail = msg.networkView.initialData.Read<bool>();
		//playerName = msg.networkView.initialData.Read<string>();



		//Debug.Log(playerName);
		Debug.Log(teamColor);
	

		//if key is being held down send it
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 0);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 1);
		}
		if (Input.GetKey(KeyCode.UpArrow))
		{
			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 2);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			transform.uLinkNetworkView().RPC("receiveMovement", uLink.NetworkPlayer.server, 3);
		}


	}

	//RPC, server sends command to run this function when a ball picks up flag. shows the flag sprite on ball
	[RPC]
	void showFlagOnPlayer(int id, int teamnumber)
	{

		if(uLink.Network.player.id == id)
		{
			if (teamnumber==1)
			{
				transform.FindChild("blueFlag").GetComponent<SpriteRenderer>().enabled = true;
			}
			else
			{
				transform.FindChild("redFlag").GetComponent<SpriteRenderer>().enabled = true;
			}
		}
		if (teamnumber==1)
		{
			GameObject.FindGameObjectWithTag("blueFlag").GetComponentInChildren<MeshRenderer>().enabled=false;
			GameObject.FindGameObjectWithTag("uiBlueFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=true;
			if(teamColor==1)
			{
				PlayAtPoint(friendlyFlagPickUpSound, transform.GetChild(0).position);
			}
			else
			{
				PlayAtPoint(flagPickUpSound,transform.GetChild(0).position);
			}
		}
		else
		{
			GameObject.FindGameObjectWithTag("redFlag").GetComponentInChildren<MeshRenderer>().enabled=false;
			GameObject.FindGameObjectWithTag("uiRedFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=true;

			if(teamColor==1)
			{
				PlayAtPoint(flagPickUpSound,transform.GetChild(0).position);
			}
			else
			{
				PlayAtPoint(friendlyFlagPickUpSound,transform.GetChild(0).position);
			}
		}

	}
	[RPC]
	public void clientResetFlag(int flagColor, int id1, int id2)
	{
		if(uLink.Network.player.id == id1 || uLink.Network.player.id ==id2)
		{
			PlayAtPoint(popSound,transform.GetChild(0).position);
		}

		if(flagColor==1)
		{
			GameObject.FindGameObjectWithTag("redFlag").GetComponentInChildren<MeshRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag("uiRedFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=false;
			if(teamColor==0)
			{
				PlayAtPoint(friendlyFlagDropSound,transform.GetChild(0).position);
			}
			else
			{
				PlayAtPoint(flagDropSound,transform.GetChild(0).position);
			}
		}
		else if (flagColor==0)
		{
			GameObject.FindGameObjectWithTag("blueFlag").GetComponentInChildren<MeshRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag("uiBlueFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=false;
			if(teamColor==1)
			{
				PlayAtPoint(friendlyFlagDropSound,transform.GetChild(0).position);
			}
			else
			{
				PlayAtPoint(flagDropSound,transform.GetChild(0).position);
			}
		}


	}

	[RPC]
	void destroyPartial(int id)
	{

		if(uLink.Network.player.id == id)
		{
			transform.GetChild(0).GetComponent<CircleCollider2D>().enabled=false;

			transform.GetChild(0).GetComponent<cameraFollowBallScript>().follow=false;
			transform.GetChild(2).GetComponent<SpriteRenderer>().enabled=false;
			transform.GetChild(1).GetComponent<SpriteRenderer>().enabled=false;

			ballAnimation.SetBool("playDeadAnimation",true);


			transform.GetChild(0).GetComponent<uLinkObjectLabel_custom>().instantiatedLabel.enabled=false;
		}
	}

	[RPC]
	void respawnBall(int id)
	{
		
		if(uLink.Network.player.id == id)
		{
			transform.GetChild(0).GetComponent<Rigidbody2D>().velocity=new Vector2(0,0);
			transform.GetChild(0).GetComponent<Rigidbody2D>().angularVelocity = 0;
			transform.GetChild(0).rotation = new Quaternion(0,0,0,0);
			transform.GetChild(0).GetComponent<CircleCollider2D>().enabled=true;
			transform.GetChild(0).GetComponent<SpriteRenderer>().enabled=true;
			transform.GetChild(0).GetComponent<cameraFollowBallScript>().follow=true;

			ballAnimation.SetBool("playDeadAnimation",false);


			if(hasJukeJuice==true)
			{
				transform.GetChild(2).GetComponent<SpriteRenderer>().enabled=true;
			}

			transform.GetChild(0).GetComponent<uLinkObjectLabel_custom>().instantiatedLabel.enabled=true;
		}
	}

	[RPC]
	void showSpawnLocation(Vector3 spawnLocation, int colorOfTeam, int id)
	{

		if(uLink.Network.player.id == id)
		{
			Debug.Log("Transperant");
			GameObject spawnTransperant;
			if(colorOfTeam==0)
			{
				spawnTransperant = (GameObject)Instantiate((GameObject)Resources.Load("transperantBlueBall", typeof(GameObject)),spawnLocation, Quaternion.identity);
			}
			else
			{
				spawnTransperant= (GameObject)Instantiate((GameObject)Resources.Load("transperantRedBall", typeof(GameObject)),spawnLocation, Quaternion.identity);
			}
			spawnTransperant.GetComponent<SpriteRenderer>().sortingOrder=5;
			StartCoroutine(removeSpawnLocationTransperant(2,spawnTransperant));
		}
	}
	
	IEnumerator removeSpawnLocationTransperant(float duration, GameObject spawn)
	{
		yield return new WaitForSeconds(duration);
		Destroy(spawn);
		
	}

	[RPC]
	void boostSoundPlay(Vector3 positionSound)
	{
		//dont play boost sounds very quickly because it hurts my ears if boosts are too close
		if(Time.time - lastBoostTime>=0.10f)
		{
			PlayAtPoint(boostSound,positionSound);

			
			lastBoostTime=Time.time;
		}

	}



	public void PlayAtPoint(AudioClip clip, Vector3 position, float volume = 1, float pitch = 1) {
		AudioSource availableAudioSource = new GameObject().AddComponent<AudioSource>();

		availableAudioSource.clip = clip;
		availableAudioSource.transform.position = position;
		availableAudioSource.volume = volume;
		availableAudioSource.pitch = pitch;
		availableAudioSource.spatialBlend = audioPlayer.spatialBlend;
		availableAudioSource.dopplerLevel = audioPlayer.dopplerLevel;
		availableAudioSource.rolloffMode = audioPlayer.rolloffMode;
		availableAudioSource.maxDistance = audioPlayer.maxDistance;
		availableAudioSource.minDistance = audioPlayer.minDistance;
		availableAudioSource.panStereo=0;


		//3d sound sounded weird so forced it 2d again
		availableAudioSource.spatialBlend = 0;

		availableAudioSource.spread = audioPlayer.spread;
		availableAudioSource.Play();       


		Destroy(availableAudioSource.gameObject, clip.length / pitch);
	}
	
	[RPC]
	void buttonSoundPlay()
	{
		PlayAtPoint(buttonSound,transform.GetChild(0).position);
		
	}
	[RPC]
	void popSoundPlay(int id1, int id2)
	{
		if(uLink.Network.player.id == id1 || uLink.Network.player.id ==id2)
		{
			PlayAtPoint(popSound,transform.GetChild(0).position);
		}
		
	}


	[RPC]
	void endGame(int teamWonInt)
	{
		GameObject.FindGameObjectWithTag("gameEndText").GetComponent<UnityEngine.UI.Text>().enabled=true;

		if(teamWonInt==1)//if red won
		{
			if(teamColor==1)
			{
				PlayAtPoint(winSound,transform.GetChild(0).position);

			}
			else
			{
				PlayAtPoint(loseSound,transform.GetChild(0).position);
			}
			GameObject.FindGameObjectWithTag("redScoreText").GetComponent<UnityEngine.UI.Text>().text = "1";
			GameObject.FindGameObjectWithTag("gameEndText").GetComponent<UnityEngine.UI.Text>().text = "Red Team Wins!";
		}
		else if(teamWonInt==0)//if blue won
		{
			if(teamColor==1)
			{
				PlayAtPoint(loseSound,transform.GetChild(0).position);
			}
			else
			{
				PlayAtPoint(winSound,transform.GetChild(0).position);
			}
			GameObject.FindGameObjectWithTag("blueScoreText").GetComponent<UnityEngine.UI.Text>().text = "1";
			GameObject.FindGameObjectWithTag("gameEndText").GetComponent<UnityEngine.UI.Text>().text = "Blue Team Wins!";
		}
		else //tie - everyone loses
		{
			PlayAtPoint(loseSound,transform.GetChild(0).position);
			GameObject.FindGameObjectWithTag("gameEndText").GetComponent<UnityEngine.UI.Text>().text = "Nobody Wins!";
		}

		
	}

	[RPC]
	void hidePowerupOnPlayer(int powerupNumber, int playerID, int exploded)
	{
		if(uLink.Network.player.id == playerID)
		{
			if (powerupNumber==0)
			{
				transform.FindChild("jukejuicepower").GetComponent<SpriteRenderer>().enabled = false;
				hasJukeJuice=false;
			}
			else if (powerupNumber==1)
			{
				if(exploded==1)
				{
					//dont play bomb sounds very quickly because it hurts my ears if bombs are too close
					if(Time.time - lastBombTime>=0.10f)
					{
						PlayAtPoint(bombExploded,transform.GetChild(0).position);
						lastBombTime=Time.time;
					}
				}
				flashBall = false;
				transform.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount",0);
			}
		}
	}

	//put splat on location
	[RPC]
	void showSplatOnLocation(Vector3 splatPosition)
	{
		int randomInt;
		int minRan;
		int maxRan;
		
		if(teamColor==1)//red
		{
			minRan = 0;//inclusive
			maxRan = 7;//exclusive
		}
		else //blue
		{
			minRan = 7;//inclusive
			maxRan = 14;//exclusive
		}



		randomInt = Random.Range(minRan, maxRan);

		//red splats are 1-6, blue splats are 6-13

		Sprite splat = spriteSplats[randomInt];

		GameObject splatInstance = (GameObject)Instantiate(splatPrefab, splatPosition, Quaternion.identity);

		splatInstance.transform.localScale=new Vector3(0.75f,0.75f,1);

		splatInstance.GetComponent<SpriteRenderer>().sprite = splat;
		splatInstance.GetComponent<SpriteRenderer>().sortingOrder=3;


	}

	[RPC]
	void showPowerupOnPlayer(int powerupNumber, int playerID)
	{

		if(uLink.Network.player.id == playerID)
		{
			if (powerupNumber==0)
			{
				hasJukeJuice = true;
				transform.FindChild("jukejuicepower").GetComponent<SpriteRenderer>().enabled = true;
			}
			else if (powerupNumber==1)
			{
				currentLerpTime = 0f;
				flashBall = true;
				lerpFlashUp=true;

			}
		}
	}

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
				transform.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount",Mathf.Lerp(0f, 0.27f, perc));
				if (perc>=1f)
				{
					currentLerpTime = 0f;
					lerpFlashUp=false;
				}
			}
			else if(lerpFlashUp==false)
			{

				transform.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount",Mathf.Lerp(0.27f, 0f, perc));
				if (perc>=1f)
				{
					currentLerpTime = 0f;
					lerpFlashUp = true;
				}
			}
		}

	}

	[RPC]
	void playPingSound()
	{
		PlayAtPoint(pingSound,transform.GetChild(0).position);
	}

	[RPC]
	void pauseGame()
	{
		StartCoroutine(showPauseText(2f));
		Camera.main.GetComponent<CameraFade>().StartFade(new Color(0,0,0,0.25f),2f);
		Time.timeScale=0;


	}

	IEnumerator showPauseText(float duration)
	{
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < duration) {
			yield return null;
			
		}

		GameObject.FindGameObjectWithTag("gamePausedText").GetComponent<UnityEngine.UI.Text>().enabled=true;
	}

	[RPC]
	void unPauseGame()
	{
		GameObject.FindGameObjectWithTag("gamePausedText").GetComponent<UnityEngine.UI.Text>().enabled=false;
		Camera.main.GetComponent<CameraFade>().StartFade(new Color(0,0,0,0f),0f);
		//CameraFade.StartAlphaFade( Color.clear, true, 0f, 2f, () => {  } );
		Time.timeScale=1;
	}

	void uLink_OnDisconnectedFromServer(uLink.NetworkDisconnection mode)
	{
		Debug.Log("restarting game");
		Application.LoadLevel(Application.loadedLevel);
	}

	[RPC]
	void playerHonkPress(int id)
	{

	}

	[RPC]
	void playerHonkRelease(int id)
	{
		
	}

	//show ball when in AOI
	[RPC]
	void showBall(int id, bool hasJukeJuice,bool hasRollingBomb, bool isHonking, bool hasFlag)
	{
		proxyBallScript[] allProxyScripts = FindObjectsOfType(typeof(proxyBallScript)) as proxyBallScript[];
		for(int i=0;i<allProxyScripts.Length;i++)
		{
			if(id == allProxyScripts[i].playerID)
			{
				allProxyScripts[i].ballAnimation.SetBool("hideBallOffScreen",false);
				allProxyScripts[i].transform.GetChild(0).GetComponent<uLinkObjectLabel_custom>().instantiatedLabel.enabled=true;

				SpriteRenderer[] spriteRenders =  allProxyScripts[i].transform.GetComponentsInChildren<SpriteRenderer>();
				if(hasRollingBomb==true)
				{
				allProxyScripts[i].pickedUpRollingBomb();
				}

					spriteRenders[0].enabled=true;
					spriteRenders[1].enabled=hasFlag;
					spriteRenders[2].enabled=hasJukeJuice;
					spriteRenders[3].enabled=isHonking;
			}
		}
	}

	//hide ball when out of AOI. called from server
	[RPC]
	void hideBall(int id)
	{
		proxyBallScript[] allProxyScripts = FindObjectsOfType(typeof(proxyBallScript)) as proxyBallScript[];
		for(int i=0;i<allProxyScripts.Length;i++)
		{


			if(id == allProxyScripts[i].playerID)
			{

				SpriteRenderer[] spriteRenders =  allProxyScripts[i].transform.GetComponentsInChildren<SpriteRenderer>();


				allProxyScripts[i].ballAnimation = allProxyScripts[i].transform.GetChild(0).GetComponent<Animator>();
				allProxyScripts[i].ballAnimation.SetBool("hideBallOffScreen",true);
				allProxyScripts[i].transform.GetChild(0).GetComponent<uLinkObjectLabel_custom>().instantiatedLabel.enabled=false;
				for(int g=0;g<spriteRenders.Length;g++)
				{

					spriteRenders[g].enabled=false;

				}
			}
		}
	}

}
