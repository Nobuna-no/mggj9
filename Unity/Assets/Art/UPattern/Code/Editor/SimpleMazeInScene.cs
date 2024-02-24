using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class SimpleMazeInScene : MonoBehaviour {
	public SimpleMaze sMaze;

	[TextArea(6,1)]
	public string Description;
	[HeaderAttribute("Module")]
	public GameObject GoSimpleMazePiece;
	//public GameObject GoEmpty = new GameObject();
	public List<GameObject> GoMaze= new List<GameObject>();
	[HeaderAttribute("Maze Properties")]
	public string MazeName;
	public Vector3 MazePosition;
	public int IndexNumber;
	public int MazeWidth;
	public int MazeHeigth;
	public int Seed;
	public bool KeepSeed;
	private GameObject GoPiece;
	private GameObject GoGizmo;
	
	public enum enPivotpoint{CENTER, TOP, BOTTOM, LEFT, RIGHT, FREE};
	public enPivotpoint Pivot;
	public Vector2 PivotPoint;
	SimpleMazeInScene.enPivotpoint PivotSelection;



	public void GetMazeInSceneProperties(SimpleMaze MazeProperties) {
		//EditorUtility.CopySerialized(MazeProperties.GetComponent<simpleMaze>(), this.GetComponent<simpleMaze>());
		sMaze = MazeProperties;
		Description = MazeProperties.Description;
		GoSimpleMazePiece = MazeProperties.SimpleMazeModule;
		GoMaze = MazeProperties.LSimpleMaze;
		MazeName = MazeProperties.MazeName;
		MazePosition = MazeProperties.MazePosition;
		Seed = MazeProperties.Seed;
		KeepSeed = MazeProperties.KeepSeed;
		IndexNumber = MazeProperties.IndexNumber;
		MazeWidth = MazeProperties.MazeWidth;
		MazeHeigth = MazeProperties.MazeHeight;	
		PivotSelection = (SimpleMazeInScene.enPivotpoint)MazeProperties.Pivot;

		Description = 	"This is the generated maze. Change values and click on regenerate!";
		//simpleMazeInScene.enPivotpoint PivotSelection = Pivot;
		print(PivotSelection);
		PivotPoint = MazeProperties.GetPivot();
	}
	public Vector2 UpdatePivotPosition() {
		switch( PivotSelection ) {
		case SimpleMazeInScene.enPivotpoint.CENTER: 
			return PivotPoint = new Vector2((float)MazeWidth/2,(float)MazeHeigth/2);
		case SimpleMazeInScene.enPivotpoint.BOTTOM: 
			return PivotPoint = new Vector2((float)MazeWidth/2,0);
		case SimpleMazeInScene.enPivotpoint.TOP: 
			return PivotPoint = new Vector2((float)MazeWidth/2,(float)MazeHeigth);
		case SimpleMazeInScene.enPivotpoint.LEFT: 
			return PivotPoint = new Vector2(0,(float)MazeHeigth/2);
		case SimpleMazeInScene.enPivotpoint.RIGHT: 
			return PivotPoint = new Vector2((float)MazeWidth,(float)MazeHeigth/2);
		}
		return PivotPoint;
	}

	public Vector3 InitInSceneMazePosition () {
		return MazePosition = transform.position;
	}
	public Vector3 UpdateInSceneMazePosition () {
		return transform.position = MazePosition;
	}
	public void RegenerateSimpleMaze () {
		UpdateInSceneMazePosition ();
		DestroyMazepieces();
		this.name = MazeName + "_" + IndexNumber  + "(" + MazeWidth + "x" + MazeHeigth + ")" + PivotSelection;
		
		for (int i = 0; i < MazeWidth; i++) {
			for (int j = 0; j < MazeHeigth; j++) {
				if (Random.Range(0,2) == 0 ) {
					GoPiece = Instantiate(GoSimpleMazePiece, new Vector3(MazePosition.x + i - PivotPoint.x + 0.5f, MazePosition.y, MazePosition.z + j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 90, 0)) as GameObject;
				}
				else {
					GoPiece = Instantiate(GoSimpleMazePiece, new Vector3(MazePosition.x + i - PivotPoint.x + 0.5f, MazePosition.y, MazePosition.z + j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 0, 0)) as GameObject;
				}
				GoPiece.name = "Piece (" + i + " | " + j + " ) ";
				GoPiece.transform.parent = this.gameObject.transform;
			}
		}
		if(KeepSeed == false)
			Seed = Random.Range(0, 9999999);
		//GoMaze[GoMaze.Count -1].GetComponent<simpleMaze>() = this.gameObject.;
		//this.transform.position =  new Vector3(MazePosition.x + PivotPoint.x - MazeWidth*0.5f,MazePosition.y,MazePosition.z + PivotPoint.y - MazeHeigth*0.5f);

	}
	public void DestroyMazepieces() {
		
		Transform[] allTransforms = gameObject.GetComponentsInChildren<Transform>();
		
		foreach(Transform childObjects in allTransforms){
			if(childObjects != null)
				if(transform.IsChildOf(childObjects.transform) == false)
					DestroyImmediate(childObjects.gameObject);
		}
	}
	
	public SimpleMaze UpdateInSceneMazeProperties() {
		UpdatePivotPosition();
		this.name = MazeName + ":" + IndexNumber  + "(" + MazeWidth + "x" + MazeHeigth + ")" + PivotSelection;
		return sMaze;
	}
/*
	public void GenerateInSceneGizmoGameobject() {
		if(GoGizmo == null) {
			GoGizmo = Instantiate(Resources.Load ("GizmoNull"), new Vector3( PivotPoint.x, MazePosition.y, PivotPoint.y),  Quaternion.identity) as GameObject;
			if(GoGizmo.GetComponent<simpleMazeGizmo>() == null) {
				GoGizmo.AddComponent<simpleMazeGizmo>();
				GoGizmo.AddComponent<simpleMazeGizmo>().name = "GIZMO OBJECT";
				GoGizmo.GetComponent<simpleMazeGizmo>().SetGizmoPosition(MazePosition);
				GoGizmo.GetComponent<simpleMazeGizmo>().SetGizmoSize(MazeWidth, MazeHeigth);
			}
		}
	}
	public void DestroyInSceneGizmoGameobject() {
		//if(IndexNumber >= 0 ) {
		DestroyImmediate(GoGizmo.gameObject);
		//}
	}
	public void UpdateInSceneGizmo() {
		GoGizmo.GetComponent<simpleMazeGizmo>().SetGizmoPosition(MazePosition);
		GoGizmo.GetComponent<simpleMazeGizmo>().SetGizmoSize(MazeWidth, MazeHeigth);
		GoGizmo.GetComponent<simpleMazeGizmo>().SetPivotPosition(new Vector3(MazePosition.x + PivotPoint.x - MazeWidth*0.5f,this.transform.position.y,MazePosition.z + PivotPoint.y - MazeHeigth*0.5f));
	}*/
}
