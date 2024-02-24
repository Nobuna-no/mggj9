using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NobunAtelier;

public class GroundGenerator : Singleton<GroundGenerator>
{
    public Camera mainCamera;
    public Transform startPoint; //Point from where ground tiles will start
    public PoolObjectDefinition tilePrefab;
    public float movingSpeed = 12;
    public int tilesToPreSpawn = 15; //How many tiles should be pre-spawned
    public int tilesWithoutObstacles = 3; //How many tiles at the beginning should not have obstacles, good for warm-up

    List<RunnerTile> spawnedTiles = new List<RunnerTile>();
    int nextTileToActivate = -1;
    [HideInInspector]
    public bool gameOver = false;
    static bool gameStarted = false;
    float score = 0;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 spawnPosition = startPoint.position;
        int tilesWithNoObstaclesTmp = tilesWithoutObstacles;

        spawnedTiles = new List<RunnerTile>(PoolManager.Instance.SpawnObjects<RunnerTile>(tilePrefab, Vector3.zero, 0, tilesToPreSpawn));
        spawnPosition -= spawnedTiles[0].StartPoint.localPosition;

        foreach(var tile in spawnedTiles)
        {
            // SC_PlatformTile spawnedTile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity) as SC_PlatformTile;

            if (tilesWithNoObstaclesTmp > 0)
            {
                tile.DeactivateObstacles();
                tilesWithNoObstaclesTmp--;
            }
            else
            {
                tile.ActivateRandomObstacle();
            }

            spawnPosition = tile.EndPoint.position;
            tile.transform.SetParent(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Move the object upward in world space x unit/second.
        //Increase speed the higher score we get
        if (!gameOver && gameStarted)
        {
            transform.Translate(-spawnedTiles[0].transform.forward * Time.deltaTime * (movingSpeed + (score / 500)), Space.World);
            score += Time.deltaTime * movingSpeed;
        }

        if (mainCamera.WorldToViewportPoint(spawnedTiles[0].EndPoint.position).z < 0)
        {
            //Move the tile to the front if it's behind the Camera
            var tileTmp = spawnedTiles[0];
            tileTmp.DeactivateObstacles();
            spawnedTiles.RemoveAt(0);
            tileTmp.transform.position = spawnedTiles[spawnedTiles.Count - 1].EndPoint.position - tileTmp.StartPoint.localPosition;
            tileTmp.ActivateRandomObstacle();
            spawnedTiles.Add(tileTmp);
        }

        if (gameOver || !gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (gameOver)
                {
                    //Restart current scene
                    Scene scene = SceneManager.GetActiveScene();
                    SceneManager.LoadScene(scene.name);
                }
                else
                {
                    //Start the game
                    gameStarted = true;
                }
            }
        }
    }

    void OnGUI()
    {
        if (gameOver)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200), "Game Over\nYour score is: " + ((int)score) + "\nPress 'Space' to restart");
        }
        else
        {
            if (!gameStarted)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 200), "Press 'Space' to start");
            }
        }


        GUI.color = Color.green;
        GUI.Label(new Rect(5, 5, 200, 25), "Score: " + ((int)score));
    }
}