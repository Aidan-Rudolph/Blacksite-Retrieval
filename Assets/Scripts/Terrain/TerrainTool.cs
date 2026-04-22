using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTool : MonoBehaviour {
    public TerrainController terrain;
    public float strength = 0.25f;
    public float radius = 2;

    public void Use(Ray ray, float dir) {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            TerrainController.TerrainChunk chunk = terrain.chunks.Find(c => c.obj == hit.collider.gameObject);
            if (chunk == null) return;
            foreach (Collider col in Physics.OverlapSphere(hit.point, radius+terrain.chunkSize, LayerMask.GetMask(terrain.terrainLayer))) {
                TerrainController.TerrainChunk c = terrain.chunks.Find(c => c.obj == col.gameObject);
                if (c != null)
                    terrain.Dig(hit.point, strength * dir, radius);
            }
        }
    }
}
