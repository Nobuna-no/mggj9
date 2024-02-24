using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

public class TubeAgent : MonoBehaviour {

	public List<GameObject> TubeAgents = new List<GameObject>();

	// Public Variables
	[TextArea(6,1)]
	public string Description;
	[HeaderAttribute("Tube Agent Modules")]
	[Tooltip("Array of Modules you want to be part of a Tube Agent path. All assets need a GameObject with the name of the connector. Convert selected assets in scene by pressing tab with the convert to context menu.")]public GameObject[] ModuleList = new GameObject[0];
	[HeaderAttribute("Agent Settings")]
	[Tooltip("The name of the Tube Agent path in scene.")]public string AgentName;
	[Tooltip("IMPORTANT: The name of a GameObject that is currently used as a connection point of the path. It also uses the rotation of this GameObject for further path deformation. Only change if your assets have GameObjects with this name.")]public string ConnectorName;
	[Tooltip("The name of the generated Tube Agent path in scene.")]public Vector3 AgentPosition;
	[Tooltip("Flag true to rotate each generated sector randomly around its parent connector.")]public bool RandomRotation = false;
	[Tooltip("Use this value for a additive rotation.")]public float RotationStep;
	[SerializeField][ReadOnlyAttribute] 
	public int IndexNumber;
	[Tooltip("The current seed of the Tube Agent Maze.")]public int TubeAgentSeed;
	[Tooltip("Flag true to keep the seed unchanged after Tube Agent  generation. \n\n IMPORTANT: This also generates self-similar structures when used with the PiecesPerTick field.")]public bool KeepSeed;
	[Tooltip("Number of path pieces that are added per clicking on the Add piece button.")]public int PiecesPerTick;

	// Private Variables
	[Tooltip("The position of the generated Tube Agent path in scene.")]public Vector3 V3InitPosition;
	private GameObject AgentPiece;
	private GameObject LastPieceParent;
	private GameObject PreviousConnector;
	private GameObject GoGizmo;
	private int iPieceNumber;

	public void InitMazeString() {
		Description =  "A Tube Agent is a path following generator that can be used to chain gameobjects. Simply create Prefabs with a connector, to stitch the modules together.\n\nPress Tab in Scene View to generate a Tube Agent at mouse position. \n\nIMPORTANT: Always use Modules with mantled GameObjects inside that fit to the current connector name of the Tube Agent. Convert selected assets in scene by pressing tab with the COVERT TO context menu option.";
	}

	public void SetInitPosition() {
		V3InitPosition = AgentPosition;
	}

	// Sets the parent Agent
	public void GetParentAgent() {
		LastPieceParent = TubeAgents[TubeAgents.Count -1];
	}

	// Sets the previous connector Gameobject
	public void SetConnector(GameObject Connector) {
		PreviousConnector = Connector;
	}

	// Sets initial settings like name, position and then adds an Agent piece
	public void GenerateAgent() {
		iPieceNumber = 0;
		TubeAgents.Add(new GameObject());
		AgentPiece = null;
		TubeAgents[TubeAgents.Count -1].transform.position = V3InitPosition;
		this.transform.position = AgentPosition;
		TubeAgents[TubeAgents.Count -1].name = AgentName + "_" + (TubeAgents.Count-1);
		AddPiece ();
	}

	// Add piece(s) to the path
	public void  AddPiece () {
		if(TubeAgents.Count != 0) {
			GetParentAgent();

			// Check if its the first piece
			if( AgentPiece != null) {
				SetConnector(AgentPiece.transform.Find(ConnectorName).gameObject);
			}
			else {
				SetConnector(this.gameObject);
				SetInitPosition();
			}
	
			// Check if modules are selected
			for(int w = 0; w < ModuleList.Length; w++) {
				if (ModuleList[w] != null) {
				}
				else {
					print("Please choose a Module");
					ModuleList[w] = Resources.Load ("EmptyGo") as GameObject;
				}
			}
			for(int p = 0; p < PiecesPerTick; p++) {
				iPieceNumber++;
				int iChoosenmodul = Random.Range(0,ModuleList.Length);
				AgentPiece = Instantiate(ModuleList[iChoosenmodul], PreviousConnector.transform.position,  Quaternion.Euler(PreviousConnector.transform.eulerAngles)) as GameObject;
		
				if(RandomRotation == true) {
					AgentPiece.transform.Rotate(0,Random.Range(0,360),0);
				}
				else {
					AgentPiece.transform.Rotate(0,RotationStep,0);
				}
		
				AgentPiece.name = "Piece "+ iPieceNumber + ": " + ModuleList[iChoosenmodul].name + " Seed: "+ TubeAgentSeed;
				AgentPiece.transform.parent = LastPieceParent.gameObject.transform;
			
				UpdateIndexNumber();
				TubeAgents[TubeAgents.Count - 1].transform.position =  new Vector3(AgentPosition.x,AgentPosition.y,AgentPosition.z);
				if(AgentPiece.transform.childCount > 0) {
					if(AgentPiece.transform.Find(ConnectorName).gameObject != null)
						SetConnector(AgentPiece.transform.Find(ConnectorName).gameObject );	
				}	
				else 
					Debug.Log("Please use a Tube Agent Module as: " + ModuleList[iChoosenmodul].transform.name);

			}
			if(KeepSeed == false)
				TubeAgentSeed = Random.Range(0, 9999999);
		}
		else
			Debug.Log("Please Generate a Tube Agent first.");
	}

	// Updates index number
	public int UpdateIndexNumber () {
		return IndexNumber = TubeAgents.Count;
	}

	// Updates Agent position
	public Vector3 UpdateMazePosition () {
		return AgentPosition = transform.position;
	}

	// Deleting last Agents
	public void  DeleteLastTubeAgent() {
		if(IndexNumber >= 1 ) {
			GameObject.DestroyImmediate(TubeAgents[TubeAgents.Count -1]);
			TubeAgents.RemoveAt(TubeAgents.Count-1);
			UpdateIndexNumber ();
		}
		else 
			print ("Generate at least one maze!");
		
		//GoMaze.Clear();
		UpdateIndexNumber ();
	}

	// Deleting all Agents
	public void  DeleteAllAgents() {
		if(TubeAgents.Count >= 1 ) {
			
			for (int i = 0; i <= TubeAgents.Count-1; i++) {
				GameObject.DestroyImmediate(TubeAgents[i]);
			}
			
		}
		else {
			print ("Generate at least one maze!");
		}
		TubeAgents.Clear();
		UpdateIndexNumber ();
	}

	// Generates Gizmo object
	public void GenerateGizmoGameobject() {
		if(GoGizmo == null) {
			GoGizmo = Instantiate(Resources.Load ("GizmoNull"), AgentPosition,  Quaternion.identity) as GameObject;
			if(GoGizmo.GetComponent<TubeAgentGizmo>() == null) {
				GoGizmo.name = "GIZMO OBJECT";
				GoGizmo.AddComponent<TubeAgentGizmo>();
				GoGizmo.GetComponent<TubeAgentGizmo>().SetGizmoPosition(AgentPosition);
			}
		}
	}

	// Destroys Gizmo object
	public void DestroyGizmoGameobject() {
		DestroyImmediate(GoGizmo.GetComponent<TubeAgentGizmo>());
		DestroyImmediate(GoGizmo.gameObject);
	}
	
	// Updates Gizmo object
	public void UpdateGizmo() {
		GoGizmo.GetComponent<TubeAgentGizmo>().SetGizmoPosition(AgentPosition);
	}
}
