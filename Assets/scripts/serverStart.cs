
using System;
using UnityEngine;
using uLink;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// A script example that can be used to start a very simple Unity server 
/// listening for uLink connection attempts from clients.
/// </summary>
/// <remarks>
/// The server is listening for UDP traffic on one port number. Default value is 7100.
/// The port number can be changed to whatever port number you like.
/// Another imporant property is targetFrameRate. This value dictates 
/// how many times per second the server reads incoming network traffic 
/// and sends outgoing traffic. It also dictates the actual frame rate for
/// the server (sometimes called tick rate). Read more about tick rate in
/// the Server Operations chapter in the uLink manual.
/// The property called registerHost dictates if this game server should
/// try to register iteself in a uLink Master Server. Read the Master Server & Proxy
/// manual chapter for more info.
/// </remarks>

public class serverStart : uLink.MonoBehaviour
{


	[Serializable]
	public class InstantiateOnConnected
	{
		public Vector3 startPosition;



		public Vector3 startRotation = new Vector3(0, 0, 0);
		
		public GameObject ownerPrefab;
		public GameObject proxyPrefab;
		public GameObject serverPrefab;

		public bool appendLoginData = true;
		
		public void Instantiate(uLink.NetworkPlayer player, int teamColor=-1, bool jailSpawn=false, string name = "Some Ball")
		{

			if (teamColor==-1)
			{
				if (numberOfPlayersOnRed>numberOfPlayersOnBlue)
				{
					teamColor=0;//blue
					numberOfPlayersOnBlue++;

				}
				else if(numberOfPlayersOnRed<numberOfPlayersOnBlue)
				{
					teamColor=1;//red
					numberOfPlayersOnRed++;
				}
				else
				{
					//randomly choose one teamColor
					float randomValue = UnityEngine.Random.value;
					if (randomValue < 0.5f) {
						Debug.Log(randomValue);
						teamColor=0;
						numberOfPlayersOnBlue++;
					}
					else {
						teamColor=1;
						numberOfPlayersOnRed++;
					}
				}
			}
				if(teamColor==1)
				{
					//start position set to hardcoded value + random value right now. if custom tile system is built, the ball can be put into a tile and an object can be designiated as the start position 
					//but since theres no tile system, we cant spawn inside tile sprites
					float randomValueX = UnityEngine.Random.Range(-3.3f,1);
					float randomValueY = UnityEngine.Random.Range(-0.4f,0.1f);
					startPosition = new Vector3(-5+randomValueX, 7.2f+randomValueY, 0);
					if(jailSpawn==true)
					{
					//ugly hardcoded values
					randomValueX = UnityEngine.Random.Range(0f,1.1f);
					randomValueY = UnityEngine.Random.Range(-0.3f,0f);
					startPosition = new Vector3(6.606f+randomValueX, -2.418f+randomValueY, 0);
					}

					

					ownerPrefab = (GameObject)Resources.Load("redBallOwner", typeof(GameObject));
					proxyPrefab = (GameObject)Resources.Load("redBallProxy", typeof(GameObject));
					serverPrefab = (GameObject)Resources.Load("redBallServer", typeof(GameObject));
				}
				else
				{
				//ugly hardcoded values
					float randomValueX = UnityEngine.Random.Range(-1f,3.3f);
					float randomValueY = UnityEngine.Random.Range(-0.1f,0.4f);
					startPosition = new Vector3(5+randomValueX, -7.2f+randomValueY, 0);
					if(jailSpawn==true)
					{
					//ugly hardcoded values
					randomValueX = UnityEngine.Random.Range(-1.1f,0f);
					randomValueY = UnityEngine.Random.Range(-0f,0.3f);
					startPosition = new Vector3(-6.606f+randomValueX, 2.418f+randomValueY, 0);
					}

					

					ownerPrefab = (GameObject)Resources.Load("blueBallOwner", typeof(GameObject));
					proxyPrefab = (GameObject)Resources.Load("blueBallProxy", typeof(GameObject));
					serverPrefab = (GameObject)Resources.Load("blueBallServer", typeof(GameObject));
				}
			//set team to local data
			player.SetLocalData(teamColor);



			if (ownerPrefab != null && proxyPrefab != null && serverPrefab != null)
			{

				Quaternion rotation = Quaternion.Euler(startRotation);

				string playerName;
				
				if (player.loginData == null || !player.loginData.TryRead(out playerName)) playerName = name;



					uLink.Network.Instantiate(player, proxyPrefab, ownerPrefab, serverPrefab, startPosition, rotation, 0,teamColor, jailSpawn, playerName);



			}

		}

	
	}


	
	public int port = 7100;
	public int maxConnections = 64;
	
	public bool cleanupAfterPlayers = true;

	
	public int targetFrameRate = 60;
	
	public bool dontDestroyOnLoad = false;

	public static int numberOfPlayersOnRed = 0;
	public static int numberOfPlayersOnBlue = 0;

	public GameObject powerupSpawner;
	public Vector3[] powerupPositions = new Vector3[7];

	public InstantiateOnConnected instantiateOnConnected = new InstantiateOnConnected();

	public bool replaysEnabled = true;


	public bool allowGamePauses = false;

	[HideInInspector]
	public bool gamePaused = false;

	void Start()
	{
		Application.targetFrameRate = targetFrameRate;
		
		if (dontDestroyOnLoad) DontDestroyOnLoad(this);
		
		uLink.Network.InitializeServer(maxConnections, port);

		uLink.MasterServer.RegisterHost("CTF", "Tagpro_Clone",
		                                "","","HolySee");

		timeObject = GameObject.FindGameObjectWithTag("timer").GetComponent<timerScript>();


	}


	void LateUpdate()
	{
		if(replaysEnabled==true)
		{
			replaySystem();
		}
	}


	 
	private timerScript timeObject;
	private float currentGameTime;
	
	private List<Vector2> ballTransforms = new List<Vector2>();
	private List<float> ballRotations = new List<float>();
	private List<float> ballIDs = new List<float>();
	private List<float> timeList = new List<float>();

	//very crude, ----unoptimized---- replay system
	//currently saves transform and rotation of all balls every frame
	//if replays are enabled on server this function will run
	void replaySystem()
	{
		currentGameTime = timeObject.timer;



		//get all server balls
		GameObject[] serverBalls = GameObject.FindGameObjectsWithTag("Player");

		for(int i=0;i<serverBalls.Length;i++)
		{
			if(serverBalls[i].GetComponent<Rigidbody2D>()!=null)
			{
				timeList.Add(currentGameTime);

				ballTransforms.Add((Vector2)serverBalls[i].transform.position);
				ballRotations.Add(serverBalls[i].GetComponent<Rigidbody2D>().rotation);
				ballIDs.Add(serverBalls[i].GetComponent<serverBallScript>().playerID);
			}

		}




	}

	void writeReplayToFile()
	{


		StreamWriter replayWriter = new System.IO.StreamWriter(Application.persistentDataPath+"/replayTest_"+DateTime.Now.ToString(@"yyyy-MM-dd__h-mm")+".txt");

		for(int i=0;i<ballTransforms.Count;i++)
		{
			replayWriter.WriteLine(timeList[i]+":"+ballIDs[i]+":"+ ballTransforms[i]+":"+ballRotations[i]);
			
		}
		replayWriter.Flush();
		replayWriter.Close();

	}



	
	void uLink_OnServerInitialized()
	{
		Debug.Log("Server successfully started on port " + uLink.Network.listenPort);


		createPowerups();
	}

	void createPowerups()
	{
		//hardcoded positions for powerups right now, cause no custom tile system...
		powerupPositions[0] = new Vector3(-0.2f, 7.4f,0);
		powerupPositions[1] = new Vector3(-0.2f, -7.8f,0);
		powerupPositions[2] = new Vector3(-7.8f, -5.8f,0);
		powerupPositions[3] = new Vector3(-10.6f, -0.19f,0);
		powerupPositions[4] = new Vector3(10.6f, -0.19f,0);
		powerupPositions[5] = new Vector3(7.8f, 5.4f,0);
		powerupPositions[6] = new Vector3(-0.2f, -0.197f,0);

		Quaternion defaultRotation = new Quaternion(0,0,0,0);
		for (int i = 0; i < powerupPositions.Length; i++)
		{

			uLink.Network.Instantiate(powerupSpawner, powerupPositions[i],defaultRotation, 0);

		}


	}
	
	void uLink_OnPlayerDisconnected(uLink.NetworkPlayer player)
	{
	
		string name = "";

		serverBallScript[] serverScripts = FindObjectsOfType(typeof(serverBallScript)) as serverBallScript[];
		foreach (serverBallScript script in serverScripts) {
			if(script.playerID==player.id)
			{
				name=script.playerName;
			}
		}

	
		GameObject.FindGameObjectWithTag("chat").transform.uLinkNetworkView().RPC("Chat", uLink.NetworkPlayer.server, name+" has left the game.");
			


		if (cleanupAfterPlayers)
		{
			//only on blue

			if((int)player.GetLocalData() == 0)
			{
				numberOfPlayersOnBlue--;
			}
			//only on red
			else if ((int)player.GetLocalData()==1)
			{
				numberOfPlayersOnRed--;
			}
			//on blue and has flag
			else if ((int)player.GetLocalData()==4)
			{
				numberOfPlayersOnBlue--;
				GameObject.FindGameObjectWithTag("redFlag").GetComponentInChildren<MeshRenderer>().enabled=true;

				transform.uLinkNetworkView().RPC("clientResetFlag", uLink.RPCMode.Buffered, 1);
				for(int i=0;i<uLink.Network.connections.Length;i++)
				{
					transform.uLinkNetworkView().RPC("clientResetFlag", uLink.Network.connections[i], 1);
				}

			}
			//on red and has flag
			else if ((int)player.GetLocalData()==3)
			{
				numberOfPlayersOnRed--;
				GameObject.FindGameObjectWithTag("blueFlag").GetComponentInChildren<MeshRenderer>().enabled=true;

				transform.uLinkNetworkView().RPC("clientResetFlag", uLink.RPCMode.Buffered, 0);
				for(int i=0;i<uLink.Network.connections.Length;i++)
				{
					transform.uLinkNetworkView().RPC("clientResetFlag", uLink.Network.connections[i], 0);
				}
			}



			uLink.Network.DestroyPlayerObjects(player);



			// this is not really necessery unless you are removing NetworkViews without calling uLink.Network.Destroy
			uLink.Network.RemoveInstantiates(player);
		}
	}
	
	void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
	{

		instantiateOnConnected.Instantiate(player, -1, false);



	}

	public void restartGame()
	{
		writeReplayToFile();

		StartCoroutine(restartMapAfterSeconds(10));

	}
	IEnumerator restartMapAfterSeconds(float duration)
	{	
		yield return new WaitForSeconds(duration);
		uLink.Network.DestroyAll();

		GameObject.FindGameObjectWithTag("timer").GetComponent<timerScript>().restart();


		//reset boosts
		boostScript[] boostGameObjects = FindObjectsOfType(typeof(boostScript)) as boostScript[];

		for(int i=0;i<boostGameObjects.Length;i++)
		{
			if(boostGameObjects[i].transform.GetComponentInChildren<MeshRenderer>() !=null)
			{
				boostGameObjects[i].transform.GetComponentInChildren<MeshRenderer>().enabled=true;
			}
		}



		createPowerups();


		//reconnect all back to server
		int amountOfConnections = uLink.Network.connections.Length;

		for(int i=0;i<amountOfConnections;i++)
		{

			uLink.Network.RedirectConnection(uLink.Network.connections[0],uLink.NetworkPlayer.server.endpoint);
		}



		
	}





}
