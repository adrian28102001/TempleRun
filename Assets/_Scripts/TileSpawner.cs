using System;
using System.Collections.Generic;
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
        
        private float gapProbability = 0.3f; // 20% chance to spawn a gap
        private float gapSize = 3f; // The size of the gap
        
        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject prevTile;

        private List<GameObject> currentTiles;
        private List<GameObject> currentObstacles;
        private int tilesSinceLastCoin = 0;
        private int totalTilesSpawned = 0;

        private int minTilesBetweenObstacles = 1;
        private GameObject lastObstacle = null;
        private int tilesSinceLastObstacle = 0;
        private bool lastTileWasGap = false;
        private bool lastTileWasObstacle = false;
        
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
            tilesSinceLastObstacle++;
            totalTilesSpawned++;

            if (spawnObstacle)
            {
                SpawnObstacle();
            }
            else
            {
                tilesSinceLastCoin++;

                if (tilesSinceLastCoin >= 4)
                {
                    SpawnCoins(currentTileLocation + Vector3.up * 0.5f);
                    tilesSinceLastCoin = 0;
                }
            }

            if (tile.type == TileType.STRAIGHT)
            {
                Vector3 tileScale = Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
                currentTileLocation += tileScale;

                // Check if we should spawn a gap, but not if the last tile was an obstacle
                if (totalTilesSpawned > tileStartCount && Random.value < gapProbability && !lastTileWasObstacle)
                {
                    // Add a gap by moving the spawn location forward
                    currentTileLocation += currentTileDirection * gapSize;
                    lastTileWasGap = true;
                    lastTileWasObstacle = false; // Reset obstacle flag as we just spawned a gap
                }
                else
                {
                    lastTileWasGap = false; // Reset gap flag as we spawned a regular tile
                }
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
            
            // Reset flags when adding a new direction
            lastTileWasGap = false;
            lastTileWasObstacle = false;
        }

        private void SpawnObstacle()
        {
            // Ensure minimum spacing since the last obstacle and check gap flag
            if (tilesSinceLastObstacle < minTilesBetweenObstacles || lastTileWasGap)
            {
                return;
            }

            // Spawn obstacle with some probability
            if (Random.value > 0.4f) return;

            GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
            Quaternion newObjectRotation = obstaclePrefab.gameObject.transform.rotation *
                                           Quaternion.LookRotation(currentTileDirection, Vector3.up);

            // Spawn the obstacle
            GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObjectRotation);
            currentObstacles.Add(obstacle);

            tilesSinceLastObstacle = 0;
            lastTileWasObstacle = true;
            lastTileWasGap = false;
        }

        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;

            return list[Random.Range(0, list.Count)];
        }
    }
}