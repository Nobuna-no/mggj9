using UnityEngine;
using System.Collections;
using UnityEditor;

public class MoveAgentGizmo : MonoBehaviour {

	public Vector3 v3GizmoPosition;
	public Vector3 v3Pivotposition;
	public Vector2 v2Pivot;
	
	public int iGizmoWidth;
	public int iGizmoHeight;
	public float fOffset = 0;
	
	private float fPivotGizmoSize = 0.2f;
	
	
	void OnDrawGizmos() {
		Gizmos.color = Color.green;
		Handles.color = Color.yellow;
		//Debug.Log("IsSelected!");
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.left*iGizmoWidth*fPivotGizmoSize);
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.forward*iGizmoHeight*fPivotGizmoSize);
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.right*iGizmoWidth*fPivotGizmoSize);
		Handles.DrawLine(v3Pivotposition, v3Pivotposition + Vector3.back*iGizmoHeight*fPivotGizmoSize);
		if(fOffset == 0)
			Gizmos.DrawWireCube(v3GizmoPosition,new Vector3(iGizmoWidth,0,iGizmoHeight));
		else {
			for (int i = 0; i < iGizmoWidth; i++) {			
				for (int j = 0; j < iGizmoHeight; j++) {
					Vector3 v3UsedModulePosition = new Vector3(+fOffset * (i+0.5f) +  i - v2Pivot.x + 0.5f, v3Pivotposition.y,fOffset * (j+0.5f) +  j - v2Pivot.y + 0.5f) + v3GizmoPosition;
					//Vector3 v3UsedModulePosition = new Vector3(fOffset*i  +  (v3GizmoPosition.x - i + iGizmoWidth*0.5f-0.5f),v3GizmoPosition.y,fOffset*j + v3GizmoPosition.z - j + iGizmoHeight*0.5f-0.5f);
					
					//Gizmos.DrawIcon(v3UsedModulePosition, "M01");
					Gizmos.DrawWireCube(v3UsedModulePosition, new Vector3(1,0,1));
					//Handles.color = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
					//Handles.Label(v3UsedModulePosition, "M01");
					
				}
			}
		}
		
		
	}
	public void SetPivotPosition( Vector3 PivotPosition) {
		v3Pivotposition = new Vector3(PivotPosition.x,v3GizmoPosition.y,PivotPosition.z);
	}
	
	public void GetPivotPosition(Vector2 Pivot) {
		v2Pivot = Pivot;
	}
	
	public void SetOffset( float Offset) {
		fOffset = Offset;
	}
	
	public void SetGizmoSize (int width, int height) {
		iGizmoWidth = width;
		iGizmoHeight = height;
	}
	
	public void SetGizmoPosition (Vector3 gizmoPosition) {
		v3GizmoPosition = gizmoPosition;
	}
}
