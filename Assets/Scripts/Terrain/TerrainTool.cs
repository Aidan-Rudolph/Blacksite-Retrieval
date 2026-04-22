using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTool : MonoBehaviour {
    public TerrainController terrain;
    public float strength = 0.25f;
    public float radius = 2;

    public void Use(Ray ray, float dir, float max_dist) {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit)) {
            TerrainController t_hit = hit.collider.transform.parent.GetComponent<TerrainController>();
            if (t_hit == null || t_hit != terrain || Vector3.Distance(hit.point, ray.origin) > max_dist) return;
            terrain.Dig(hit.point, strength * dir, radius);
        }
    }
}
