using UnityEngine;
using System.Collections;

public class proxyBallScript : MonoBehaviour {
	

	[HideInInspector]
	public int playerID;
	public int teamColor;
	public bool isInJail;
	public bool hasFlag;
	string playerName;

	float lerpTime = 0.35f;
	float currentLerpTime;
	
	bool flashBall=false;
	bool lerpFlashUp=true;

	Rigidbody2D ballphysics;

	public GameObject splatPrefab;

	public Sprite[] spriteSplats;

	public AudioClip bombExploded;
	public AudioClip friendlyFlagPickUpSound;
	public AudioClip flagPickUpSound;
	public AudioClip friendlyFlagDropSound;
	public AudioClip flagDropSound;
	public AudioClip honkSound;

	public AudioSource audioPlayer;

	bool hasJukeJuice = false;

	private float lastBombTime;


	public Animator ballAnimation;	

	private bool isHonking = false;

	// Use this for initialization
	void Start () {
		//do not show flag when started
		ballphysics =  transform.GetChild(0).GetComponent<Rigidbody2D>(); //get ball 2drigiddbody
		audioPlayer = transform.GetChild(0).GetComponent<AudioSource>();
		ballAnimation = transform.GetChild(0).GetComponent<Animator>();

		GameObject.FindGameObjectWithTag("gameEndText").GetComponent<UnityEngine.UI.Text>().enabled=false;
	}
	
	// Update is called once per frame
	void Update () {
		rollingBombFlash();
	}
	void LateUpdate()
	{
		setJukeJuiceToBall();
		setHonkingSpriteToBall();
	}

	void setHonkingSpriteToBall()
	{
		if(isHonking==true)
		{
			transform.GetChild(3).position=new Vector3(transform.GetChild(0).position.x+0.015f	,transform.GetChild(0).position.y+0.28f, transform.GetChild(3).position.z);
			
		}
	}

	void setJukeJuiceToBall()
	{
		transform.FindChild("jukejuicepower").position = new Vector3(transform.GetChild(0).position.x-0.111f	,transform.GetChild(0).position.y-0.141f, transform.GetChild(0).position.z);
		
	}



	void uLink_OnSerializeNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (stream.isWriting==false)
		{
			// This is performed on deserialization.
			if(ballphysics==null)
			{
				ballphysics =  transform.GetChild(0).GetComponent<Rigidbody2D>();
			}

			ballphysics.transform.position = stream.Read<Vector3>();
			ballphysics.transform.rotation = stream.Read<Quaternion>();
			ballphysics.velocity = stream.Read<Vector2>();
			ballphysics.angularVelocity = stream.Read<float>();
			
			
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
	
	//save id of client player proxy is representing
	void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo msg)
	{
		playerID = msg.networkView.owner.id;

		teamColor = msg.networkView.initialData.Read<int>();
		isInJail = msg.networkView.initialData.Read<bool>();
		//playerName = msg.networkView.initialData.Read<string>();

	}
	//proxy show flag on player
	[RPC]
	void showFlagOnPlayer(int id, int teamnumber)
	{

		if(playerID == id)
		{
			if (teamnumber==1)
			{
				transform.FindChild("blueFlag").GetComponent<SpriteRenderer>().enabled = true;
				hasFlag = true;
			}
			else
			{
				transform.FindChild("redFlag").GetComponent<SpriteRenderer>().enabled = true;
				hasFlag = true;
			}
			audioPlayer = transform.GetChild(0).GetComponent<AudioSource>();
			if (teamnumber==1)
			{
				GameObject.FindGameObjectWithTag("blueFlag").GetComponentInChildren<MeshRenderer>().enabled=false;
				GameObject.FindGameObjectWithTag("uiBlueFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=true;
				if(teamColor==0)
				{
					PlayAtPoint(friendlyFlagPickUpSound, transform.position);
				}
				else
				{
					PlayAtPoint(flagPickUpSound, transform.position);
				}
			}
			else
			{
				GameObject.FindGameObjectWithTag("redFlag").GetComponentInChildren<MeshRenderer>().enabled=false;
				GameObject.FindGameObjectWithTag("uiRedFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=true;
				
				if(teamColor==0)
				{
					PlayAtPoint(flagPickUpSound, transform.position);
				}
				else
				{
					PlayAtPoint(friendlyFlagPickUpSound, transform.position);
				}
			}
		}



		
	}

	[RPC]
	void destroyPartial(int id)
	{
		
		if(playerID == id)
		{
			transform.GetChild(0).GetComponent<CircleCollider2D>().enabled=false;

			transform.GetChild(1).GetComponent<SpriteRenderer>().enabled=false;
			transform.GetChild(2).GetComponent<SpriteRenderer>().enabled=false;
			
			ballAnimation.SetBool("playDeadAnimation",true);


			transform.GetChild(0).GetComponent<uLinkObjectLabel_custom>().instantiatedLabel.enabled=false;

			//run pop animation

		}
	}

	//respawn the ball
	//set hidden properties to enabled
	[RPC]
	void respawnBall(int id)
	{
		
		if(playerID == id)
		{
			transform.GetChild(0).GetComponent<Rigidbody2D>().velocity=new Vector2(0,0);
			transform.GetChild(0).GetComponent<Rigidbody2D>().angularVelocity = 0;
			transform.GetChild(0).rotation = new Quaternion(0,0,0,0);
			transform.GetChild(0).GetComponent<CircleCollider2D>().enabled=true;
			transform.GetChild(0).GetComponent<SpriteRenderer>().enabled=true;

			ballAnimation.SetBool("playDeadAnimation",false);


			if(hasJukeJuice==true)
			{
				transform.GetChild(2).GetComponent<SpriteRenderer>().enabled=true;
			}
			
			transform.GetChild(0).GetComponent<uLinkObjectLabel_custom>().instantiatedLabel.enabled=true;
		}

	}
	[RPC]
	void clientResetFlag(int flagColor)
	{
		if(flagColor==1)
		{
			GameObject.FindGameObjectWithTag("redFlag").GetComponentInChildren<MeshRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag("uiRedFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=false;
			if(teamColor==0)
			{
				PlayAtPoint(friendlyFlagDropSound, transform.position);
			}
			else
			{
				PlayAtPoint(flagDropSound, transform.position);
			}
		}
		else if (flagColor==0)
		{
			GameObject.FindGameObjectWithTag("blueFlag").GetComponentInChildren<MeshRenderer>().enabled=true;
			GameObject.FindGameObjectWithTag("uiBlueFlag").GetComponentInChildren<UnityEngine.UI.Image>().enabled=false;
			if(teamColor==1)
			{
				PlayAtPoint(friendlyFlagDropSound, transform.position);
			}
			else
			{
				PlayAtPoint(flagDropSound, transform.position);
			}
		}
	}

	[RPC]
	void hidePowerupOnPlayer(int powerupNumber, int id, int exploded)
	{
		if(id == playerID)
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
					//don't play sound unless you can see ball
					if((Vector3.Distance(transform.position,GameObject.FindGameObjectWithTag("owner").transform.GetChild(0).transform.position)) <10)
					{
						//dont play bomb sounds very quickly because it hurts my ears if bombs are too close
						if(Time.time - lastBombTime>=0.10f)
						{
							PlayAtPoint(bombExploded, transform.position);
							lastBombTime=Time.time;
						}
					}
				}
				flashBall = false;
				transform.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_FlashAmount",0);
			}


		}
	}
	
	[RPC]
	void showPowerupOnPlayer(int powerupNumber, int id)
	{
		
		if(id == playerID)
		{
			if (powerupNumber==0)
			{
				transform.FindChild("jukejuicepower").GetComponent<SpriteRenderer>().enabled = true;
				hasJukeJuice=true;
			}
			else if (powerupNumber==1)
			{
				pickedUpRollingBomb();

			}
		}
	}

	public void pickedUpRollingBomb()
	{
		currentLerpTime = 0f;
		flashBall = true;
		lerpFlashUp=true;
	}
	
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
	void popSoundPlay(int id1, int id2)
	{
		//do nothing for proxy
		
	}

	[RPC]
	void showSpawnLocation(Vector3 spawnLocation, int colorOfTeam, int id)
	{
		if(id==playerID)
		{
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
	void playerHonkPress(int id)
	{
		if(id==playerID)
		{
			isHonking=true;
			transform.GetChild(3).GetComponent<SpriteRenderer>().enabled=true;
			audioPlayer.Play();
		}
	}

	[RPC]
	void playerHonkRelease(int id)
	{
		if(id==playerID)
		{
			isHonking=false;
			transform.GetChild(3).GetComponent<SpriteRenderer>().enabled=false;
			audioPlayer.Stop();

		}
	}


}
