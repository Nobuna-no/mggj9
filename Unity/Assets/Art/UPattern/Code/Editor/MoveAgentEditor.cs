using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

// This custom editor class is part of the Tupe Agent generator
[CustomEditor(typeof(MoveAgent))]
public class MoveAgentEditor : Editor {

	// Private Variables
	private MoveAgent MoveAgentScript;
	private Texture2D LableTexture;
	private int iModuleClassArraySize;
	private bool bSizeChanged = false;
	
	// Serialized Editor Properties
	public SerializedProperty
		Description_Prop,
		ModuleList_Prop,
		MapType_Prop,
		AgentName_Prop,
		AgentPosition_Prop,
		IndexNumber_Prop,
		AgentWidth_Prop,
		AgentHeight_Prop,
		MovePos_Prop,
		PositionFactor_Prop,
		MoveScale_Prop,
		ScaleFactor_Prop,
		MoveRot_Prop,
		RotationFactor_Prop,
		TextureTilingX_Prop,
		TextureTilingY_Prop,
		InversTexture_Prop,
		Clamp_Prop,
		InversClamp_Prop,
		ClampAlpha_Prop,
		NoiseResolution_Prop,
		NoisePosX_Prop,
		NoisePosY_Prop,
		MapTexture_Prop,
		Pivot_Prop,
		PivotPoint_Prop,
		Refresh_Prop;


	
	void OnEnable () {
		LableTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/UPattern/Resources/MoveAgentIcon_MoveAgentMap.png", typeof(Texture2D));

		// Setup the SerializedProperties
		Description_Prop = serializedObject.FindProperty ("Description");
		ModuleList_Prop = serializedObject.FindProperty ("MoveAgentModule");
		MapType_Prop =serializedObject.FindProperty ("MapType");
		AgentName_Prop = serializedObject.FindProperty ("AgentName");
		AgentPosition_Prop = serializedObject.FindProperty ("AgentPosition");
		IndexNumber_Prop = serializedObject.FindProperty ("IndexNumber");
		AgentWidth_Prop = serializedObject.FindProperty ("AgentWidth");
		AgentHeight_Prop = serializedObject.FindProperty ("AgentHeight");
		MovePos_Prop = serializedObject.FindProperty ("MovePos");
		PositionFactor_Prop = serializedObject.FindProperty ("PositionFactor");
		MoveScale_Prop = serializedObject.FindProperty ("MoveScale");
		ScaleFactor_Prop = serializedObject.FindProperty ("ScaleFactor");
		MoveRot_Prop = serializedObject.FindProperty ("MoveRot");
		RotationFactor_Prop = serializedObject.FindProperty ("RotationFactor");
		TextureTilingX_Prop = serializedObject.FindProperty ("TextureTilingX");
		TextureTilingY_Prop = serializedObject.FindProperty ("TextureTilingY");
		InversTexture_Prop = serializedObject.FindProperty ("InversTexture");
		Clamp_Prop = serializedObject.FindProperty ("Clamp");
		InversClamp_Prop = serializedObject.FindProperty ("InversClamp");
		ClampAlpha_Prop = serializedObject.FindProperty ("ClampAlpha");
		NoiseResolution_Prop = serializedObject.FindProperty ("NoiseResolution");
		NoisePosX_Prop = serializedObject.FindProperty ("NoisePosX");
		NoisePosY_Prop = serializedObject.FindProperty ("NoisePosY");
		MapTexture_Prop = serializedObject.FindProperty ("MapTexture");
		Pivot_Prop = serializedObject.FindProperty ("Pivot");
		PivotPoint_Prop = serializedObject.FindProperty ("PivotPoint");
		Refresh_Prop = serializedObject.FindProperty ("AutoRefresh");
		
		MoveAgentScript = (MoveAgent)target;
		MoveAgentScript.UpdateIndexNumber();
		MoveAgentScript.InitMazeValues();
		MoveAgentScript.GetComponent<Transform>().hideFlags = HideFlags.HideInInspector;
		MoveAgentScript.GenerateGizmoGameobject();
	}
	
	void OnDisable () {
		//Debug.Log("NotSelected!");
		MoveAgentScript.DestroyGizmoGameobject();
	}



	public override void OnInspectorGUI()
	{
		MoveAgent.enPivotpoint PivotSelection = (MoveAgent.enPivotpoint)Pivot_Prop.enumValueIndex;
		MoveAgent.enMapping MapType = (MoveAgent.enMapping)MapType_Prop.enumValueIndex;

		MoveAgentScript.GetPivot();
		MoveAgentScript.UpdateIndexNumber();
		serializedObject.Update ();
		//DrawDefaultInspector();
		GUILayout.Label(LableTexture);
		GUILayout.Label("Move Agent Generator:", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox( Description_Prop.stringValue , MessageType.Info);
		EditorGUILayout.PropertyField( ModuleList_Prop, true );
		EditorGUILayout.PropertyField( AgentName_Prop );
		EditorGUILayout.PropertyField( AgentPosition_Prop );
		EditorGUILayout.PropertyField( IndexNumber_Prop );
		EditorGUI.BeginChangeCheck ();
		EditorGUILayout.PropertyField( AgentWidth_Prop );
		EditorGUILayout.PropertyField( AgentHeight_Prop );
		if (EditorGUI.EndChangeCheck()) {
			bSizeChanged = true;
		}
		EditorGUILayout.PropertyField( MapType_Prop );

		switch( MapType ) {
			case MoveAgent.enMapping.PEARLIN_NOISE: 
				EditorGUILayout.PropertyField( PositionFactor_Prop );
				EditorGUILayout.PropertyField( NoiseResolution_Prop );
				EditorGUILayout.PropertyField( NoisePosX_Prop );
				EditorGUILayout.PropertyField( NoisePosY_Prop );
				break; 
			case MoveAgent.enMapping.TEXTURE: 
				EditorGUILayout.PropertyField( MovePos_Prop );
				if (MovePos_Prop.boolValue == true) {
					EditorGUILayout.PropertyField( PositionFactor_Prop );
				}
				EditorGUILayout.PropertyField( MoveScale_Prop );
				if (MoveScale_Prop.boolValue == true) {
					EditorGUILayout.PropertyField(ScaleFactor_Prop );
				}
				EditorGUILayout.PropertyField( MoveRot_Prop );
				if (MoveRot_Prop.boolValue == true) {
					EditorGUILayout.PropertyField( RotationFactor_Prop );
				}

				EditorGUILayout.Separator();
				EditorGUILayout.PropertyField( TextureTilingX_Prop );
				EditorGUILayout.PropertyField( TextureTilingY_Prop );
				EditorGUILayout.BeginVertical("Box");
					EditorGUILayout.BeginHorizontal();
					EditorGUI.BeginChangeCheck ();
						MapTexture_Prop.objectReferenceValue = EditorGUILayout.ObjectField("MapTexture", MapTexture_Prop.objectReferenceValue, typeof (Texture2D), true);
					if (EditorGUI.EndChangeCheck()) {
					}
					EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
				EditorGUILayout.PropertyField( InversTexture_Prop );
				EditorGUILayout.PropertyField( Clamp_Prop );
				EditorGUILayout.PropertyField( InversClamp_Prop );
				EditorGUILayout.PropertyField( ClampAlpha_Prop );
				break; 
		}
		
		//EditorGUILayout.PropertyField( Pivot_Prop );
		EditorGUILayout.PropertyField( PivotPoint_Prop );




		// If Inspector has changed
		if( GUI.changed) {
			// Only unsingned int allowed in inspector
			if (AgentWidth_Prop.intValue < 1 ) {
				AgentWidth_Prop.intValue = 1;
			}
			if (AgentHeight_Prop.intValue < 1 ) {
				AgentHeight_Prop.intValue = 1;
			}
			if (NoiseResolution_Prop.floatValue <= 0.001f ) {
				NoiseResolution_Prop.floatValue = 0.001f ;
			}
			serializedObject.ApplyModifiedProperties ();

			if(Refresh_Prop.boolValue == true) {
				Regenerate(bSizeChanged);
				bSizeChanged = false;
			}
			
			//Debug.Log("Has Changed!");
			switch( PivotSelection ) {
			case MoveAgent.enPivotpoint.CENTER: 
				MoveAgentScript.SetPivot((float)AgentWidth_Prop.intValue/2, (float)AgentHeight_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = MoveAgentScript.GetPivot();
				break; 
			case MoveAgent.enPivotpoint.BOTTOM: 
				MoveAgentScript.SetPivot((float)AgentWidth_Prop.intValue/2, 0);
				PivotPoint_Prop.vector2Value = MoveAgentScript.GetPivot();
				break;
			case MoveAgent.enPivotpoint.TOP: 
				MoveAgentScript.SetPivot((float)AgentWidth_Prop.intValue/2, (float)AgentHeight_Prop.intValue);
				PivotPoint_Prop.vector2Value = MoveAgentScript.GetPivot();
				break; 
			case MoveAgent.enPivotpoint.LEFT: 
				MoveAgentScript.SetPivot(0, (float)AgentHeight_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = MoveAgentScript.GetPivot();
				break;
			case MoveAgent.enPivotpoint.RIGHT: 
				MoveAgentScript.SetPivot((float)AgentWidth_Prop.intValue , (float)AgentHeight_Prop.intValue/2);
				PivotPoint_Prop.vector2Value = MoveAgentScript.GetPivot();
				break; 
			case MoveAgent.enPivotpoint.FREE: 
				MoveAgentScript.SetPivot((float)AgentWidth_Prop.intValue , (float)AgentHeight_Prop.intValue);
				//PivotPoint_Prop.vector2Value = myScript.GetPivot();
				break; 
			}
			MoveAgentScript.GetPivot();
			
		}
		
		// Draw the Buttons
		if(GUILayout.Button("Generate Move Agent")){
			MoveAgentScript.GenerateMoveAgent(true);
		}

		EditorGUILayout.BeginHorizontal();		
		if(GUILayout.Button("Regenerate")){
			Regenerate(true);
		}
		EditorGUILayout.PropertyField( Refresh_Prop );
		
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Delete last"))
		{	
			MoveAgentScript.DeleteMoveAgent();
			MoveAgentScript.UpdateIndexNumber();
		}
		if(GUILayout.Button("Delete all"))
		{
			MoveAgentScript.DeleteAll();
			MoveAgentScript.UpdateIndexNumber();
		}
		EditorGUILayout.EndHorizontal();
		
		
		
		// Apply properties
		serializedObject.ApplyModifiedProperties ();
		MoveAgentScript.UpdateGizmo();
		
	}
	private void Regenerate(bool sizeChange) {


		//if(KeepSeed_Prop.boolValue == true)
		//	Random.seed = MoveAgentSeed_Prop.intValue;
		MoveAgentScript.DeleteMoveAgent();
		//MoveAgentScript.UpdateIndexNumber();
		MoveAgentScript.GenerateMoveAgent(sizeChange);
		serializedObject.Update ();
		serializedObject.ApplyModifiedProperties ();
	}
}
