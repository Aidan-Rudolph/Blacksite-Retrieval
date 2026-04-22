using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainController : MonoBehaviour {
    public int sub = 1;
    public float chunkSize = 1;
    public Material mat;
    public float updates_per_second = 10;
    public string terrainLayer;
    public float threshold = 0.5f;

    [HideInInspector]
    public float[] voxel;

    private float timer = 0;
    int numX;
    int numY;
    int numZ;
    int sizeX, sizeY, sizeZ;
    int strideZ, strideY;

    private MapGridGenerator mgg;

    public class TerrainChunk {
        public Mesh mesh;
        public MeshCollider collider;
        public MeshRenderer renderer;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uv;
        public GameObject obj;
        public int x_i;
        public int z_i;
        public int y_i;
    }

    public List<TerrainChunk> chunks = new List<TerrainChunk>();
    private HashSet<TerrainChunk> toUpdate = new HashSet<TerrainChunk>();

    void Start() {
        mgg = GetComponent<MapGridGenerator>();
        if (mgg == null || !mgg.run()) {
            mgg = null;
        }
        gameObject.layer = LayerMask.NameToLayer(terrainLayer);
        numX = Mathf.CeilToInt(transform.localScale.x / chunkSize);
        numY = Mathf.CeilToInt(transform.localScale.y / chunkSize);
        numZ = Mathf.CeilToInt(transform.localScale.z / chunkSize);
        sizeX = (int)(sub * transform.localScale.x) + 1;
        sizeY = (int)(sub * transform.localScale.y) + 1;
        sizeZ = (int)(sub * transform.localScale.z) + 1;
        strideZ = sizeX;
        strideY = sizeX * sizeZ;
        transform.localScale = new Vector3(numX*chunkSize, numY*chunkSize, numZ*chunkSize);
        voxel = new float[sizeX*sizeY*sizeZ];
        for (int y = 0; y < numY; y++) {
            for (int z = 0; z < numZ; z++) {
                for (int x = 0; x < numX; x++) {
                    GameObject chunkObj = new GameObject($"Chunk_{x}_{y}_{z}");
                    chunkObj.transform.parent = transform;
                    chunkObj.transform.localPosition = new Vector3(
                        x/(float)numX - 0.5f,
                        y/(float)numY - 0.5f,
                        z/(float)numZ - 0.5f
                    );
                    chunkObj.transform.localScale = new Vector3(
                        chunkSize/transform.localScale.x,
                        chunkSize/transform.localScale.y,
                        chunkSize/transform.localScale.z
                    );

                    var mf = chunkObj.AddComponent<MeshFilter>();
                    var mr = chunkObj.AddComponent<MeshRenderer>();
                    mr.material = mat;
                    var mc = chunkObj.AddComponent<MeshCollider>();
                    chunkObj.layer = LayerMask.NameToLayer(terrainLayer);

                    Mesh m = new Mesh();
                    m.name = $"Chunk_{x}_{y}_{z}";
                    m.MarkDynamic();

                    TerrainChunk chunk = new TerrainChunk {
                        mesh = m,
                        collider = mc,
                        renderer = mr,
                        vertices = new List<Vector3>(),
                        triangles = new List<int>(),
                        uv = new List<Vector2>(),
                        obj = chunkObj,
                        x_i = x,
                        y_i = y,
                        z_i = z
                    };
                    mf.mesh = m;
                    mc.sharedMesh = m;

                    chunks.Add(chunk);
                }
            }
        }
        int[,,] map = mgg.GetMap();
        for (int y = 0; y < sizeY; y++) {
            for (int z = 0; z < sizeZ; z++) {
                for (int x = 0; x < sizeX; x++) {
                    int i = x + z * strideZ + y * strideY;
                    int mx = Mathf.Clamp(x-5, 0, map.GetLength(0) - 1);
                    int my = Mathf.Clamp(y-5, 0, map.GetLength(1) - 1);
                    int mz = Mathf.Clamp(z-5, 0, map.GetLength(2) - 1);
                    voxel[i] = (map[mx, my, mz] == 1 && y/sub < transform.localScale.y-transform.position.y) ? 10 : -10;
                }
            }
        }
        foreach (TerrainChunk chunk in chunks) {
            UpdateChunkData(chunk);
        }
    }

    float getVoxel(int x, int y, int z) {
        if (x < 0 || x >= sizeX || y < 0 || y >= sizeY || z < 0 || z >= sizeZ) {
            return -100;
        }
        int curr = x + (z*strideZ) + (y*strideY);
        return voxel[curr];
    }

    float getVoxel(TerrainChunk chunk, int x, int y, int z) {
        x = (int)(chunk.x_i*sub*chunkSize) + x;
        y = (int)(chunk.y_i*sub*chunkSize) + y;
        z = (int)(chunk.z_i*sub*chunkSize) + z;
        return getVoxel(x, y, z);
    }

    Vector3 indToObject(int x, int y, int z) {
        return new Vector3(x/(sub*chunkSize), y/(sub*chunkSize), z/(sub*chunkSize));
    }

    Vector3 Interpolate(Vector3 pointA, Vector3 pointB, float valueA, float valueB, float threshold) {
        float denom = valueB - valueA;
        if (denom == 0) return pointA;
        float t = (threshold - valueA) / (denom);
        return pointA + t * (pointB - pointA);
    }

    void MarchCube(TerrainChunk chunk, int x, int y, int z, List<Vector3> tris) {
        Vector3[] points = {
            indToObject(x + 0, y + 0, z + 0),
            indToObject(x + 1, y + 0, z + 0),
            indToObject(x + 1, y + 0, z + 1),
            indToObject(x + 0, y + 0, z + 1),
            indToObject(x + 0, y + 1, z + 0),
            indToObject(x + 1, y + 1, z + 0),
            indToObject(x + 1, y + 1, z + 1),
            indToObject(x + 0, y + 1, z + 1)
        };
        float[] values = {
            getVoxel(chunk, x + 0, y + 0, z + 0),
            getVoxel(chunk, x + 1, y + 0, z + 0),
            getVoxel(chunk, x + 1, y + 0, z + 1),
            getVoxel(chunk, x + 0, y + 0, z + 1),
            getVoxel(chunk, x + 0, y + 1, z + 0),
            getVoxel(chunk, x + 1, y + 1, z + 0),
            getVoxel(chunk, x + 1, y + 1, z + 1),
            getVoxel(chunk, x + 0, y + 1, z + 1)
        };

        byte cubeIndex = 0;
        cubeIndex |= values[0] < threshold ? (byte)0x01 : (byte)0x00;
        cubeIndex |= values[1] < threshold ? (byte)0x02 : (byte)0x00;
        cubeIndex |= values[2] < threshold ? (byte)0x04 : (byte)0x00;
        cubeIndex |= values[3] < threshold ? (byte)0x08 : (byte)0x00;
        cubeIndex |= values[4] < threshold ? (byte)0x10 : (byte)0x00;
        cubeIndex |= values[5] < threshold ? (byte)0x20 : (byte)0x00;
        cubeIndex |= values[6] < threshold ? (byte)0x40 : (byte)0x00;
        cubeIndex |= values[7] < threshold ? (byte)0x80 : (byte)0x00;

        if (cubeIndex == 0 || cubeIndex == 0xFF) return;
        Vector3[] vertList = new Vector3[12];
        if ((Tables.edgeTable[cubeIndex] & 1) != 0) { vertList[0] = Interpolate(points[0], points[1], values[0], values[1], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 2) != 0) { vertList[1] = Interpolate(points[1], points[2], values[1], values[2], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 4) != 0) { vertList[2] = Interpolate(points[2], points[3], values[2], values[3], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 8) != 0) { vertList[3] = Interpolate(points[3], points[0], values[3], values[0], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 16) != 0) { vertList[4] = Interpolate(points[4], points[5], values[4], values[5], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 32) != 0) { vertList[5] = Interpolate(points[5], points[6], values[5], values[6], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 64) != 0) { vertList[6] = Interpolate(points[6], points[7], values[6], values[7], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 128) != 0) { vertList[7] = Interpolate(points[7], points[4], values[7], values[4], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 256) != 0) { vertList[8] = Interpolate(points[0], points[4], values[0], values[4], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 512) != 0) { vertList[9] = Interpolate(points[1], points[5], values[1], values[5], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 1024) != 0) { vertList[10] = Interpolate(points[2], points[6], values[2], values[6], threshold); }
        if ((Tables.edgeTable[cubeIndex] & 2048) != 0) { vertList[11] = Interpolate(points[3], points[7], values[3], values[7], threshold); }

        for (int i = 0; Tables.triangleTable[cubeIndex, i] != -1; i += 3) {
            Vector3 a = vertList[Tables.triangleTable[cubeIndex, i + 2]];
            Vector3 b = vertList[Tables.triangleTable[cubeIndex, i + 1]];
            Vector3 c = vertList[Tables.triangleTable[cubeIndex, i + 0]];
            if (a == b || b == c || a == c) continue;
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
        }
    }

    void UpdateChunkData(TerrainChunk chunk) {
        List<Vector3> tris = new List<Vector3>();
        for (int y = (chunk.y_i==0 ? -1 : 0); y < sub*chunkSize + (chunk.y_i==numY-1 ? 1 : 0); ++y) {
            for (int z = (chunk.z_i==0 ? -1 : 0); z < sub*chunkSize + (chunk.z_i==numZ-1 ? 1 : 0); ++z) {
                for (int x = (chunk.x_i==0 ? -1 : 0); x < sub*chunkSize + (chunk.x_i==numX-1 ? 1 : 0); ++x) {
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
        UpdateChunk(chunk);
    }

    bool ThreeDistinct(List<Vector3> verts) {
        HashSet<Vector3> verts_set = new HashSet<Vector3>();
        foreach (Vector3 v in verts) {
            verts_set.Add(v);
        }
        return verts_set.Count >= 3;
    }

    void UpdateChunk(TerrainChunk chunk) {
        if (!ThreeDistinct(chunk.vertices)) {
            chunk.renderer.enabled = false;
            chunk.collider.enabled = false;
            return;
        }

        chunk.renderer.enabled = true;
        chunk.collider.enabled = true;

        chunk.mesh.Clear();
        chunk.mesh.vertices = chunk.vertices.ToArray();
        chunk.mesh.triangles = chunk.triangles.ToArray();
        chunk.mesh.uv = chunk.uv.ToArray();
        chunk.mesh.RecalculateNormals();
        chunk.mesh.RecalculateBounds();

        chunk.collider.sharedMesh = null;
        chunk.collider.sharedMesh = chunk.mesh;
    }

    TerrainChunk getChunk(int x, int y, int z) {
        x = Mathf.Min(Mathf.Max(x, 0), numX-1);
        y = Mathf.Min(Mathf.Max(y, 0), numY-1);
        z = Mathf.Min(Mathf.Max(z, 0), numZ-1);
        return chunks[x + z * numX + y * numX * numZ];
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer*updates_per_second >= 1) {
            timer = 0;
            foreach (TerrainChunk chunk in toUpdate) {
                UpdateChunkData(chunk);
            }
            toUpdate.Clear();
        }
    }

    public void Dig(Vector3 center, float strength = 1f, float radius = 1f) {
        int maxX = sizeX - 1;
        int maxY = sizeY - 1;
        int maxZ = sizeZ - 1;

        float radiusSqr = radius * radius;

        Vector3 origin = transform.TransformPoint(new Vector3(-0.5f, -0.5f, -0.5f));

        Vector3 stepX = transform.right * (transform.localScale.x / maxX);
        Vector3 stepY = transform.up * (transform.localScale.y / maxY);
        Vector3 stepZ = transform.forward * (transform.localScale.z / maxZ);

        Vector3 localCenter = transform.InverseTransformPoint(center) + new Vector3(0.5f, 0.5f, 0.5f);

        int minX = Mathf.Max(0, Mathf.FloorToInt((localCenter.x - radius / transform.localScale.x) * maxX));
        int maxXb = Mathf.Min(maxX, Mathf.CeilToInt((localCenter.x + radius / transform.localScale.x) * maxX));

        int minY = Mathf.Max(0, Mathf.FloorToInt((localCenter.y - radius / transform.localScale.y) * maxY));
        int maxYb = Mathf.Min(maxY, Mathf.CeilToInt((localCenter.y + radius / transform.localScale.y) * maxY));

        int minZ = Mathf.Max(0, Mathf.FloorToInt((localCenter.z - radius / transform.localScale.z) * maxZ));
        int maxZb = Mathf.Min(maxZ, Mathf.CeilToInt((localCenter.z + radius / transform.localScale.z) * maxZ));

        for (int y = minY; y <= maxYb; y++) {
            for (int z = minZ; z <= maxZb; z++) {
                for (int x = minX; x <= maxXb; x++) {
                    Vector3 worldPos = origin + stepX * x + stepY * y + stepZ * z;

                    Vector3 d = worldPos - center;
                    float distSqr = d.sqrMagnitude;

                    if (distSqr > radiusSqr) continue;

                    float falloff = 1f - (distSqr / radiusSqr);
                    falloff = falloff * falloff * strength;

                    int i = x + z * strideZ + y * strideY;
                    voxel[i] = Mathf.Clamp(falloff+voxel[i], -10, 10);

                    int chunkX = x / (int)(sub * chunkSize);
                    int chunkY = y / (int)(sub * chunkSize);
                    int chunkZ = z / (int)(sub * chunkSize);

                    for (int oy = -1; oy <= 1; oy++)
                    for (int oz = -1; oz <= 1; oz++)
                    for (int ox = -1; ox <= 1; ox++) {
                        toUpdate.Add(getChunk(chunkX+ox, chunkY+oy, chunkZ+oz));
                    }
                }
            }
        }
    }

    void calc_uv(TerrainChunk chunk, int start = 0, int end = -1) {
        if (end == -1) end = chunk.vertices.Count;
        for (int i = start; i < end; ++i) {
            Vector3 curr = chunk.obj.transform.TransformPoint(chunk.vertices[i]) - transform.position;
            if (chunk.uv.Count == i) chunk.uv.Add(new Vector2(0,0));
            else if (chunk.uv.Count < i) {
                Debug.LogError("Calc_uv used invalid range... uv list too small for start value.");
                return;
            }
            chunk.uv[i] = new Vector2(((curr.x/transform.localScale.x + curr.z/transform.localScale.z)/2) + 0.5f, Mathf.Clamp(curr.y/transform.localScale.y + 0.5f, 0, 1));
        }
    }
}
