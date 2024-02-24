using UnityEngine;
using System.Collections;
using UnityEditor;

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

// This class provides handles for the Tube Agent.
public class TubeAgentGizmo : MonoBehaviour {

	// Public Variables
	public Vector3 v3GizmoPosition;
	
	// private Variables
	private float fPivotGizmoSize = 2f;
	
	// Draws Handles
	void OnDrawGizmos() {
		Handles.color = Color.magenta;
		Handles.ArrowHandleCap(2,v3GizmoPosition, Quaternion.Euler(-90,0,0),fPivotGizmoSize, EventType.Ignore);
		Handles.DrawLine(v3GizmoPosition, v3GizmoPosition + Vector3.left*fPivotGizmoSize);
		Handles.DrawLine(v3GizmoPosition,v3GizmoPosition + Vector3.forward*fPivotGizmoSize);
		Handles.DrawLine(v3GizmoPosition, v3GizmoPosition + Vector3.right*fPivotGizmoSize);
		Handles.DrawLine(v3GizmoPosition, v3GizmoPosition + Vector3.back*fPivotGizmoSize);
	}
	
	// Sets parameters for Handles
	public void SetGizmoPosition (Vector3 gizmoPosition) {
		v3GizmoPosition = gizmoPosition;
	}
}
