using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SimpleMazeInScene))]
public class simpleMazeInSceneEditor : Editor {

	SimpleMazeInScene myScript;
	//private Texture2D LableTexture= Resources.Load("SimpleMaze") as Texture2D;
	
	public SerializedProperty
		Description_Prop,
		GameObject_Prop,
		MazeName_Prop,
		MazePosition_Prop,
		//IndexNumber_Prop,
		MazeWidth_Prop,
		MazeHeigth_Prop,
		//Pivot_Prop,
		Seed_Prop,
		KeepSeed_Prop,
		PivotPoint_Prop;
	
	
	void OnEnable () {
		//Debug.Log("Selected!");
		// Setup the SerializedProperties
		Description_Prop = serializedObject.FindProperty ("Description");
		GameObject_Prop = serializedObject.FindProperty ("SimpleMazeModule");
		MazeName_Prop = serializedObject.FindProperty ("MazeName");
		MazePosition_Prop = serializedObject.FindProperty ("MazePosition");
		//IndexNumber_Prop = serializedObject.FindProperty ("IndexNumber");
		MazeWidth_Prop = serializedObject.FindProperty ("MazeWidth");
		MazeHeigth_Prop = serializedObject.FindProperty ("MazeHeigth");
		Seed_Prop = serializedObject.FindProperty ("Seed");
		KeepSeed_Prop = serializedObject.FindProperty ("KeepSeed");
		//Pivot_Prop = serializedObject.FindProperty ("Pivot");
		PivotPoint_Prop = serializedObject.FindProperty ("PivotPoint");
		
		Random.InitState(Seed_Prop.intValue);
		
		myScript = (SimpleMazeInScene)target;
		myScript.GetComponent<Transform>().hideFlags = HideFlags.HideInInspector;
		//myScript.GetComponent<simpleMazeInScene>().GetMazeInSceneProperties(
		//myScript.GetComponent<oneBitMazeGizmo>().hideFlags = HideFlags.HideInInspector;
		//myScript.GetComponent<oneBitMazeGizmo>().hideFlags = HideFlags.HideInHierarchy;
		
		myScript.sMaze = myScript.UpdateInSceneMazeProperties();
		myScript.InitInSceneMazePosition();
		
	}
	
	void OnDisable () {
		//Debug.Log("NotSelected!");
		//myScript.DestroyInSceneGizmoGameobject();
	}


	public void OnSceneGUI() {
		if( Event.current.type == EventType.MouseUp) {
			myScript.InitInSceneMazePosition();
			}
		if(GUI.changed)
			Debug.Log("InSceneChanged");
	}
	
	public override void OnInspectorGUI()
	{
		
		//myScript.GenerateInSceneGizmoGameobject();
		//myScript.UpdateInSceneMazePosition();
		//myScript.UpdateInSceneGizmo();

		
		myScript.sMaze.UpdateIndexNumber();
		//Debug.Log("IsSelected!");
		//EditorGUILayout.PropertyField( controllable_Prop, new GUIContent("controllable") );
		//EditorGUILayout.IntSlider ( valForAB_Prop, 0, 100, new GUIContent("valForAB") );
		serializedObject.Update ();
		//DrawDefaultInspector();
		//EditorGUILayout.BeginHorizontal();
		//GUILayout.Label(LableTexture);
		EditorGUILayout.HelpBox( Description_Prop.stringValue , MessageType.None);
		//EditorGUILayout.EndHorizontal();
		//EditorGUILayout.PropertyField( GameObject_Prop );
		EditorGUILayout.PropertyField( MazeName_Prop );
		EditorGUILayout.PropertyField( MazePosition_Prop );
		//EditorGUILayout.PropertyField( IndexNumber_Prop );
		EditorGUILayout.PropertyField( MazeWidth_Prop );
		EditorGUILayout.PropertyField( MazeHeigth_Prop );
		EditorGUILayout.PropertyField( Seed_Prop );
		EditorGUILayout.PropertyField( KeepSeed_Prop );
		//EditorGUILayout.PropertyField( Pivot_Prop );
		EditorGUILayout.PropertyField( PivotPoint_Prop );

		
		// Only unsingned int allowed in inspector
		if (MazeWidth_Prop.intValue < 1 ) {
			MazeWidth_Prop.intValue = 1;
		}
		if (MazeHeigth_Prop.intValue < 1 ) {
			MazeHeigth_Prop.intValue = 1;
		}
		// If Edit has changed
		if( GUI.changed == true ) {
			Random.InitState(Seed_Prop.intValue);

			EditorUtility.SetDirty(target);

			myScript.UpdateInSceneMazePosition();
			//Debug.Log("Has Changed!");

/*
			switch( PivotSelection ) {
			case simpleMazeInScene.enPivotpoint.CENTER: 
				myScript.sMaze.SetPivot((float)MazeWidth_Prop.intValue/2, (float)MazeHeigth_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = myScript.sMaze.GetPivot();
				break; 
			case simpleMazeInScene.enPivotpoint.BOTTOM: 
				myScript.sMaze.SetPivot((float)MazeWidth_Prop.intValue/2, 0);
				PivotPoint_Prop.vector2Value = myScript.sMaze.GetPivot();
				break;
			case simpleMazeInScene.enPivotpoint.TOP: 
				myScript.sMaze.SetPivot((float)MazeWidth_Prop.intValue/2, (float)MazeHeigth_Prop.intValue);
				PivotPoint_Prop.vector2Value = myScript.sMaze.GetPivot();
				break; 
			case simpleMazeInScene.enPivotpoint.LEFT: 
				myScript.sMaze.SetPivot(0, (float)MazeHeigth_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = myScript.sMaze.GetPivot();
				break;
			case simpleMazeInScene.enPivotpoint.RIGHT: 
				myScript.sMaze.SetPivot((float)MazeWidth_Prop.intValue , (float)MazeHeigth_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = myScript.sMaze.GetPivot();
				break; 
			}
			myScript.sMaze.GetPivot();
		

*/
		}
		// Draw the Buttons
		if(GUILayout.Button("Regenerate")){
			if(KeepSeed_Prop.boolValue == true)
				Random.InitState(Seed_Prop.intValue);
			
			myScript.RegenerateSimpleMaze();
		}


		serializedObject.ApplyModifiedProperties ();
		myScript.UpdateInSceneMazeProperties();
		//myScript.UpdateInSceneGizmo();
	}
}
