using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomEntityGenerator : MonoBehaviour {
    public GameObject[] entities;
    public Vector3Int maxSpawnBox = new Vector3Int(5, 5, 5);
    public int spawnChance = 200;
    private Vector3Int spawnBoxCenter;
    private int[,,] map;
    private static RandomEntityGenerator spawner = null;

    void Start() {
        if (spawner != null) {
            Destroy(gameObject);
            return;
        }
        spawner = this;
        if (maxSpawnBox.x%2 == 0) ++maxSpawnBox.x;
        if (maxSpawnBox.z%2 == 0) ++maxSpawnBox.z;
        spawnBoxCenter = new Vector3Int(maxSpawnBox.x/2, 0, maxSpawnBox.z/2);
    }

    public static void SpawnEntity(Vector3 position) {
        if (spawner == null) Debug.LogError("No spawner in scene.");
        else spawner.SpawnEntity_m(position);
    }

    private void SpawnEntity_m(Vector3 position) {
        Debug.Log(position);
        if (entities == null || entities.Length == 0) return;
        Instantiate(entities[UnityEngine.Random.Range(0, entities.Length)], position, Quaternion.identity);
    }

    public static void SetMap(int[,,] map) {
        if (spawner == null) Debug.LogError("No spawner in scene.");
        else spawner.SetMap_m(map);
    }

    private void SetMap_m(int[,,] map) {
        this.map = new int[map.GetLength(0),map.GetLength(1),map.GetLength(2)];
        Array.Copy(map, this.map, map.Length);
    }

    public static bool CheckSpawn(Vector3Int position, Vector3Int size) {
        if (spawner == null) {
            Debug.LogError("No spawner in scene.");
            return false;
        }
        else return spawner.CheckSpawn_m(position, size);
    }

    private bool CheckSpawn_m(Vector3Int position, Vector3Int size) {
        Vector3Int start = position-spawnBoxCenter;
        Vector3Int end = start+maxSpawnBox;
        if (start.x < 0 || start.y < 0 || start.z < 0) return false;
        if (end.x >= size.x || end.y >= size.y || end.z >= size.z) return false;
        for (int y_i = 0; y_i < maxSpawnBox.y; ++y_i) {
            for (int z_i = 0; z_i < maxSpawnBox.z; ++z_i) {
                for (int x_i = 0; x_i < maxSpawnBox.x; ++x_i) {
                    if (map[start.x+x_i, start.y+y_i, start.z+z_i] == 1) return false;
                }
            }
        }
        if (UnityEngine.Random.Range(0, spawnChance) != 0) return false;
        for (int y_i = 0; y_i < maxSpawnBox.y; ++y_i) {
            for (int z_i = 0; z_i < maxSpawnBox.z; ++z_i) {
                for (int x_i = 0; x_i < maxSpawnBox.x; ++x_i) {
                    map[start.x+x_i, start.y+y_i, start.z+z_i] = 1;
                }
            }
        }
        return true;
    }
}
