using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class ScreenPixels : MonoBehaviour {

	[SerializeField] private int widthPixelsStep = 300;
	[SerializeField] private int heightPixelsStep = 200;

	void Update(){
		// Check if read screen button is pushed (by default it's Fire1 = leftCtrl or left click)
		bool pushed = CrossPlatformInputManager.GetButtonDown("Fire1");
		if (pushed)
			StartCoroutine (Start ());
	}

	// Read immediatly
	IEnumerator Start () {
		yield return ReadScreen();
	}
	
	// Update is called once per frame
	IEnumerator ReadScreen () {
		// We should only read the screen buffer after rendering is complete
		yield return new WaitForEndOfFrame();

		// Create a texture the size of the screen, RGB24 format
		int width = Screen.width;
		int height = Screen.height;
		Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		print("Screen Size : (" + width + ", " + height + ")"); 

		// Read screen contents into the texture
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		tex.Apply();

		// Check if texture is correctly read in 20 pixels upper left
		Color32[] pixels = tex.GetPixels32();
		string res = "";

		for (int i = 0; i < height; i += heightPixelsStep) {
			for (int j = 0; j < width; j += widthPixelsStep) {
				res += pixels [i * height + j].ToString ();
			}
			res += "\n";
		}
		print (res);
	}
}
