using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(SimpleMaze))]
public class simpleMazeEditor : Editor{

	SimpleMaze myScript;
	private Texture2D LableTexture;


	public SerializedProperty
		Description_Prop,
		GameObject_Prop,
		MazeName_Prop,
		MazePosition_Prop,
		IndexNumber_Prop,
		MazeWidth_Prop,
		MazeHeigth_Prop,
		ModuleNumber_Prop,
		Seed_Prop,
		KeepSeed_Prop,
		Pivot_Prop,
		PivotPoint_Prop;


	void OnEnable () {
		LableTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/UPattern/Resources/SimpleMazeIcon_MoveAgentMap.png", typeof(Texture2D));

		//Debug.Log("Selected!");
		// Setup the SerializedProperties
		Description_Prop = serializedObject.FindProperty ("Description");
		GameObject_Prop = serializedObject.FindProperty ("SimpleMazeModule");
		MazeName_Prop = serializedObject.FindProperty ("MazeName");
		MazePosition_Prop = serializedObject.FindProperty ("MazePosition");
		IndexNumber_Prop = serializedObject.FindProperty ("IndexNumber");
		MazeWidth_Prop = serializedObject.FindProperty ("MazeWidth");
		MazeHeigth_Prop = serializedObject.FindProperty ("MazeHeight");
		ModuleNumber_Prop =  serializedObject.FindProperty ("ModuleNumber");
		Seed_Prop = serializedObject.FindProperty ("Seed");
		KeepSeed_Prop = serializedObject.FindProperty ("KeepSeed");
		Pivot_Prop = serializedObject.FindProperty ("Pivot");
		PivotPoint_Prop = serializedObject.FindProperty ("PivotPoint");

		Random.InitState(Seed_Prop.intValue);
		
		myScript = (SimpleMaze)target;
		myScript.UpdateIndexNumber();
		myScript.InitMazeValues();
		myScript.GetComponent<Transform>().hideFlags = HideFlags.HideInInspector;
		myScript.GenerateGizmoGameobject();
		//myScript.GetComponent<oneBitMazeGizmo>().hideFlags = HideFlags.HideInInspector;
		//myScript.GetComponent<oneBitMazeGizmo>().hideFlags = HideFlags.HideInHierarchy;
		
	}
	
	void OnDisable () {
		//Debug.Log("NotSelected!");
		myScript.DestroyGizmoGameobject();
	}

	public override void OnInspectorGUI()
	{

		myScript.UpdateGizmo();
		myScript.GetPivot();
		myScript.UpdateIndexNumber();
		
		//Debug.Log("IsSelected!");
		//EditorGUILayout.PropertyField( controllable_Prop, new GUIContent("controllable") );
		//EditorGUILayout.IntSlider ( valForAB_Prop, 0, 100, new GUIContent("valForAB") );
		serializedObject.Update ();
		//DrawDefaultInspector();



		GUILayout.Label(LableTexture);
		GUILayout.Label("Simple Maze Generator:", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox( Description_Prop.stringValue , MessageType.Info);
		//AssetPreview()
		//EditorGUI.DrawPreviewTexture(new Rect(0,0,64,64), Edit);
		EditorGUILayout.PropertyField( GameObject_Prop );
		EditorGUILayout.PropertyField( MazeName_Prop );
		EditorGUILayout.PropertyField( MazePosition_Prop );
		EditorGUILayout.PropertyField( IndexNumber_Prop );
		EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( MazeWidth_Prop );
			EditorGUILayout.PropertyField( MazeHeigth_Prop );
		if (EditorGUI.EndChangeCheck())
			ModuleNumber_Prop.intValue = MazeWidth_Prop.intValue * MazeHeigth_Prop.intValue;
		if(ModuleNumber_Prop.intValue >= 15000)
			EditorGUILayout.HelpBox( "WARNING: You are about to generate " + ModuleNumber_Prop.intValue + " GameObjects to your current scene. Decrease the maze size to improve your performance.", MessageType.Warning);

		
		EditorGUILayout.PropertyField( ModuleNumber_Prop );

		EditorGUILayout.PropertyField( Seed_Prop );
		EditorGUILayout.PropertyField( KeepSeed_Prop );
		EditorGUILayout.PropertyField( Pivot_Prop );
		EditorGUILayout.PropertyField( PivotPoint_Prop );


		SimpleMaze.enPivotpoint PivotSelection = (SimpleMaze.enPivotpoint)Pivot_Prop.enumValueIndex;

		// Only unsingned int allowed in inspector
		if (MazeWidth_Prop.intValue < 1 ) {
			MazeWidth_Prop.intValue = 1;
			serializedObject.Update ();
		}
		if (MazeHeigth_Prop.intValue < 1 ) {
			MazeHeigth_Prop.intValue = 1;
			serializedObject.Update ();
		}


		// If Edit has changed
		if( GUI.changed == true ) {
			if (KeepSeed_Prop.boolValue == false)
				Seed_Prop.intValue = Random.Range(0, 9999999);
			
			Random.InitState(Seed_Prop.intValue);

			EditorUtility.SetDirty(target);
			//Debug.Log("Has Changed!");
			switch( PivotSelection ) {
			case SimpleMaze.enPivotpoint.CENTER: 
					myScript.SetPivot((float)MazeWidth_Prop.intValue/2, (float)MazeHeigth_Prop.intValue/2);
					PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			case SimpleMaze.enPivotpoint.BOTTOM: 
				myScript.SetPivot((float)MazeWidth_Prop.intValue/2, 0);
					PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break;
			case SimpleMaze.enPivotpoint.TOP: 
					myScript.SetPivot((float)MazeWidth_Prop.intValue/2, (float)MazeHeigth_Prop.intValue);
					PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			case SimpleMaze.enPivotpoint.LEFT: 
					myScript.SetPivot(0, (float)MazeHeigth_Prop.intValue/2);
					PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break;
			case SimpleMaze.enPivotpoint.RIGHT: 
					myScript.SetPivot((float)MazeWidth_Prop.intValue , (float)MazeHeigth_Prop.intValue/2);
					PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			case SimpleMaze.enPivotpoint.FREE: 
				myScript.SetPivot((float)MazeWidth_Prop.intValue , (float)MazeHeigth_Prop.intValue);
				//PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			}
			myScript.GetPivot();
			
		}
		
		// Draw the Buttons
		if(GUILayout.Button("Generate Maze")){
			if(KeepSeed_Prop.boolValue == true)
				Random.InitState(Seed_Prop.intValue);
			myScript.GenerateSimpleMaze();
		}
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Delete last"))
		{	
			myScript.DeleteOneBitMaze();
			myScript.UpdateIndexNumber();
		}
		if(GUILayout.Button("Delete all"))
		{
			myScript.DeleteAll();
			myScript.UpdateIndexNumber();
		}
		GUILayout.EndHorizontal();



		// Apply properties
		serializedObject.ApplyModifiedProperties ();
		myScript.UpdateGizmo();
	}
}