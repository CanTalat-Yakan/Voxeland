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

    [SerializeField] VoxelMaster master;
    internal static GenerationActionDelegate GenerationAction = null;
    Camera mainCamera;
    Vector3 renderPos;
    int meshesLastFrame = 0;


    void Start() { StartCoroutine(UpdateGeneration()); }
    void Update()
    {
        if (mainCamera != null)
            if (Vector3.Distance(renderPos, mainCamera.transform.position) >= updateDistance)
                renderPos = mainCamera.transform.position;
    }

    IEnumerator UpdateGeneration()
    {
        if (GenerationAction is null)
            GenerationAction = GetComponent<NodeGeneration>().Generation;

        if (mainCamera is null)
        {
            yield return new WaitWhile(() => GameManager.Instance is null);
            yield return new WaitWhile(() => GameManager.Instance.m_MainCamera is null);

            mainCamera = GameManager.Instance.m_MainCamera;
        }

        UpdateNow = true;
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
                    yield return new WaitWhile(() => GameManager.Instance is null);

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
        Vector3Int _pInt = VoxelMaster.FloorToIntChunk(_p);
        Voxel[,,] nodes = new Voxel[Chunk.SIZE, Chunk.SIZE, Chunk.SIZE];
        bool isEmpty = true, isFilled = true;

        for (int x = -1; x <= Chunk.SIZE; x++)
            for (int y = -1; y <= Chunk.SIZE; y++)
                for (int z = -1; z <= Chunk.SIZE; z++)
                {
                    var id = GenerationAction(
                        x * (1 << _l) + _pInt.x,
                        y * (1 << _l) + _pInt.y,
                        z * (1 << _l) + _pInt.z,
                        _l);

                    if (id != -1)
                    {
                        if ((x & Chunk.MASK) != x ||
                            (y & Chunk.MASK) != y ||
                            (z & Chunk.MASK) != z)
                            continue;

                        isEmpty = false;
                        nodes[x, y, z] = new Voxel((byte)id);
                    }
                    else
                        isFilled = false;
                }

        if (_l == 0)
            isFilled = false;

        if (!isEmpty && !isFilled)
        {
            Chunk chunk = master.CreateChunk(_p, _l);
            chunk.SetNodes(nodes);
        }
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
