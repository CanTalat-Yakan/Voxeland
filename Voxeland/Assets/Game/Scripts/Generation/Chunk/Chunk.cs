using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum FaceSide { FRONT, LEFT, BACK, RIGHT, TOP, BOTTOM }
public struct MeshInfo
{
    public MeshRenderer Renderer;
    public MeshFilter Filter;
    public MeshCollider Collider;
    public ChunkManager Manager;
    public Mesh Mesh;
}
public class Chunk
{
    static int curID = 0;
    internal const byte SIZE = 1 << 4;
    internal byte LOD { get; private set; }
    internal int ID { get; private set; }
    internal bool visible;
    internal bool SetVisible
    {
        get { return visible; }
        set { if (value == visible) return; visible = value; info.Renderer.enabled = visible; if (visible) FastRefresh(); }
    }
    internal bool Dirty { get; set; }
    internal bool Empty { get; private set; }
    internal GameObject gameObject { get; private set; }
    int x, y, z;
    internal Vector3Int Pos { get { return new Vector3Int(x, y, z); } }
    internal Vector3 CenteredPosition { get { return Pos + Vector3.one * HalfDiameter; } }
    internal float Diameter { get { return Chunk.SIZE << LOD; } }
    internal float HalfDiameter { get { return Diameter * 0.5f; } }
    internal VoxelMaster Master { get; set; }
    int? blockCount = null;
    internal int count
    {
        get
        {
            if (blockCount is null)
            {
                blockCount = 0;
                for (int x = 0; x < nodes.GetLength(0); x++)
                    for (int y = 0; y < nodes.GetLength(1); y++)
                        for (int z = 0; z < nodes.GetLength(2); z++)
                            if (nodes[x, y, z] != null) blockCount++;
            }
            return blockCount.Value;
        }
    }

    internal MeshInfo info;
    internal Voxel[,,] nodes;
    MeshBuilder builder;


    internal Chunk(VoxelMaster _p, byte _l, int _x, int _y, int _z)
    {
        ID = curID; curID++;
        Master = _p;
        LOD = _l;
        x = _x;
        y = _y;
        z = _z;

        gameObject = VoxelGeneration.gameObjectPool.Get();
        gameObject.name = ($"ID:{ID}_{LOD} ({_x},{_y},{_z})");
        gameObject.tag = LOD.ToString();
        gameObject.transform.position = Pos;
        gameObject.transform.localScale *= (1 << LOD);
        gameObject.transform.position *= 1 - LOD * 0.0005f;
        gameObject.transform.localScale *= 1 - LOD * 0.001f;

        gameObject.transform.SetParent(_p.transform);
        gameObject.isStatic = true;
        Dirty = false;

        nodes = new Voxel[SIZE, SIZE, SIZE];

        info = new MeshInfo()
        {
            Mesh = new Mesh(),
            Filter = gameObject.AddComponent<MeshFilter>(),
            Renderer = gameObject.AddComponent<MeshRenderer>(),
            Collider = gameObject.AddComponent<MeshCollider>(),
            Manager = gameObject.AddComponent<ChunkManager>(),
        };
        info.Mesh.Optimize();
        info.Mesh.MarkDynamic();
        info.Manager.Chunk = this;
        info.Renderer.material.color = UnityEngine.Random.ColorHSV();
    }
    internal void Remove()
    {
        UnityEngine.Object.Destroy(info.Mesh); // Destroy the mesh to save memory.
        Master.RemoveChunk(this, LOD);
    }

    internal void SetNodes(Voxel[,,] _n)
    {
        // info.Filter.sharedMesh = Parent.DefaultCubeMesh;

        this.nodes = _n;
        this.Dirty = true;
    }

    internal void FastRefresh()
    {
        if (Dirty && visible)
            Refresh();
    }
    internal void Refresh()
    {
        if (Master.TerrainMaterial == null || Master.VoxelDictionary == null || Master.VoxelDictionary.VoxelInfo.Length <= 0)
            return;

        builder = new MeshBuilder(this);

        info.Renderer.sharedMaterial = Master.TerrainMaterial;

        List<Vector3> vertices = new List<Vector3>(count * 24);
        List<int> triangles = new List<int>(count * 36);
        List<Vector2> uv = new List<Vector2>(count * 24);
        List<Color32> colors = new List<Color32>(count * 24);

        Thread t = new Thread(() => builder.GenerateMesh(vertices, triangles, uv, colors))
        { Priority = System.Threading.ThreadPriority.Normal };
        // t.IsBackground = true;
        t.Start();
    }

    internal short GetVoxelID(int _x, int _y, int _z)
    {
        if ((_x < 0 || _x >= Chunk.SIZE)
            || (_y < 0 || _y >= Chunk.SIZE)
            || (_z < 0 || _z >= Chunk.SIZE))
            return -1;

        Voxel v = nodes[_x, _y, _z];
        return (v != null ? v.ID : (short)-1);
    }
    internal short GetVoxelID(Vector3Int _pos)
    {
        return GetVoxelID(_pos.x, _pos.y, _pos.z);
    }
    internal Voxel GetVoxel(int _x, int _y, int _z)
    {
        if ((_x < 0 || _x >= Chunk.SIZE)
            || (_y < 0 || _y >= Chunk.SIZE)
            || (_z < 0 || _z >= Chunk.SIZE))
            return null;

        return nodes[_x, _y, _z];
    }
    internal Voxel GetVoxel(Vector3Int _pos)
    {
        return GetVoxel(_pos.x, _pos.y, _pos.z);
    }
    internal void SetVoxelID(int _x, int _y, int _z, short _id = 0)
    {
        if ((_x < 0 || _x >= Chunk.SIZE)
            || (_y < 0 || _y >= Chunk.SIZE)
            || (_z < 0 || _z >= Chunk.SIZE))
            return;

        if (_id == -1)
            nodes[_x, _y, _z] = null;
        else
            nodes[_x, _y, _z] = new Voxel(this, (byte)_id);

        info.Manager.Dirty = true;
        Dirty = true;
    }
    internal void SetVoxelID(Vector3Int _p, short _id = 0)
    {
        SetVoxelID(_p.x, _p.y, _p.z, _id);
    }
}