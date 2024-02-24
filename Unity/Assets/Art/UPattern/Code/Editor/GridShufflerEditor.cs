using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(GridShuffler))]
public class shufflerEditor : Editor {

	private GridShuffler myScript;
	private Texture2D LableTexture;
	public AnimationCurve test = AnimationCurve.Linear(0,0,1,1);
	
//	private int iModuleClassArraySize;
	private float[] fModuleClassArrayValues;
	private float[] fInitModuleClassArrayValues;
	private float fChanceSum;

	public SerializedProperty
		Description_Prop,
		Module_Prop,
		Mode_Prop,
		RandomizeRotation_Prop,
		Offset_Prop,
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
		//Debug.Log("Selected!");
		// Setup the SerializedProperties

		LableTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/UPattern/Resources/ShufflerIcon_MoveAgentMap.png", typeof(Texture2D));
		//Debug.Log(LableTexture);
		Description_Prop = serializedObject.FindProperty ("Description");
		Module_Prop = serializedObject.FindProperty ("ModuleList"); 
		Mode_Prop = serializedObject.FindProperty ("GeneratorMode"); 
		RandomizeRotation_Prop = serializedObject.FindProperty ("RandomizeRotation");
		Offset_Prop = serializedObject.FindProperty ("ModuleOffset");
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

		myScript = (GridShuffler)target;
		myScript.UpdateIndexNumber();
		myScript.InitMazeString();
		//SetInitModuleIntValues();
		myScript.GetComponent<Transform>().hideFlags = HideFlags.HideInInspector;
		myScript.GenerateGizmoGameobject();
	//	iModuleClassArraySize = Module_Prop.arraySize;
	}
	
	void OnDisable () {
		//Debug.Log("NotSelected!");
		myScript.DestroyGizmoGameobject();
		//SetInitModuleIntValues();
		//UpdateModuleChance();
		//CheckModuleIntValuesSetChance();
	//	iModuleClassArraySize = Module_Prop.arraySize;
	}

	public override void OnInspectorGUI()
	{
		myScript.UpdateGizmo();
		myScript.GetPivot();
		myScript.UpdateIndexNumber();
		serializedObject.Update ();
		GUILayout.Label(LableTexture);
		GUILayout.Label("Grid Shuffler Generator:", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox( Description_Prop.stringValue , MessageType.Info);	
		EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField( Module_Prop ,true);
		if (EditorGUI.EndChangeCheck()) {
			serializedObject.ApplyModifiedProperties ();
			//iModuleClassArraySize = Module_Prop.arraySize;
		}
		EditorGUILayout.PropertyField( Mode_Prop );
		EditorGUILayout.PropertyField( RandomizeRotation_Prop );
		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField( Offset_Prop );
		if (EditorGUI.EndChangeCheck()) {
			if (Offset_Prop.floatValue < 0 ) {
				Offset_Prop.floatValue = 0;
			}
		}
		EditorGUILayout.PropertyField( MazeName_Prop);
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
		//EditorGUILayout.Separator( );
		//EditorGUILayout.Foldout(false, "yo");
		//AnimationCurve yo = EditorGUILayout.CurveField(test);



		//Debug.Log(yo.Evaluate(3));
		//EditorGUILayout.BoundsField(GameObject_Prop.boundsValue	);
		
		//EditorGUILayout.ColorField(Color.blue);
		
		GridShuffler.enPivotpoint PivotSelection = (GridShuffler.enPivotpoint)Pivot_Prop.enumValueIndex;

		//EditorGUILayout.EnumPopup(PivotSelection);

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
			

			Debug.Log("Custome Inspector Changed!");
			switch( PivotSelection ) {
			case GridShuffler.enPivotpoint.CENTER: 
				myScript.SetPivot(Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 +  (float)MazeWidth_Prop.intValue/2,Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 + (float)MazeHeigth_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			case GridShuffler.enPivotpoint.BOTTOM: 
				myScript.SetPivot(Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 +(float)MazeWidth_Prop.intValue/2, 0);
				PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break;
			case GridShuffler.enPivotpoint.TOP: 
				myScript.SetPivot(Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 +(float)MazeWidth_Prop.intValue/2, Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 +(float)MazeHeigth_Prop.intValue);
				PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			case GridShuffler.enPivotpoint.LEFT: 
				myScript.SetPivot(0,Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 + (float)MazeHeigth_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break;
			case GridShuffler.enPivotpoint.RIGHT: 
				myScript.SetPivot(Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 +(float)MazeWidth_Prop.intValue ,Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 + (float)MazeHeigth_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			case GridShuffler.enPivotpoint.FREE: 
				myScript.SetPivot(Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 +(float)MazeWidth_Prop.intValue ,Offset_Prop.floatValue * MazeWidth_Prop.intValue/2 + (float)MazeHeigth_Prop.intValue);
				//PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			}
			myScript.GetPivot();
			
		}
		
		// Draw the Buttons
		if(GUILayout.Button("Generate Maze")){
			if(KeepSeed_Prop.boolValue == true)
				Random.InitState(Seed_Prop.intValue);
			myScript.GenerateShuffler();
		}
		if(GUILayout.Button("Regenerate")){
			if(KeepSeed_Prop.boolValue == true)
				Random.InitState(Seed_Prop.intValue);
			myScript.DeleteOneBitMaze();
			myScript.UpdateIndexNumber();
			myScript.GenerateShuffler();
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
