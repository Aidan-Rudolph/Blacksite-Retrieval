using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridGenerator : MonoBehaviour
{
    public GameObject testTerrain;
    public bool testing = false;

    [Header("Grid Dimensions")]
    public int sizeX = 100;
    public int sizeY = 50;
    public int sizeZ = 100;

    [Header("Walker Settings")]
    public int maxSteps = 2000;     // How long the walker explores
    public int carveRadius = 2;     // How thick the tunnels are
    public int carveSpeed = 3;      // How many movement steps before carving occurs

    // Internal storage for the map
    private int[,,] map = null; // 1 = solid, 0 = empty

    // Start is called before the first frame update
    public int[,,] GetMap() {
        return map;
    }

    void Start()
    {
        if (!testing || !init()) return;
        fillGridSolid();
        randomWalker();
        testSpawn();
    }

    bool init() {
        if (map != null) return false;
        map = new int[sizeX, sizeY, sizeZ];
        return true;
    }

    public bool run() {
        if (init()) {
            fillGridSolid();
            randomWalker();
            if (testing) testSpawn();
            return true;
        }
        return false;
    }

    void fillGridSolid()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    map[x, y, z] = 1; // Mark as solid
                }
            }
        }
    }

    void randomWalker()
    {
        Vector3Int pos = new Vector3Int(sizeX / 2, sizeY / 2, sizeZ / 2); // Start in the center

        for (int step = 0; step < maxSteps; step++)
        {
            // Carve if it's time to carve
            if ((step % carveSpeed) == 0 || step == (maxSteps - 1))
            {
                carveSphere(pos, carveRadius);
            }

            // Move in a random direction if valid
            Vector3Int dir = RandomDirection3D();
            Vector3Int newPos = pos + dir;

            if (isInBounds(newPos))
            {
                pos = newPos; // Move to the new position
            }

        }
    }

    Vector3Int RandomDirection3D()
    {
        int dir = UnityEngine.Random.Range(0, 6);
        switch (dir)
        {
            case 0: return Vector3Int.right;   // +X
            case 1: return Vector3Int.left;    // -X
            case 2: return Vector3Int.up;      // +Y
            case 3: return Vector3Int.down;    // -Y
            case 4: return Vector3Int.forward; // +Z
            case 5: return Vector3Int.back;    // -Z
            default: return Vector3Int.zero;
        }
    }

    bool isInBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < sizeX &&
               pos.y >= 0 && pos.y < sizeY &&
               pos.z >= 0 && pos.z < sizeZ;
    }

    void carveSphere(Vector3Int center, int radius)
    {
        int rSquared = radius * radius;

        int startX = center.x - radius;
        int endX = center.x + radius;

        int startY = center.y - radius;
        int endY = center.y + radius;

        int startZ = center.z - radius;
        int endZ = center.z + radius;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                for (int z = startZ; z <= endZ; z++)
                {
                    // Skip if outside grid
                    if (x < 0 || x >= sizeX ||
                        y < 0 || y >= sizeY ||
                        z < 0 || z >= sizeZ)
                        continue;

                    int dx = x - center.x;
                    int dy = y - center.y;
                    int dz = z - center.z;

                    // Sphere check without square root
                    if (dx * dx + dy * dy + dz * dz <= rSquared)
                    {
                        map[x, y, z] = 0;
                    }
                }
            }
        }
    }

    void testSpawn()
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    if (map[x, y, z] == 0)
                    {
                        Instantiate(testTerrain, new Vector3(-x, y, -z) + transform.localScale*0.5f + transform.position, Quaternion.identity);
                    }
                }
            }
        }
    }
}
