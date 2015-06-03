using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class muteAudioScript : MonoBehaviour {

	public Sprite VolumeOn, VolumeOff;
	bool showEnabled = false;
	bool isMute = false;

	private Image theImageRenderer;
	// Use this for initialization
	void Start () {
		theImageRenderer = gameObject.GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		popUpAudioSprite();
	}

	void popUpAudioSprite()
	{
		if(Input.GetKeyDown(KeyCode.Escape)){
			showEnabled= !showEnabled;
			theImageRenderer.enabled = showEnabled;
		}
	}




	public void clickedImage()
	{
		isMute = !isMute;
		AudioListener.pause = isMute;
		if(isMute==true)
		{
			theImageRenderer.sprite = VolumeOff;
		}
		else if(isMute==false)
		{
			theImageRenderer.sprite = VolumeOn;
		}
	}


}
