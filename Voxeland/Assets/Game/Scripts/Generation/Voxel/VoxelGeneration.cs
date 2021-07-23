using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

struct Pair
{
    public float distance;
    public Vector3 pos;

    public Pair(float distance, Vector3 pos)
    {
        this.distance = distance;
        this.pos = pos;
    }
}
public class VoxelGeneration : MonoBehaviour
{
    internal delegate short GenerationActionDelegate(int x, int y, int z, byte lod);
    internal float updateDistance { get => 1 << master.RenderDistance - 2; }
    internal static bool UpdateNow { get; set; }
    internal static ObjectPool<GameObject> GameObjectPool = new ObjectPool<GameObject>(128);

    [SerializeField] VoxelMaster master;
    internal static GenerationActionDelegate GenerationAction = null;
    Camera mainCamera;
    Vector3 renderPos;
    [SerializeField] int targetFPS = 100;
    [SerializeField] int chunksPerFrame = 4;
    int meshesLastFrame = 0;


    void Start()
    {
        TryAssign();
        StartCoroutine(UpdateGeneration());
    }
    void Update()
    {
        if (mainCamera != null)
            if (Vector3.Distance(renderPos, mainCamera.transform.position) >= updateDistance)
                renderPos = mainCamera.transform.position;
    }

    void TryAssign()
    {
        if (GenerationAction is null)
            GenerationAction = GetComponent<BaseGeneration>().Generation;

        if (mainCamera is null)
            mainCamera = GameManager.Instance.m_MainCamera;
    }

    IEnumerator UpdateGeneration()
    {
        Vector3 lastPos = renderPos + Vector3.one * updateDistance;
        Vector3[] chunkGrid = null;

        while (true)
        {
            if (lastPos != renderPos || UpdateNow)
            {
                meshesLastFrame = 0;

                lastPos = renderPos;
                UpdateNow = false;

                for (byte lod = 0; lod <= master.LevelOfDetailsCount; lod++)
                {
                    if (Application.platform == RuntimePlatform.WebGLPlayer)
                        chunkGrid = GetChunkGrid(lastPos, lod);
                    else
                    {
                        Thread t = new Thread(new ThreadStart(() => chunkGrid = GetChunkGrid(lastPos, lod)))
                        { Priority = System.Threading.ThreadPriority.Normal };
                        t.IsBackground = true;
                        t.Start();
                    }

                    yield return new WaitWhile(() => chunkGrid is null);

                    foreach (Vector3 chunkPos in chunkGrid)
                    {
                        yield return new WaitForEndOfFrame();

                        if (lastPos != renderPos) break;
                        if (master.ChunkExists(chunkPos, lod)) continue;

                        GenerateChunk(chunkPos, lod);
                        meshesLastFrame++;
                    }

                    yield return null;
                }
            }

            yield return null;
        }
    }
    void GenerateChunk(Vector3 _p, byte _l)
    {
        Voxel[,,] nodes = new Voxel[Chunk.SIZE, Chunk.SIZE, Chunk.SIZE];
        bool isEmpty = true;
        bool isFilled = true;
        Chunk chunk = master.CreateChunk(_p, _l);
        int ChunkSize = Chunk.SIZE;

        for (int x = -1; x <= ChunkSize; x++)
            for (int y = -1; y <= ChunkSize; y++)
                for (int z = -1; z <= ChunkSize; z++)
                {
                    var id = GenerationAction(
                        x * (1 << _l) + chunk.Pos.x,
                        y * (1 << _l) + chunk.Pos.y,
                        z * (1 << _l) + chunk.Pos.z,
                        _l);

                    if (id != -1)
                    {
                        if ((x & Chunk.MASK) != x ||
                            (y & Chunk.MASK) != y ||
                            (z & Chunk.MASK) != z)
                            continue;

                        isEmpty = false;
                        nodes[x, y, z] = new Voxel(chunk, (byte)id);
                    }
                    else
                        isFilled = false;
                }

        if (_l == 0)
            isFilled = false;

        if (!isEmpty && !isFilled)
            chunk.SetNodes(nodes);
    }

    Vector3[] GetChunkGrid(Vector3 _p, byte _l)
    {
        List<Pair> grid = new List<Pair>();
        float maxDistance = 1 << master.RenderDistance + _l;
        float minDistance = _l == 0 ? 0 : 1 << master.RenderDistance + (_l - 1);
        int chunkSize = Chunk.SIZE << _l;

        int lowX = ChunkRound(_p.x - maxDistance, chunkSize);
        int lowY = ChunkRound(_p.y - maxDistance, chunkSize);
        int lowZ = ChunkRound(_p.z - maxDistance, chunkSize);

        int highX = ChunkRound(_p.x + maxDistance, chunkSize);
        int highY = ChunkRound(_p.y + maxDistance, chunkSize);
        int highZ = ChunkRound(_p.z + maxDistance, chunkSize);

        for (int x = lowX; x <= highX; x += chunkSize)
            for (int y = lowY; y <= highY; y += chunkSize)
                for (int z = lowZ; z <= highZ; z += chunkSize)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    float distance = Vector3.Distance(pos, _p);

                    if (distance <= maxDistance && distance >= minDistance && !master.ChunkExists(pos, _l))
                        grid.Add(new Pair(distance, pos));
                }

        return grid.OrderBy(o => o.distance).Select(o => o.pos).ToArray();
    }

    static int ChunkRound(float v, int ChunkSize) { return Mathf.FloorToInt(v / ChunkSize) * ChunkSize; }
}
