using UnityEngine;
using System.Collections;

public class cameraAspectRatio : MonoBehaviour {

	Camera camera;
	public Camera background_image_camera;
	
	float Native_Screen_Width;
	float Native_Screen_Height;
	
	public float width = 1280f;
	public float height = 800f;
	
	float newGameBoxHeight;
	float newGameBoxWidth;


	
	void Start(){


		//Get the camera this script is attached to
		camera = this.gameObject.GetComponent<Camera>();
		#if MOBILE_INPUT
		//if on mobile, don't limit screen
		width = Screen.width;
		height = Screen.height;
		#endif


		
	#region Setup the camera for Game Box

			Native_Screen_Width = Screen.width;
			Native_Screen_Height = Screen.height;		


			//Check if the screen width and height are higher or equal 1280x800, if they are, then we do not have to do any scaling for a true 1280x800 pixel render
			if(Screen.width >= width && Screen.height >= height){
				camera.pixelRect = new Rect(Screen.width/2f - width/2f, Screen.height/2f - height/2f, width, height);
			}
			else{
				//If screen width and/or height are less than 1280x800 - fit the game box as well as possible, starting with height
				newGameBoxHeight = Screen.height;
				newGameBoxWidth = newGameBoxHeight*(width/height);
				
				//If the game box were the same size as screen height and game box length were greater than screen width - fit the game box lengthwise
				if(newGameBoxWidth >= Screen.width){
					newGameBoxWidth = Screen.width;
					newGameBoxHeight = newGameBoxWidth*(height/width);
					
				}
					camera.pixelRect = new Rect(Screen.width/2f - newGameBoxWidth/2f, Screen.height/2f - newGameBoxHeight/2f, newGameBoxWidth, newGameBoxHeight);
			}
	#endregion
	
	#region Setup the camera for Background Image
		//Change the background image rendering camera so it would fill the screen without stretching
	if(background_image_camera != null)
		background_image_camera.aspect = Native_Screen_Width/Native_Screen_Height;
	#endregion


	}
	
}
