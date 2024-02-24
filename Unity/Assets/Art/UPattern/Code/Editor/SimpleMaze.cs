using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class SimpleMaze : MonoBehaviour {
	// public list
	public List<GameObject> LSimpleMaze= new List<GameObject>();

	[TextArea(6,1)]
	public string Description;
	[HeaderAttribute("Module")]
	[Tooltip("Select a Gameobject that u want to use as a Simple Maze module.")]public GameObject SimpleMazeModule;
	//public GameObject GoEmpty = new GameObject();
	[HeaderAttribute("Maze Settings")]
	[Tooltip("The name of the generated maze in scene.")]public string MazeName;
	[Tooltip("The position of the generated maze in scene.")]public Vector3 MazePosition;
	[ReadOnlyAttribute][Tooltip("The index number of the generated maze.")]public int IndexNumber;
	[Tooltip("The width of the generated maze.")]public int MazeWidth;
	[Tooltip("The height of the generated maze.")]public int MazeHeight;
	[ReadOnlyAttribute][Tooltip("The number of modules that will be generated.")]public int ModuleNumber;
	[Tooltip("The current seed of the Simple Maze.")]public int Seed;
	[Tooltip("Flag true to keep the seed unchanged after Simple Maze generation.")]public bool KeepSeed;
	private GameObject GoPiece;
	private GameObject GoGizmo;
	public enum enPivotpoint{CENTER, TOP, BOTTOM, LEFT, RIGHT, FREE};
	[HeaderAttribute("Pivot Settings")]
	[Tooltip("Position of the Pivot Point. Pivot Point can be altered if set to FREE")]public enPivotpoint Pivot;
	[SerializeField]
	[Tooltip("Current Pivot Point.")]private Vector2 PivotPoint;

	public void InitMazeValues() {
		Description =  "This is a simple maze generator. Change settings here and press Generate Maze.\n\nPress Tab in Scene View to generate a Simple Maze on mouse position.";
	}

	public void GenerateSimpleMaze() {
		if (SimpleMazeModule != null) {
			LSimpleMaze.Add(new GameObject());
			LSimpleMaze[LSimpleMaze.Count -1].name = MazeName + "_" + (LSimpleMaze.Count-1)  + "(" + MazeWidth + "x" + MazeHeight + ")" + Pivot;

			float ElapsedTime  =0;
			float InitTime =   Time.realtimeSinceStartup;

			for (int i = 0; i < MazeWidth; i++) {
	        	if(ElapsedTime>5f) {
					Debug.Log("Simple Maze aborted. Please choose a smaller array size. ");
					break;
				}
				for (int j = 0; j < MazeHeight; j++) {
					ElapsedTime = Time.realtimeSinceStartup - InitTime;
					if (Random.Range(0,2) == 0 ) {
						GoPiece = Instantiate(SimpleMazeModule, new Vector3( i - PivotPoint.x + 0.5f, 0, j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 90, 0)) as GameObject;
					}
					else {
						GoPiece = Instantiate(SimpleMazeModule, new Vector3( i - PivotPoint.x + 0.5f, 0, j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 0, 0)) as GameObject;
					}
					GoPiece.name = "Piece (" + i + " | " + j + " ) ";
					GoPiece.transform.parent = LSimpleMaze[IndexNumber].gameObject.transform;
				}
			}
			LSimpleMaze[LSimpleMaze.Count -1].AddComponent<SimpleMazeInScene>();
			LSimpleMaze[LSimpleMaze.Count -1].GetComponent<SimpleMazeInScene>().GetMazeInSceneProperties(this);
			//GoMaze[GoMaze.Count -1].GetComponent<simpleMaze>() = this.gameObject.;
			LSimpleMaze[LSimpleMaze.Count -1].transform.position =  new Vector3(MazePosition.x + PivotPoint.x - MazeWidth*0.5f,MazePosition.y,MazePosition.z + PivotPoint.y - MazeHeight*0.5f);
			UpdateIndexNumber();
			if(KeepSeed == false)
				Seed = Random.Range(0, 9999999);
		}
		else
			print("Please choose a Module");
	}
	// Deleting One Maze
	public void  DeleteOneBitMaze() {
		if(IndexNumber >= 1 ) {
			GameObject.DestroyImmediate(LSimpleMaze[LSimpleMaze.Count-1]);
			LSimpleMaze.RemoveAt(LSimpleMaze.Count-1);
			UpdateIndexNumber ();
		}
		else 
			print ("Generate at least one maze!");

		//GoMaze.Clear();
		UpdateIndexNumber ();
	}
	// Deleting All Mazes
	public void  DeleteAll() {
		if(LSimpleMaze.Count > 1 ) {
			
			for (int i = 0; i <= LSimpleMaze.Count-1; i++) {
				GameObject.DestroyImmediate(LSimpleMaze[i]);
			}
			
		}
		else {
			print ("Generate at least one maze!");
		}
		//print ("Index is = " + IndexNumber);
		LSimpleMaze.Clear();
		UpdateIndexNumber ();
	}
	public int UpdateIndexNumber () {
		return IndexNumber = LSimpleMaze.Count;
	}
	public Vector3 UpdateMazePosition () {
		return MazePosition = transform.position;
	}

	public void SetPivot(float x, float y) {
		PivotPoint = new Vector2(x, y);
	}
	public Vector2 GetPivot() {
		return PivotPoint;
	}

	public void GenerateGizmoGameobject() {
		if(GoGizmo == null) {
			GoGizmo = Instantiate(Resources.Load ("GizmoNull"), new Vector3( GetPivot().x, MazePosition.y, GetPivot().y),  Quaternion.identity) as GameObject;
			if(GoGizmo.GetComponent<SimpleMazeGizmo>() == null) {
				GoGizmo.AddComponent<SimpleMazeGizmo>();
				GoGizmo.AddComponent<SimpleMazeGizmo>().name = "GIZMO OBJECT";
				GoGizmo.GetComponent<SimpleMazeGizmo>().SetGizmoPosition(MazePosition);
				GoGizmo.GetComponent<SimpleMazeGizmo>().SetGizmoSize(MazeWidth, MazeHeight);
			}
		}
	}
	public void DestroyGizmoGameobject() {
		//if(IndexNumber >= 0 ) {
			DestroyImmediate(GoGizmo.gameObject);
		//}
	}

	public void UpdateGizmo() {
		GoGizmo.GetComponent<SimpleMazeGizmo>().SetGizmoPosition(MazePosition);
		GoGizmo.GetComponent<SimpleMazeGizmo>().SetGizmoSize(MazeWidth, MazeHeight);
		GoGizmo.GetComponent<SimpleMazeGizmo>().SetPivotPosition(new Vector3(MazePosition.x + PivotPoint.x - MazeWidth*0.5f,this.transform.position.y,MazePosition.z + PivotPoint.y - MazeHeight*0.5f));
	}

}
