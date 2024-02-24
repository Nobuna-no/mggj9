using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// 	----------------------------------- //
//	author	: Lewnatic					//
//	email	: lewnatic@live.com	 		//
//	twitter : twitter.com/Lewnaticable	//
//	-----------------------------------	//

// This Monobehaviour is part of the Move Agent generator
public class MoveAgent : MonoBehaviour {

	//public simpleMaze Shuffler;
	public List<GameObject> LMoveAgent= new List<GameObject>();


	[TextArea(6,1)]
	public string Description;
	[HeaderAttribute("Module")]
	[Tooltip("Array of Modules you want to generate as a Move Agent Grid.")]public GameObject MoveAgentModule;
	[HeaderAttribute("Agent Settings")]
	[Tooltip("The name of the generated Move Agent grid in scene.")]public string AgentName;
	public enum enMapping{PEARLIN_NOISE, TEXTURE};
	[Tooltip("Maptype of the Move Agent\n\nPEARLIN_NOISE: Use a pearlin noise map to modify 2D arrays of assets in realtime.\nTEXTURE: Use a textures greyscale values to modify 2D arrays of assets in realtime. \n\n IMPORTANT: TEXTURE mode only works with read/write enabled textures. Change textures read/write mode in advaced textur import settings.")]public enMapping MapType;
	[Tooltip("The position of the generated Move Agent grid in scene.")]	public Vector3 AgentPosition;
	[ReadOnlyAttribute]public int IndexNumber;
	[Tooltip("The width of the generated Move Agent grid in scene.")][Range(1f,128.0f)]public int AgentWidth;
	[Tooltip("The height of the generated Move Agent grid in scene.")][Range(1f,128.0f)]public int AgentHeight;
	[HeaderAttribute("Move Settings")]
	[Tooltip("Use a Texture for position transformation.")]public bool MovePos = false;
	[Tooltip("The modification value of the position.")]public float PositionFactor = 10f;
	[Tooltip("Use a Texture for scale transformation.")]public bool MoveScale = true;
	[Tooltip("The modification value of the scale.")]public float ScaleFactor = 10f;
	[Tooltip("Use a Texture for rotation transformation.")]public bool MoveRot = false;
	[Tooltip("The modification value of the rotation.")]public float RotationFactor = 10f;
	[Tooltip("The x-tiling of the texture map.")]public float TextureTilingX = 1f;
	[Tooltip("The y-tiling of the texture map.")]public float TextureTilingY = 1f;
	[Tooltip("Inverses the selected texture.")]public bool InversTexture = false;
	[Tooltip("Clamps GameObjecs below value.")][Range(-0.01f,1.0f)]public float Clamp = 0f;
	[Tooltip("Inverses the clamping.")]public bool InversClamp = false;
	[Tooltip("Clamp Alpha.")]public bool ClampAlpha = false;
	[Tooltip("The resolution of the pearlin noise map.")]public float NoiseResolution = 10f;
	[Tooltip("The x-position of the pearlin noise map.")]public float NoisePosX;
	[Tooltip("The y-position of the pearlin noise map.")]public float NoisePosY;
	[Tooltip("The texture used to modify the generated Move Agent grid.\n\nIMPORTANT: Texture mode only works with read/write enabled textures. Change to read/write mode in advaced texture import settings. ")]public Texture2D MapTexture;
	public enum enPivotpoint{CENTER, TOP, BOTTOM, LEFT, RIGHT, FREE};
	[HeaderAttribute("Pivot Settings")]
	[Tooltip("Position of the Pivot Point. Pivot Point can be altered if set to FREE")]public enPivotpoint Pivot;
	[Tooltip("Flag this true to autorefresh the Move Agent grid.\n\nWARNING: Only use this with low texture resolutions and a low agent size.(32 x 32)")]public bool AutoRefresh;
	[SerializeField]
	[Tooltip("Current Pivot Point.")]private Vector2 PivotPoint;

	private GameObject GoPiece;
	[SerializeField]
	private GameObject[,] GoMoveAgentPieces;

	private GameObject GoGizmo;

	public void InitMazeValues() {
		Description =  "This is a Move Agent generator. Change settings in the Inspector..\n\nPress Tab in Scene View to generate a Move Agent at mouse position.";
		
	}
	
	public void GenerateMoveAgent(bool sizeChange) {

		if (MoveAgentModule != null) {
			LMoveAgent.Add(new GameObject());
			LMoveAgent[LMoveAgent.Count -1].name = AgentName+ "_" + (LMoveAgent.Count-1)  + " (" + AgentWidth + "x" + AgentHeight + " ) " + Pivot;
			Vector2 V2MapSizeRatio = new Vector2(MapTexture.width/AgentWidth, MapTexture.height/AgentHeight);
			int iMapPositionX;
			int iMapPositionY;

				//print(V2MapSize);
			try {
  				MapTexture.GetPixels32();
			}
 			catch (UnityException e) {
				print(e + "Please set read/write enable in advanced texture import settings.");
			}

			float fPosY;
			float fScale;
			float fRot;

			float fAlpha;
			float fGray;
			for (int i = 0; i < AgentWidth; i++) {
				for (int j = 0; j < AgentHeight; j++) {
					switch( MapType ) {

						//IsPearlin();
						case enMapping.PEARLIN_NOISE: 
							GoPiece = Instantiate(MoveAgentModule, new Vector3( i - PivotPoint.x + 0.5f, Mathf.PerlinNoise(i/NoiseResolution + NoisePosX,j/NoiseResolution + NoisePosY)*PositionFactor, j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 0, 0)) as GameObject;
							//GoMoveAgentPieces[i,j].transform.position = new Vector3( i - PivotPoint.x + 0.5f, Mathf.PerlinNoise(i/NoiseResolution + NoisePosX,j/NoiseResolution + NoisePosY)*MoveAmount, j - PivotPoint.y + 0.5f);
						break; 

						//IsTexture();
						case enMapping.TEXTURE: 


    						if(MapTexture.GetPixels32().IsReadOnly) {
								print("Please set read/write enable in advanced texture import settings.");
							}
							else {
								iMapPositionX = (int)(i*TextureTilingX*V2MapSizeRatio.x-1);
								iMapPositionY = (int)(j*TextureTilingY*V2MapSizeRatio.y-1);

								fGray = MapTexture.GetPixel(iMapPositionX, iMapPositionY).grayscale;
								fAlpha = MapTexture.GetPixel(iMapPositionX, iMapPositionY).a;

								if (InversTexture == false ) {
									fPosY = MapTexture.GetPixel(iMapPositionX, iMapPositionY).grayscale*PositionFactor;
									fScale = MapTexture.GetPixel(iMapPositionX, iMapPositionY).grayscale*ScaleFactor;
									fRot = MapTexture.GetPixel(iMapPositionX, iMapPositionY).grayscale*RotationFactor;
								}
								else {
									fPosY = (1 - MapTexture.GetPixel(iMapPositionX, iMapPositionY).grayscale)*PositionFactor;
									fScale = (1 - MapTexture.GetPixel(iMapPositionX, iMapPositionY).grayscale)*ScaleFactor;
									fRot = (1-  MapTexture.GetPixel(iMapPositionX, iMapPositionY).grayscale)*RotationFactor;
								}
								if(MovePos == true )
									GoPiece = Instantiate(MoveAgentModule, new Vector3( i - PivotPoint.x + 0.5f, fPosY, j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 0, 0)) as GameObject;
								else
									GoPiece = Instantiate(MoveAgentModule, new Vector3( i - PivotPoint.x + 0.5f, 0, j - PivotPoint.y + 0.5f),  Quaternion.Euler(0, 0, 0)) as GameObject;
								if(MoveScale == true )
									GoPiece.transform.localScale = Vector3.one*fScale;
								else
									GoPiece.transform.localScale = Vector3.one;
								if(MoveRot == true ) {
									GoPiece.transform.Rotate(fRot,fRot,fRot);
								}
								if ( ClampAlpha == true && fAlpha < 1f) {
									GoPiece.SetActive(false);
								}
	
								if(fGray <= Clamp && InversClamp == false) {
									GoPiece.SetActive(false);
								}
								if(fGray > Clamp && InversClamp == true) {
									GoPiece.SetActive(false);
								}
							}
							break; 
						}
					GoPiece.name = "Piece (" + i + " | " + j + " ) ";
					GoPiece.transform.parent = LMoveAgent[IndexNumber].gameObject.transform;
				}
			}

			LMoveAgent[LMoveAgent.Count -1].transform.position =  new Vector3(AgentPosition.x + PivotPoint.x - AgentWidth*0.5f,AgentPosition.y,AgentPosition.z + PivotPoint.y - AgentHeight*0.5f);
			UpdateIndexNumber();
		}
		else
			print("Please choose a Module");
	}
	// Deleting One Maze
	public void  DeleteMoveAgent() {
		if(IndexNumber >= 1 ) {
			GameObject.DestroyImmediate(LMoveAgent[LMoveAgent.Count-1]);
			LMoveAgent.RemoveAt(LMoveAgent.Count-1);
			UpdateIndexNumber ();
		}
		else 
			print ("Generate at least one maze!");
		
		//GoMaze.Clear();
		UpdateIndexNumber ();
	}
	// Deleting All Mazes
	public void  DeleteAll() {
		if(LMoveAgent.Count > 1 ) {
			
			for (int i = 0; i <= LMoveAgent.Count-1; i++) {
				GameObject.DestroyImmediate(LMoveAgent[i]);
			}
			
		}
		else {
			print ("Generate at least one maze!");
		}
		//print ("Index is = " + IndexNumber);
		LMoveAgent.Clear();
		UpdateIndexNumber ();
	}
	public int UpdateIndexNumber () {
		return IndexNumber = LMoveAgent.Count;
	}
	public Vector3 UpdateMazePosition () {
		return AgentPosition = transform.position;
	}
	
	public void SetPivot(float x, float y) {
		PivotPoint = new Vector2(x, y);
	}
	public Vector2 GetPivot() {
		return PivotPoint;
	}
	
	public void GenerateGizmoGameobject() {
		if(GoGizmo == null) {
			GoGizmo = Instantiate(Resources.Load ("GizmoNull"), new Vector3( GetPivot().x, AgentPosition.y, GetPivot().y),  Quaternion.identity) as GameObject;
			if(GoGizmo.GetComponent<MoveAgentGizmo>() == null) {
				GoGizmo.AddComponent<MoveAgentGizmo>();
				GoGizmo.AddComponent<MoveAgentGizmo>().name = "GIZMO OBJECT";
				GoGizmo.GetComponent<MoveAgentGizmo>().SetGizmoPosition(AgentPosition);
				GoGizmo.GetComponent<MoveAgentGizmo>().SetGizmoSize(AgentWidth, AgentHeight);
			}
		}
	}
	public void DestroyGizmoGameobject() {
		//if(IndexNumber >= 0 ) {
		DestroyImmediate(GoGizmo.gameObject);
		//}
	}

	public void IsPearlin() {

	}
	public void IsTexture() {
		//MoveAmount = 1;
		NoiseResolution = 1;
		NoisePosX = 0;
		NoisePosY = 0;
	}
	public void UpdateGizmo() {
		GoGizmo.GetComponent<MoveAgentGizmo>().SetGizmoPosition(AgentPosition);
		GoGizmo.GetComponent<MoveAgentGizmo>().SetGizmoSize(AgentWidth, AgentHeight);
		GoGizmo.GetComponent<MoveAgentGizmo>().SetPivotPosition(new Vector3(AgentPosition.x + PivotPoint.x - AgentWidth*0.5f,this.transform.position.y,AgentPosition.z + PivotPoint.y - AgentHeight*0.5f));
	}
}
