using UnityEngine;  
using System.Collections; 
using System.Collections.Generic; 
using UnityEditor;  
using System.IO;

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

// This class provides a popup to use uPattern generators in scene view.
[InitializeOnLoad]
public class uPatternPopupMenu : Editor
{
	static Vector3 v3MousePos;
	static Vector3 v3MouseDir;

	static float fFactor;

	static uPatternPopupMenu ()
	{
		SceneView.onSceneGUIDelegate += OnScene;
	}

	static void OnScene(SceneView sceneView) {
	
		int iControlID = GUIUtility.GetControlID(FocusType.Passive);
		Event currentEvent = Event.current;

		if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Tab){
			v3MousePos = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).origin;
			v3MouseDir = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition).direction;

			fFactor = v3MousePos.y / v3MouseDir.y;
				
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Create uPattern Tools Camera"), false, uPatternToolsCameraAtMousePosition);
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Generate on mouse position/Simple Maze"), false, SimpleMazeAtMousePosition);
			menu.AddItem(new GUIContent("Generate on mouse position/Grid Shuffler"), false, GridShufflerAtMousePosition);
			menu.AddItem(new GUIContent("Generate on mouse position/Move Agent"), false, MoveAgentAtMousePosition);
			menu.AddItem(new GUIContent("Generate on mouse position/Tube Agent"), false, TubeAgentAtMousePosition);
			menu.AddItem(new GUIContent("Convert selection to .../Tube Agent module"), false, ConvertToTubeAgentModule);
			
			menu.ShowAsContext();
			currentEvent.Use();
			Tools.current = Tool.View;
			HandleUtility.AddDefaultControl(iControlID);
		}
	}

	// Generates a uPattern Tools Camera.
	static void uPatternToolsCameraAtMousePosition() {
		GameObject uPatternToolsCamera = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/Misc/uPatternToolsCamera.prefab", typeof(GameObject)) as GameObject;
		uPatternToolsCamera.transform.position = v3MousePos - v3MouseDir * fFactor;
		Instantiate(uPatternToolsCamera);
		
	}

	// Generates a Simple Maze at mouse position.
	static void SimpleMazeAtMousePosition() {
		GameObject SimpleMazePrefab = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/SimpleMaze.prefab", typeof(GameObject)) as GameObject;
		SimpleMazePrefab.GetComponent<SimpleMaze>().MazePosition = v3MousePos - v3MouseDir * fFactor;
		SimpleMazePrefab.GetComponent<SimpleMaze>().GenerateSimpleMaze();

	}

	// Generates a Grid Shuffler at mouse position.
	static void GridShufflerAtMousePosition() {
		GameObject GridShufflerPrefab = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/GridShuffler.prefab", typeof(GameObject)) as GameObject;
		GridShufflerPrefab.GetComponent<GridShuffler>().MazePosition = v3MousePos - v3MouseDir * fFactor;
		GridShufflerPrefab.GetComponent<GridShuffler>().GenerateShuffler();
		
	}

	// Generates a Move Agent at mouse position.
	static void MoveAgentAtMousePosition() {
		GameObject TubeAgentPrefab = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/MoveAgent.prefab", typeof(GameObject)) as GameObject;
		TubeAgentPrefab.GetComponent<MoveAgent>().AgentPosition = v3MousePos - v3MouseDir * fFactor;
		TubeAgentPrefab.GetComponent<MoveAgent>().GenerateMoveAgent(true);
		
	}

	// Generates a Tube Agent at mouse position.
	static void TubeAgentAtMousePosition() {
		GameObject TubeAgentPrefab = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/TubeAgent.prefab", typeof(GameObject)) as GameObject;
		TubeAgentPrefab.GetComponent<TubeAgent>().AgentPosition = v3MousePos - v3MouseDir * fFactor;
		TubeAgentPrefab.GetComponent<TubeAgent>().SetInitPosition();
		TubeAgentPrefab.GetComponent<TubeAgent>().GenerateAgent();
		
	}


	// -------------- //
	// | Conversion | //
	// -------------- //

	// Convertes selection to a Tube Agent module
	static void ConvertToTubeAgentModule() {
		if(Selection.activeGameObject){
			GameObject SelectedObject = Selection.activeGameObject;	
			GameObject TubeAgentPrefab = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/TubeAgent.prefab", typeof(GameObject)) as GameObject;
			GameObject GoNewConnector = new GameObject();
			string sAssetPath = "Assets/uPattern/3dAssets/Prefabs/TubeAgentModules/"+TubeAgentPrefab.GetComponent<TubeAgent>().AgentName+"_TAModule.prefab";
			GoNewConnector.name = TubeAgentPrefab.GetComponent<TubeAgent>().ConnectorName;
			GoNewConnector.transform.position = SelectedObject.transform.position;
			GoNewConnector.transform.parent = SelectedObject.transform;
			if(System.IO.Directory.Exists("Assets/uPattern/3dAssets/Prefabs/TubeAgentModules/"))
				PrefabUtility.CreatePrefab( sAssetPath, SelectedObject);
			else {
				System.IO.Directory.CreateDirectory("Assets/uPattern/3dAssets/Prefabs/TubeAgentModules/" );
				PrefabUtility.CreatePrefab( sAssetPath, SelectedObject);
			}
			Debug.Log("Tube Agent module created at: " + sAssetPath);
		}
		else
			Debug.Log("Please select something in the scene view!");
	}
}
