using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour {
    public int sub = 16;
    public float chunkSize = 4;
    public Material mat;
    public float updates_per_second = 10;
    private float timer = 0;

    class TerrainChunk {
        public Mesh mesh;
        public MeshCollider collider;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uv;
        public GameObject obj;
        public int x_i;
        public int z_i;
    }

    private List<TerrainChunk> chunks = new List<TerrainChunk>();
    private HashSet<TerrainChunk> toUpdate = new HashSet<TerrainChunk>();

    void Start() {
        int numX = Mathf.CeilToInt(transform.localScale.x / chunkSize);
        int numZ = Mathf.CeilToInt(transform.localScale.z / chunkSize);
        transform.localScale = new Vector3(numX*chunkSize, transform.localScale.y, numZ*chunkSize);

        for (int z = 0; z < numZ; z++) {
            for (int x = 0; x < numX; x++) {
                GameObject chunkObj = new GameObject($"Chunk_{x}_{z}"); // transform.position + rel_pos, transform.rotation, new Vector3(chunkSize, 0, chunkSize)
                chunkObj.transform.parent = transform;
                chunkObj.transform.localPosition = new Vector3(x/(float)numX - 0.5f, 0, z/(float)numZ - 0.5f);
                chunkObj.transform.localScale = new Vector3(chunkSize/transform.localScale.x, 1, chunkSize/transform.localScale.z);

                var mf = chunkObj.AddComponent<MeshFilter>();
                var mr = chunkObj.AddComponent<MeshRenderer>();
                mr.material = mat;
                var mc = chunkObj.AddComponent<MeshCollider>();

                Mesh m = new Mesh();
                m.name = $"Chunk_{x}_{z}";
                m.MarkDynamic();

                TerrainChunk chunk = new TerrainChunk {
                    mesh = m,
                    collider = mc,
                    vertices = new List<Vector3>((int)((chunkSize*sub+1)*(chunkSize*sub+1))),
                    triangles = new List<int>((int)((chunkSize*sub)*(chunkSize*sub)*2)),
                    uv = new List<Vector2>((int)((chunkSize*sub+1)*(chunkSize*sub+1))),
                    obj = chunkObj,
                    x_i = x,
                    z_i = z
                };

                GenerateChunkMesh(chunk);
                mf.mesh = m;
                mc.sharedMesh = m;

                chunks.Add(chunk);
            }
        }
    }

    void GenerateChunkMesh(TerrainChunk chunk) {
        float size = chunkSize * sub;

        chunk.vertices.Clear();
        chunk.triangles.Clear();
        chunk.uv.Clear();

        // Generate grid
        for (int z = 0; z <= size; z++) {
            for (int x = 0; x <= size; x++) {
                Vector3 v = new Vector3(x/size, 0, z/size);
                chunk.vertices.Add(v);
            }
        }
        calc_uv(chunk);

        for (int z = 0; z < size; z++) {
            for (int x = 0; x < size; x++) {
                int i = z * ((int)size + 1) + x;
                chunk.triangles.Add(i);
                chunk.triangles.Add(i + (int)size + 1);
                chunk.triangles.Add(i + 1);
                chunk.triangles.Add(i + 1);
                chunk.triangles.Add(i + (int)size + 1);
                chunk.triangles.Add(i + (int)size + 2);
            }
        }

        chunk.mesh.Clear();
        chunk.mesh.vertices = chunk.vertices.ToArray();
        chunk.mesh.triangles = chunk.triangles.ToArray();
        chunk.mesh.uv = chunk.uv.ToArray();
        chunk.mesh.RecalculateNormals();
        chunk.mesh.RecalculateBounds();

        chunk.collider.sharedMesh = null;
        chunk.collider.sharedMesh = chunk.mesh;
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer*updates_per_second >= 1) {
            timer = 0;
            foreach (TerrainChunk chunk in toUpdate) {
                UpdateChunk(chunk);
            }
            toUpdate.Clear();
        }
        if (Input.GetKey(KeyCode.Space)) {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)) {
                TerrainChunk chunk = chunks.Find(c => c.obj == hit.collider.gameObject);
                if (chunk != null) {
                    foreach (TerrainChunk c in chunks)
                        Dig(c, hit.point, Camera.main.transform.position, 10 * Time.deltaTime);
                }
            }
        }
    }

    void Dig(TerrainChunk chunk, Vector3 center, Vector3 offset, float strength = 1f) {
        bool changed = false;
        float radius = strength * 2;
        for (int i = 0; i < chunk.vertices.Count; i++) {
            float dist = Vector3.Distance(chunk.obj.transform.TransformPoint(chunk.vertices[i]), center);
            if (dist > radius) continue;
            changed = true;
            Vector3 apex = center + ((center - offset).normalized * strength);
            Vector3 move = chunk.obj.transform.InverseTransformPoint(apex) - chunk.obj.transform.InverseTransformPoint(center);
            float falloff = 1f - (dist / radius);
            falloff = falloff * falloff;
            chunk.vertices[i] += move * falloff;
        }
        if (changed) {
            calc_uv(chunk);
            toUpdate.Add(chunk);
        }
    }

    void UpdateChunk(TerrainChunk chunk) {
        chunk.mesh.vertices = chunk.vertices.ToArray();
        chunk.mesh.uv = chunk.uv.ToArray();
        chunk.mesh.RecalculateNormals();
        chunk.mesh.RecalculateBounds();

        chunk.collider.sharedMesh = null;
        chunk.collider.sharedMesh = chunk.mesh;
    }

    void calc_uv(TerrainChunk chunk, int start = 0, int end = -1) {
        if (end == -1) end = chunk.vertices.Capacity;
        for (int i = start; i < end; ++i) {
            Vector3 curr = chunk.vertices[i];
            if (chunk.uv.Count == i) chunk.uv.Add(new Vector2(0,0));
            else if (chunk.uv.Count < i) {
                Debug.LogError("Calc_uv used invalid range... uv list too small for start value.");
                return;
            }
            chunk.uv[i] = new Vector2(((curr.x + curr.z)/2) + 0.5f, Mathf.Clamp(curr.y+1, 0, 1));
        }
    }
}
