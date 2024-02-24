using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;


public class GridShuffler : MonoBehaviour {	//public simpleMaze Shuffler;
	public List<GameObject> GoMaze = new List<GameObject>();

	[System.Serializable]
	public class Module{
		public GameObject GoModule;
		//[Tooltip("Division in %")]
		//public float Chance;
	}

	[TextArea(6,1)]
	public string Description;
	[HeaderAttribute("Shuffler Modules")]
	[Tooltip("Array of Modules you want to generate as a 2D Grid.")]public Module[] ModuleList = new Module[0];
	public enum enModes{SIMPLE_MAZE, DETERMINISTIC, RANDOM};
	[HeaderAttribute("Module Options")]
	[Tooltip("Option for module shuffling types. \n\n SIMPLE_MAZE: Places the Modules with a random 90° turn like the Simple Maze. \nDETERMINISTIC: Places the modules deterministically by turns. \nBY_CHANCE: Generates the modules by the set chance. (This Option is still work in progress and currently not working) \nRANDOM: Randomly picks the modules from the module list.")]public enModes GeneratorMode;
	[Tooltip("Randomizes the rotation of the modules set in the array.")]public bool RandomizeRotation = false;
	[Tooltip("Sets an position offset for the modules.")]public float ModuleOffset = 0;
	public bool RandomizeScale = false;
	public float Scale = 0;
	[HeaderAttribute("Maze Settings")]
	[Tooltip("The name of the generated asset grid in scene.")]public string MazeName;
	[Tooltip("The position of the generated asset grid in scene.")]public Vector3 MazePosition;
	[ReadOnlyAttribute][Tooltip("The index number of the asset grid.")]public int IndexNumber;
	[Tooltip("The width of the asset grid.")]public int MazeWidth;
	[Tooltip("The height of the asset grid.")]public int MazeHeight;
	[ReadOnlyAttribute][Tooltip("The number of modules that will be generated.")]public int ModuleNumber;
	[Tooltip("The current seed of the asset grid.")]public int Seed;
	[Tooltip("Flag true to keep the seed unchanged after asset grid generation.")]public bool KeepSeed;
	private GameObject GoPiece;
	private GameObject GoGizmo;
	public enum enPivotpoint{CENTER, TOP, BOTTOM, LEFT, RIGHT, FREE};
	[HeaderAttribute("Pivot Settings")][Tooltip("Position of the Pivot Point. Pivot Point can be altered if set to FREE")]
	public enPivotpoint Pivot;
	[SerializeField]
	[Tooltip("Current Pivot Point.")]private Vector2 PivotPoint;

	public void InitMazeString() {
		Description =  "This is a shuffler. Use the Shuffler to arrange multiple Assets in your scene. \n\nPress Tab in Scene View to generate a Grid Shuffler at mouse position.";
	}

	
	public void GenerateShuffler() {
		int iDeterministicModule = 0;
		GoMaze.Add(new GameObject());
		GoMaze[GoMaze.Count -1].name = MazeName + "_" + (GoMaze.Count-1)  + " (" + MazeWidth + "x" + MazeHeight + " ) " + Pivot;

		for(int w = 0; w < ModuleList.Length; w++) {
			if (ModuleList[w].GoModule != null) {
			}
			else {
				print("Please choose a Module");
				ModuleList[w].GoModule = Resources.Load ("EmptyGo") as GameObject;
			}
		}

		float ElapsedTime  =0;
		float InitTime =   Time.realtimeSinceStartup;

		for (int i = 0; i < MazeWidth; i++) {
	        if(ElapsedTime>5f) {
				Debug.Log("Grid Shuffler aborted. Please choose a smaller array size. ");
				break;
			}
			for (int j = 0; j < MazeHeight; j++) {
				ElapsedTime = Time.realtimeSinceStartup - InitTime;

				switch( GeneratorMode ) {
				case enModes.DETERMINISTIC: 
					GoPiece = Instantiate(ModuleList[iDeterministicModule].GoModule, new Vector3( ModuleOffset * i + i - PivotPoint.x + 0.5f, 0, ModuleOffset * j + j - PivotPoint.y + 0.5f),  Quaternion.identity) as GameObject;
					iDeterministicModule++;
					if(iDeterministicModule >= ModuleList.Length)
						iDeterministicModule = 0;
					break; 
				case enModes.RANDOM: 
					if(RandomizeRotation == true) 
						GoPiece = Instantiate(ModuleList[Random.Range(0,ModuleList.Length)].GoModule, new Vector3(ModuleOffset * i +  i - PivotPoint.x + 0.5f, 0,ModuleOffset * j +  j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, Random.Range(0,360), 0)) as GameObject;
					else
						GoPiece = Instantiate(ModuleList[Random.Range(0,ModuleList.Length)].GoModule, new Vector3(ModuleOffset * i +  i - PivotPoint.x + 0.5f, 0,ModuleOffset * j +  j - PivotPoint.y + 0.5f),  Quaternion.identity) as GameObject;
					break; 
				case enModes.SIMPLE_MAZE: 
					if(RandomizeRotation == true) {
						GoPiece = Instantiate(ModuleList[Random.Range(0,ModuleList.Length)].GoModule, new Vector3(ModuleOffset * i +  i - PivotPoint.x + 0.5f, 0,ModuleOffset * j +  j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, Random.Range(0,360), 0)) as GameObject;
					}
					else {
						if (Random.Range(0,2) == 0 ) {
							GoPiece = Instantiate(ModuleList[Random.Range(0,ModuleList.Length)].GoModule, new Vector3(ModuleOffset * i +  i - PivotPoint.x + 0.5f, 0,ModuleOffset * j +  j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 90, 0)) as GameObject;
						}
						else {
							GoPiece = Instantiate(ModuleList[Random.Range(0,ModuleList.Length)].GoModule, new Vector3(ModuleOffset * i +  i - PivotPoint.x + 0.5f, 0,ModuleOffset * j +  j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 0, 0)) as GameObject;
						}
					}
					break; 
				}

				GoPiece.name = "Piece (" + i + " | " + j + " ) ";
				GoPiece.transform.parent = GoMaze[IndexNumber].gameObject.transform;
			}
		}
		//}
		UpdateIndexNumber();
		//GoMaze[GoMaze.Count -1].AddComponent<GridShufflerInScene>();
		//GoMaze[GoMaze.Count -1].GetComponent<GridShufflerInScene>().GetMazeInSceneProperties(this);
		//GoMaze[GoMaze.Count -1].GetComponent<simpleMaze>() = this.gameObject.;
		GoMaze[GoMaze.Count -1].transform.position =  new Vector3(MazePosition.x + PivotPoint.x - MazeWidth*0.5f - ModuleOffset* (MazeWidth-1) *0.5f,MazePosition.y,MazePosition.z + PivotPoint.y - MazeHeight*0.5f-ModuleOffset* (MazeWidth-1) *0.5f);
		if(KeepSeed == false)
			Seed = UnityEngine.Random.Range(0, 9999999);
	}
	// Deleting One Maze
	public void  DeleteOneBitMaze() {
		if(IndexNumber >= 1 ) {
			GameObject.DestroyImmediate(GoMaze[GoMaze.Count-1]);
			GoMaze.RemoveAt(GoMaze.Count-1);
			UpdateIndexNumber ();
		}
		else 
			print ("Generate at least one maze!");
		
		//GoMaze.Clear();
		UpdateIndexNumber ();
	}
	// Deleting All Mazes
	public void  DeleteAll() {
		if(GoMaze.Count >= 1 ) {
			
			for (int i = 0; i <= GoMaze.Count-1; i++) {
				GameObject.DestroyImmediate(GoMaze[i]);
			}
			
		}
		else {
			print ("Generate at least one maze!");
		}
		//print ("Index is = " + IndexNumber);
		GoMaze.Clear();
		UpdateIndexNumber ();
	}
	public int UpdateIndexNumber () {
		return IndexNumber = GoMaze.Count;
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
			if(GoGizmo.GetComponent<GridShufflerGizmo>() == null) {
				GoGizmo.AddComponent<GridShufflerGizmo>();
				GoGizmo.AddComponent<GridShufflerGizmo>().name = "GIZMO OBJECT";
				GoGizmo.GetComponent<GridShufflerGizmo>().SetGizmoPosition(MazePosition);
				GoGizmo.GetComponent<GridShufflerGizmo>().SetGizmoSize(MazeWidth, MazeHeight);
			}
		}
	}
	public void DestroyGizmoGameobject() {
		DestroyImmediate(GoGizmo.gameObject);
	}
	
	public void UpdateGizmo() {
		GoGizmo.GetComponent<GridShufflerGizmo>().SetGizmoPosition(MazePosition);
		GoGizmo.GetComponent<GridShufflerGizmo>().SetGizmoSize(MazeWidth, MazeHeight);
		GoGizmo.GetComponent<GridShufflerGizmo>().SetPivotPosition(new Vector3(MazePosition.x + PivotPoint.x - MazeWidth*0.5f-ModuleOffset*MazeWidth*0.5f,this.transform.position.y,MazePosition.z + PivotPoint.y - MazeHeight*0.5f-ModuleOffset*MazeWidth*0.5f));
		GoGizmo.GetComponent<GridShufflerGizmo>().GetPivotPosition(PivotPoint);
		GoGizmo.GetComponent<GridShufflerGizmo>().SetOffset(ModuleOffset);
	}
}
