using UnityEngine;  
using System.Collections; 
using System.Collections.Generic; 
using UnityEditor;  

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

// This class provides a custom menu to select uPattern generators.
public class uPatternCustomMenu : EditorWindow    
{  
	[MenuItem("Tools/uPattern Tools/Simple Maze",false,1)]
	static void MenuItemSimpleMaze()  
	{  
		if (AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/SimpleMaze.prefab", typeof(Object))) {
			Object found_asset = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/SimpleMaze.prefab", typeof(Object));
			Selection.activeObject = found_asset;
		}
		else
			Debug.Log("Asset not found. Make sure the asset is still in your Generators folder!");
	}  

	[MenuItem("Tools/uPattern Tools/Grid Shuffler",false,51)]  
	static void MenuItemGridShuffler()  
	{  
		if (AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/GridShuffler.prefab", typeof(Object))) {
			Object found_asset = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/GridShuffler.prefab", typeof(Object));
			Selection.activeObject = found_asset;
		}
		else
			Debug.Log("Asset not found. Make sure the asset is still in your generators folder!");
	} 

	[MenuItem("Tools/uPattern Tools/Tube Agent",false,101)]  
	static void MenuItemTubeAgent()  
	{  
		if (AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/TubeAgent.prefab", typeof(Object))) {
			Object found_asset = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/TubeAgent.prefab", typeof(Object));
			Selection.activeObject = found_asset;
		}
		else
			Debug.Log("Asset not found. Make sure the asset is still in your generators folder!");
	}

	[MenuItem("Tools/uPattern Tools/Move Agent",false,102)]  
	static void MenuItemMoveAgent()  
	{  
		if (AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/MoveAgent.prefab", typeof(Object))) {
			Object found_asset = AssetDatabase.LoadAssetAtPath("Assets/uPattern/Generators/MoveAgent.prefab", typeof(Object));
			Selection.activeObject = found_asset;
		}
		else
			Debug.Log("Asset not found. Make sure the asset is still in your generators folder!");
	}
}  