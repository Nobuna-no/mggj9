using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

// 	-------------------------------	//
//	author	: Lewnatic				//
//	email	: lewnatic@live.com	 	//
//	-------------------------------	//

public class RenderCamera : MonoBehaviour {

	// Public variables
	[TextArea(6,1)]
	public string Description = "Use this camera to take screenshots of your current game view. The file is always stored in your screenshots folder.";
	public string filename = "";
	
	// Private variables
	private string time;

	// Public methods
	public void TakeScreenShot(string file)
	{
		// Safe file string 
		time = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

		// Create the folder beforehand if not exists
		if(!System.IO.Directory.Exists(Application.dataPath + "/uPattern/Screenshots/"))
			System.IO.Directory.CreateDirectory(Application.dataPath + "/uPattern/Screenshots/");
		
		// Create the screenshot and refresh assets
		ScreenCapture.CaptureScreenshot(string.Format(Application.dataPath + "/uPattern/Screenshots/{0}_{1}.png", file, time));
		AssetDatabase.Refresh();

		// Check if everything is right
		if(!System.IO.File.Exists(string.Format(Application.dataPath + "/uPattern/Screenshots/{0}_{1}.png", file, time)))
			Debug.Log("File: "+ string.Format(Application.dataPath + "/uPattern/Screenshots/{0}_{1}.png", file, time) + " created");
		else 
			Debug.Log("Error creating file.");                
	}
	
}