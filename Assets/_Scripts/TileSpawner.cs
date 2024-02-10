using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TempleRun
{
    public class TileSpawner : MonoBehaviour
    {
        [SerializeField] private int tileStartCount = 10;

        [SerializeField] private int minimumStraightTiles = 3;

        [SerializeField] private int maximumStraightTiles = 15;

        [SerializeField] private GameObject startingTile;

        [SerializeField] private List<GameObject> turnTiles;

        [SerializeField] private List<GameObject> obstacles;

        private Vector3 currentTileLocation = Vector3.zero;
        private Vector3 currentTileDirection = Vector3.forward;
        private GameObject prevTile;

        private List<GameObject> currenctTiles;
        private List<GameObject> currentObstacles;

        // Start is called before the first frame update
        private void Start()
        {
            currenctTiles = new List<GameObject>();
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
            Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);
            
            // Instantiate the new tile at the currentTileLocation instead of using currentTileDirection.
            prevTile = GameObject.Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
            currenctTiles.Add(prevTile);

            // After adding the new tile, update the currentTileLocation for the next tile.
            // This ensures the next tile is placed at the correct position.
            // You might need to adjust this logic based on the orientation and desired direction of your tiles.
            Vector3 tileOffset = Vector3.Scale(prevTile.GetComponent<Renderer>().bounds.size, currentTileDirection);
            currentTileLocation += new Vector3(tileOffset.x * currentTileDirection.x, 0,
                tileOffset.z * currentTileDirection.z);
        }

        private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
        {
            if (list.Count == 0) return null;

            return list[Random.Range(0, list.Count)];
        }
    }
}