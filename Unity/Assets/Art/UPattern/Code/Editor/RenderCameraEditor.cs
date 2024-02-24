using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

[CustomEditor(typeof(RenderCamera))]
public class RenderCameraEditor : Editor{
	
	// Related script
	RenderCamera CustomCameraScript;	
	
	// Serialized properties
	public SerializedProperty
		Description_Prop,
		Filename_Prop;
	
	// When inspector is drawn
	void OnEnable () {
		CustomCameraScript = (RenderCamera)target;
		
		serializedObject.ApplyModifiedProperties ();
		EditorUtility.SetDirty(CustomCameraScript);
		AssetDatabase.Refresh();
		
		Description_Prop = serializedObject.FindProperty ("Description");
		Filename_Prop = serializedObject.FindProperty ("filename");	

	}
	
	// When you interact with inspector
	public override void OnInspectorGUI()
	{		
		// Draw the fields
		EditorGUILayout.HelpBox( Description_Prop.stringValue , MessageType.Info);


		// Check if field has changed
		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField( Filename_Prop);
		if (EditorGUI.EndChangeCheck()) {
			// Apply changes and set dirty
			serializedObject.ApplyModifiedProperties ();
			EditorUtility.SetDirty(CustomCameraScript);
		}
		
		// Draw the buttons
		if(GUILayout.Button("Render Camera View")){	
			CustomCameraScript.TakeScreenShot(Filename_Prop.stringValue);
			AssetDatabase.Refresh();
			serializedObject.Update ();	
		}
	}
}