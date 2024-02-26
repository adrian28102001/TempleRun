using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TempleRun
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int tileStartCount = 6;

        [SerializeField] private int minimumStraightTiles = 3;

        [SerializeField] private int maximumStraightTiles = 9;

        [SerializeField] private GameObject startingTile;

        [SerializeField] private List<GameObject> turnTiles;

        [SerializeField] private List<GameObject> obstacles;

        [SerializeField] private GameObject coinPrefab;

        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject prevTile;

        private List<GameObject> currentTiles;
        private List<GameObject> currentObstacles;
        private int tilesSinceLastCoin = 0;
        private CinemachineVirtualCamera cinemachineVirtualCamera;

        private void Awake()
        {
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }
        
        private void Start()
        {
            currentTiles = new List<GameObject>();
            currentObstacles = new List<GameObject>();

            Random.InitState(DateTime.Now.Millisecond);

            for (int i = 0; i < tileStartCount; ++i)
            {
                SpawnTile(startingTile.GetComponent<Tile>());
            }
            
            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
        }
        
        private void SpawnTile(Tile tile, bool spawnObstacle = false)
        {
            Quaternion newTileRotation = tile.gameObject.transform.rotation *
                                         Quaternion.LookRotation(currentTileDirection, Vector3.up);

            prevTile = Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
            currentTiles.Add(prevTile);

            if (spawnObstacle)
            {
                SpawnObstacle();
            }
            else
            {
                tilesSinceLastCoin++;

                // Check if the required number of tiles has been spawned since the last coin
                if (tilesSinceLastCoin >= 4) 
                {
                    SpawnCoins(currentTileLocation + Vector3.up * 0.5f);
                    tilesSinceLastCoin = 0;
                }    
            }

            if (tile.type == TileType.STRAIGHT)
            {
                currentTileLocation +=
                    Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
            }
        }
        
        private void SpawnCoins(Vector3 tilePosition)
        {
            Vector3 spawnPosition = new Vector3(
                tilePosition.x, // Center the coin on the tile
                tilePosition.y + 3f, // Adjust the height as needed
                tilePosition.z
            );

            GameObject coin = Instantiate(coinPrefab, spawnPosition, Quaternion.identity, transform);

            Quaternion coinRotation = Quaternion.LookRotation(currentTileDirection, Vector3.up);
            coin.transform.rotation = coinRotation;

            // Adjust if necessary to make sure the coin is facing up
            coin.transform.Rotate(90, 0, 0);
        }
        
        private void DeletePreviousTiles()
        {
            while (currentTiles.Count != 1)
            {
                GameObject tile = currentTiles[0];
                currentTiles.RemoveAt(0);
                Destroy(tile);
            }
            
            while (currentObstacles.Count != 0)
            {
                GameObject obstacle = currentObstacles[0];
                currentObstacles.RemoveAt(0);
                Destroy(obstacle);
            }
        }
        
        public void AddNewDirection(Vector3 direction)
        {
            currentTileDirection = direction;
            DeletePreviousTiles();

            Vector3 tilePlacementScale;
            if (prevTile.GetComponent<Tile>().type == TileType.SIDEWAYS)
            {
                tilePlacementScale =
                    Vector3.Scale(
                        prevTile.GetComponent<Renderer>().bounds.size / 2 +
                        Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2, currentTileDirection);
            }
            else
            {
                //Left or right
                tilePlacementScale =
                    Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size - (Vector3.one * 2) +
                                  Vector3.one * startingTile.GetComponent<BoxCollider>().size.z / 2,
                        currentTileDirection);
            }

            currentTileLocation += tilePlacementScale;

            int currentPathLength = Random.Range(minimumStraightTiles, maximumStraightTiles);

            for (int i = 0; i < currentPathLength; ++i)
            {
                SpawnTile(startingTile.GetComponent<Tile>(), spawnObstacle: i != 0);
            }

            SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>(), spawnObstacle: false);
        }
        
        private void SpawnObstacle()
        {
            if (Random.value > 0.4f) return;

            GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
            Quaternion newObjectRotation = obstaclePrefab.gameObject.transform.rotation *
                                           Quaternion.LookRotation(currentTileDirection, Vector3.up);

            GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObjectRotation);
            currentObstacles.Add(obstacle);
        }
        
        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;

            return list[Random.Range(0, list.Count)];
        }
    }
}