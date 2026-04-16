using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController1 : MonoBehaviour {
    public int sub = 16;
    public float chunkSize = 4;
    public Material mat;
    public float updates_per_second = 10;
    private float timer = 0;

    class TerrainChunk {
        public Mesh mesh;
        public MeshCollider collider;
        public float[] fills;
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
        int numY = Mathf.CeilToInt(transform.localScale.y);
        int numZ = Mathf.CeilToInt(transform.localScale.z / chunkSize);
        transform.localScale = new Vector3(numX*chunkSize, numY, numZ*chunkSize);

        for (int z = 0; z < numZ; z++) {
            for (int x = 0; x < numX; x++) {
                GameObject chunkObj = new GameObject($"Chunk_{x}_{z}");
                chunkObj.transform.parent = transform;
                chunkObj.transform.localPosition = new Vector3(x/(float)numX - 0.5f, 0.5f, z/(float)numZ - 0.5f);
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
                    fills = new float[(int)((sub*transform.localScale.y*sub*chunkSize*sub*chunkSize))],
                    vertices = new List<Vector3>(),
                    triangles = new List<int>(),
                    uv = new List<Vector2>(),
                    obj = chunkObj,
                    x_i = x,
                    z_i = z
                };

                for (int i = 0; i < chunk.fills.Length; ++i) {
                    chunk.fills[i] = Mathf.Clamp((i/(int)(sub*chunkSize*sub*chunkSize)) - (sub*transform.localScale.y/2), -100, 100); // based on y val
                }
                mf.mesh = m;
                mc.sharedMesh = m;

                chunks.Add(chunk);
            }
        }
        foreach (TerrainChunk chunk in chunks) {
            UpdateChunkData(chunk);
        }
    }

    float getFills(TerrainChunk chunk, int x, int y, int z) {
        int numX = Mathf.CeilToInt(transform.localScale.x / chunkSize);
        int numZ = Mathf.CeilToInt(transform.localScale.z / chunkSize);

        TerrainChunk curr_chunk = chunk;
        if (y < 0 || y >= sub*transform.localScale.y) {
            return -100;
        }
        if (x < 0) {
            if (chunk.x_i == 0) return -100;
            x += (int)(sub*chunkSize);
            curr_chunk = chunks[curr_chunk.x_i-1 + (curr_chunk.z_i*numX)];
        }
        else if (x >= sub*chunkSize) {
            if (chunk.x_i == numX-1) return -100;
            x = 0;
            curr_chunk = chunks[curr_chunk.x_i+1 + (curr_chunk.z_i*numX)];
        }
        if (z < 0) {
            if (chunk.z_i == 0) return -100;
            z += (int)(sub*chunkSize);
            curr_chunk = chunks[curr_chunk.x_i + ((curr_chunk.z_i-1)*numX)];
        }
        else if (z >= sub*chunkSize) {
            if (chunk.z_i == numZ-1) return -100;
            z = 0;
            curr_chunk = chunks[curr_chunk.x_i + ((curr_chunk.z_i+1)*numX)];
        }
        return curr_chunk.fills[(int)(y*(sub*chunkSize*sub*chunkSize) + z*(sub*chunkSize) + x)];
    }

    Vector3 getPoint(TerrainChunk chunk, int x, int y, int z) {
        ++x;++z;
        return new Vector3(x/(chunkSize*sub), -1 * (y/(transform.localScale.y*sub)), z/(chunkSize*sub));
    }

    Vector3 Interpolate(Vector3 pointA, Vector3 pointB, float valueA, float valueB, float threshold) {
        float t = (threshold - valueA) / (valueB - valueA);
        return pointA + t * (pointB - pointA);
    }

    void MarchCube(TerrainChunk chunk, int x, int y, int z, List<Vector3> tris) {
        Vector3[] points = {
            getPoint(chunk, x - 1, y - 1, z - 1),
            getPoint(chunk, x - 0, y - 1, z - 1),
            getPoint(chunk, x - 0, y - 1, z - 0),
            getPoint(chunk, x - 1, y - 1, z - 0),
            getPoint(chunk, x - 1, y - 0, z - 1),
            getPoint(chunk, x - 0, y - 0, z - 1),
            getPoint(chunk, x - 0, y - 0, z - 0),
            getPoint(chunk, x - 1, y - 0, z - 0)
        };
        float[] values = {
            getFills(chunk, x - 1, y - 1, z - 1),
            getFills(chunk, x - 0, y - 1, z - 1),
            getFills(chunk, x - 0, y - 1, z - 0),
            getFills(chunk, x - 1, y - 1, z - 0),
            getFills(chunk, x - 1, y - 0, z - 1),
            getFills(chunk, x - 0, y - 0, z - 1),
            getFills(chunk, x - 0, y - 0, z - 0),
            getFills(chunk, x - 1, y - 0, z - 0)
        };

        byte cubeIndex = 0;
        cubeIndex |= values[0] < 1 ? (byte)0x01 : (byte)0x00;
        cubeIndex |= values[1] < 1 ? (byte)0x02 : (byte)0x00;
        cubeIndex |= values[2] < 1 ? (byte)0x04 : (byte)0x00;
        cubeIndex |= values[3] < 1 ? (byte)0x08 : (byte)0x00;
        cubeIndex |= values[4] < 1 ? (byte)0x10 : (byte)0x00;
        cubeIndex |= values[5] < 1 ? (byte)0x20 : (byte)0x00;
        cubeIndex |= values[6] < 1 ? (byte)0x40 : (byte)0x00;
        cubeIndex |= values[7] < 1 ? (byte)0x80 : (byte)0x00;

        if (cubeIndex == 0 || cubeIndex == 0xFF) return;
        Vector3[] vertList = new Vector3[12];
        if ((Tables.edgeTable[cubeIndex] & 1) != 0) { vertList[0] = Interpolate(points[0], points[1], values[0], values[1], 1); }
        if ((Tables.edgeTable[cubeIndex] & 2) != 0) { vertList[1] = Interpolate(points[1], points[2], values[1], values[2], 1); }
        if ((Tables.edgeTable[cubeIndex] & 4) != 0) { vertList[2] = Interpolate(points[2], points[3], values[2], values[3], 1); }
        if ((Tables.edgeTable[cubeIndex] & 8) != 0) { vertList[3] = Interpolate(points[3], points[0], values[3], values[0], 1); }
        if ((Tables.edgeTable[cubeIndex] & 16) != 0) { vertList[4] = Interpolate(points[4], points[5], values[4], values[5], 1); }
        if ((Tables.edgeTable[cubeIndex] & 32) != 0) { vertList[5] = Interpolate(points[5], points[6], values[5], values[6], 1); }
        if ((Tables.edgeTable[cubeIndex] & 64) != 0) { vertList[6] = Interpolate(points[6], points[7], values[6], values[7], 1); }
        if ((Tables.edgeTable[cubeIndex] & 128) != 0) { vertList[7] = Interpolate(points[7], points[4], values[7], values[4], 1); }
        if ((Tables.edgeTable[cubeIndex] & 256) != 0) { vertList[8] = Interpolate(points[0], points[4], values[0], values[4], 1); }
        if ((Tables.edgeTable[cubeIndex] & 512) != 0) { vertList[9] = Interpolate(points[1], points[5], values[1], values[5], 1); }
        if ((Tables.edgeTable[cubeIndex] & 1024) != 0) { vertList[10] = Interpolate(points[2], points[6], values[2], values[6], 1); }
        if ((Tables.edgeTable[cubeIndex] & 2048) != 0) { vertList[11] = Interpolate(points[3], points[7], values[3], values[7], 1); }

        for (int i = 0; Tables.triangleTable[cubeIndex, i] != -1; i += 3) {
            tris.Add(vertList[Tables.triangleTable[cubeIndex, i + 0]]);
            tris.Add(vertList[Tables.triangleTable[cubeIndex, i + 1]]);
            tris.Add(vertList[Tables.triangleTable[cubeIndex, i + 2]]);
        }
    }

    void UpdateChunkData(TerrainChunk chunk) {
        int zs = (int)(sub*chunkSize);
        int ys = zs*zs;

        // Generate vertices
        List<Vector3> tris = new List<Vector3>();
        for (int y = 0; y <= sub*transform.localScale.y; ++y) {
            for (int z = 0; z <= sub*chunkSize; z++) {
                for (int x = 0; x <= sub*chunkSize; x++) {
                    MarchCube(chunk, x, y, z, tris);
                }
            }
        }
        chunk.vertices = new List<Vector3>(tris.Count);
        chunk.triangles = new List<int>(tris.Count);
        for (int i = 0; i < tris.Count; ++i) {
            chunk.vertices.Add(tris[i]);
            chunk.triangles.Add(i);
        }
        chunk.uv = new List<Vector2>(chunk.vertices.Count);
        calc_uv(chunk);

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
                UpdateChunkData(chunk);
            }
            if (toUpdate.Count > 0)
                Debug.Log("Updated");
            toUpdate.Clear();
        }
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E)) {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)) {
                TerrainChunk chunk = chunks.Find(c => c.obj == hit.collider.gameObject);
                if (chunk != null) {
                    foreach (TerrainChunk c in chunks)
                        Dig(c, hit.point, Input.GetKey(KeyCode.E), 0.1f*Time.deltaTime);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)) {
                TerrainChunk chunk = chunks.Find(c => c.obj == hit.collider.gameObject);
                if (chunk != null) {
                    for (int i = 0; i < chunk.fills.Length; ++i)
                        Debug.Log(chunk.fills[i]);
                }
            }
        }
    }

    void Dig(TerrainChunk chunk, Vector3 center, bool add, float strength = 1f, float radius = 1) {
        bool changed = false;
        strength *= (add ? 1 : -1);
        for (int i = 0; i < chunk.fills.Length; i++) {
            int y = (int)(i/(sub*chunkSize*sub*chunkSize));
            int z = (i%(int)(sub*chunkSize*sub*chunkSize))/(int)(sub*chunkSize);
            int x = i%(int)(sub*chunkSize);
            float dist = Vector3.Distance(getPoint(chunk, x, y, z), center);
            if (dist > radius) continue;
            changed = true;
            float falloff = 1f - (dist / radius);
            falloff = falloff * falloff;
            chunk.fills[i] = Mathf.Clamp(falloff + chunk.fills[i], -100, 100);
        }
        if (changed) {
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
        if (end == -1) end = chunk.vertices.Count;
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
