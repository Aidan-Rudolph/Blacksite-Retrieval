using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEntityGenerator : MonoBehaviour {
    public GameObject[] entities;
    public Vector3 maxSpawnBox = new Vector3(5, 5, 5);

    public void SpawnEntity(Vector3 position) {
        if (entities == null || entities.Length == 0) return;
        Instantiate(entities[UnityEngine.Random.Range(0, entities.Length)], position, Quaternion.identity);
    }

    public bool CheckSpawn(Vector3Int position, Vector3Int size, int[,,] map) {
        int strideZ = size.x;
        int strideY = size.x*size.z;
    }
}
