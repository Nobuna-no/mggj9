using UnityEngine;
using System.Collections;
using UnityEditor;

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

// This custom editor class is part of the Tupe Agent generator
[CustomEditor(typeof(TubeAgent))]
public class TubeAgentEditor : Editor {

	// Private Variables
	private TubeAgent TubeAgentScript;
	private Texture2D LableTexture;

	// Serialized Editor Properties
	public SerializedProperty
		Description_Prop,
		Module_Prop,
		AgentName_Prop,
		ConnectorName_Prop,
		AgentPosition_Prop,
		RandomRotation_Prop,
		RotationStep_Prop,
		IndexNumber_Prop,
		TubeAgentSeed_Prop,
		KeepSeed_Prop,
		PiecesPerTick_Prop;


	// Inspector is drawn
	void OnEnable () {
		LableTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/UPattern/Resources/TubeAgentIcon_MoveAgentMap.png", typeof(Texture2D));

		// Setup the SerializedProperties
		Description_Prop = serializedObject.FindProperty ("Description");
		Module_Prop = serializedObject.FindProperty ("ModuleList"); 
		AgentName_Prop = serializedObject.FindProperty ("AgentName"); 
		ConnectorName_Prop = serializedObject.FindProperty ("ConnectorName"); 
		AgentPosition_Prop = serializedObject.FindProperty ("AgentPosition"); 
		RandomRotation_Prop = serializedObject.FindProperty ("RandomRotation");
		RotationStep_Prop = serializedObject.FindProperty ("RotationStep");
		IndexNumber_Prop = serializedObject.FindProperty ("IndexNumber");
		TubeAgentSeed_Prop = serializedObject.FindProperty ("TubeAgentSeed");
		KeepSeed_Prop = serializedObject.FindProperty ("KeepSeed");
		PiecesPerTick_Prop = serializedObject.FindProperty ("PiecesPerTick");
		
		// Initialising the Seed value
		if (TubeAgentSeed_Prop.intValue != 0 )
			Random.InitState(TubeAgentSeed_Prop.intValue);
		else
			TubeAgentSeed_Prop.intValue = Random.Range(0, 9999999);
		
		// Assign the Editor script
		TubeAgentScript = (TubeAgent)target;

		// Initialising Tube Agent description
		TubeAgentScript.InitMazeString();

		// Hide the Transform in Inspector
		TubeAgentScript.GetComponent<Transform>().hideFlags = HideFlags.HideInInspector;

		// Generates the Gizmo object for the Tube Agent
		TubeAgentScript.GenerateGizmoGameobject();
		
	}
	
	// Inspector is not drawn
	void OnDisable () {
		TubeAgentScript.DestroyGizmoGameobject();
	}


	public override void OnInspectorGUI()
	{
		TubeAgentScript.UpdateIndexNumber();
		serializedObject.Update ();	
		TubeAgentScript.UpdateGizmo();
		GUILayout.Label(LableTexture);
		GUILayout.Label("Tube Agent Generator:", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox( Description_Prop.stringValue , MessageType.Info);	
		EditorGUILayout.PropertyField( Module_Prop ,true);
		EditorGUILayout.PropertyField( AgentName_Prop );
		EditorGUILayout.PropertyField( ConnectorName_Prop );
		//EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField( AgentPosition_Prop );
		//if (EditorGUI.EndChangeCheck()) {
		//	TubeAgentScript.AgentPosition = AgentPosition_Prop.vector3Value;
		//}
		EditorGUILayout.PropertyField( RandomRotation_Prop );
		if(RandomRotation_Prop.boolValue == false) {
			EditorGUILayout.PropertyField( RotationStep_Prop );
		}
		EditorGUILayout.PropertyField( IndexNumber_Prop );
		EditorGUILayout.PropertyField( TubeAgentSeed_Prop );
		EditorGUILayout.PropertyField( KeepSeed_Prop );
		
		// Only unsingned int allowed in inspector
		if (PiecesPerTick_Prop.intValue < 1 ) {
			PiecesPerTick_Prop.intValue = 1;
		}

		// If Editor has changed
		if( GUI.changed == true ) {
			if (KeepSeed_Prop.boolValue == false)
				TubeAgentSeed_Prop.intValue = Random.Range(0,9999999);
			
			Random.InitState(TubeAgentSeed_Prop.intValue);
			EditorUtility.SetDirty(target);

			Debug.Log("Custome Inspector Changed!");
		}
		
		// Draw the Buttons
		if(GUILayout.Button("Generate New Agent")){
			if(KeepSeed_Prop.boolValue == true)
				Random.InitState(TubeAgentSeed_Prop.intValue);
			TubeAgentScript.AgentPosition = AgentPosition_Prop.vector3Value;
			TubeAgentScript.GenerateAgent();
		}

		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Add Piece")){
			if(KeepSeed_Prop.boolValue == true)
				Random.InitState(TubeAgentSeed_Prop.intValue);

			TubeAgentScript.AddPiece();
		}
		EditorGUILayout.PropertyField( PiecesPerTick_Prop );
		

		GUILayout.EndHorizontal();
		if(GUILayout.Button("Regenerate")){
			if(KeepSeed_Prop.boolValue == true)
				Random.InitState(TubeAgentSeed_Prop.intValue);
			TubeAgentScript.UpdateIndexNumber();
		}
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Delete last"))
		{	
			TubeAgentScript.DeleteLastTubeAgent();
		}
		if(GUILayout.Button("Delete all agents"))
		{
			TubeAgentScript.DeleteAllAgents();
		}
		GUILayout.EndHorizontal();
				
		// Apply properties
		serializedObject.ApplyModifiedProperties ();
		TubeAgentScript.UpdateGizmo();
	}

}
