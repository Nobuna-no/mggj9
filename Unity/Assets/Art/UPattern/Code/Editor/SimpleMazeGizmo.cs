using UnityEngine;
using System.Collections;
using UnityEditor;

public class SimpleMazeGizmo : MonoBehaviour {

	public Vector3 v3GizmoPosition;
	public Vector3 v3Pivotposition;
		
	public int iGizmoWidth;
	public int iGizmoheight;
	private float fPivotGizmoSize = 0.2f;

	// Sets parameters for Gizmos and Handles
	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Handles.color = Color.yellow;
		//Debug.Log("IsSelected!");
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.left*iGizmoWidth*fPivotGizmoSize);
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.forward*iGizmoheight*fPivotGizmoSize);
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.right*iGizmoWidth*fPivotGizmoSize);
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.back*iGizmoheight*fPivotGizmoSize);
		Gizmos.DrawWireCube(v3GizmoPosition,new Vector3(iGizmoWidth,0,iGizmoheight));


		//Handles.DrawLine(v3GizmoPosition, v3GizmoPosition + Vector3.left*iGizmoWidth);
		//Handles.DrawWireArc(v3GizmoPosition, transform.up, -transform.right, 180, iGizmoWidth*iGizmoheight/20);
		//this.hideFlags = HideFlags.HideInHierarchy;


	}
	public void SetPivotPosition( Vector3 PivotPosition) {
		v3Pivotposition = new Vector3(PivotPosition.x,v3GizmoPosition.y,PivotPosition.z);
	}

	public void SetGizmoSize (int width, int height) {
		iGizmoWidth = width;
		iGizmoheight = height;
	}

	public void SetGizmoPosition (Vector3 gizmoPosition) {
		v3GizmoPosition = gizmoPosition;
	}
}
