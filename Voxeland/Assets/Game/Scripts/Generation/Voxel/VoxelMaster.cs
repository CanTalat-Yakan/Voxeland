using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class VoxelMaster : MonoBehaviour
{
    public static readonly Queue<UnityAction> MainThread = new Queue<UnityAction>();
    public static readonly Queue<UnityAction> ColliderBuffer = new Queue<UnityAction>();
    internal static ObjectPool<Thread> ThreadPool = new ObjectPool<Thread>(8);

    [SerializeField] internal byte LevelOfDetailsCount { get => (byte)Settings.LOD; }
    [SerializeField] internal byte RenderDistance { get => (byte)(6 + Settings.RenderDistance); }
    [SerializeField] public PrefabPool PrefabPool;
    [Space]
    [Range(1, 6)] [SerializeField] internal float DisposeFactor = 2;
    [Space]
    [SerializeField] internal SettingsContainer Settings;
    [SerializeField] internal VoxelDictionary VoxelDictionary;
    [SerializeField] internal Material TerrainMaterial;
    [SerializeField] internal int Tiling;
    [Range(0, 0.4999f)] [SerializeField] internal float UVPadding;

    internal Dictionary<Vector3, Chunk> MainCollection = new Dictionary<Vector3, Chunk>();
    internal Dictionary<byte, List<Vector3Int>> LODCollection = new Dictionary<byte, List<Vector3Int>>();
    internal Mesh DefaultCubeMesh;
    Chunk tmpChunk;


    void OnValidate() { VoxelGeneration.UpdateNow = true; }
    void Awake()
    {
        var gob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DefaultCubeMesh = gob.GetComponent<MeshFilter>().sharedMesh;
        Destroy(gob);

        for (byte i = 0; i < LevelOfDetailsCount + 1; i++)
            LODCollection.Add(i, new List<Vector3Int>());
    }
    void Update()
    {
        while (MainThread.Count > 0)
        {
            UnityAction action = MainThread.Dequeue();
            if (action != null) action();
        }

        while (ColliderBuffer.Count > 0)
        {
            UnityAction action = ColliderBuffer.Dequeue();
            if (action != null) action();
        }
    }

    internal void RemoveVoxelAt(Vector3 _pos)
    {
        SetVoxelID(_pos, -1);
    }
    internal void SetVoxelID(Vector3 _pos, short _id)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);

        if (tmpChunk != null && tmpChunk.Pos == FloorToIntChunk(_pos))
            tmpChunk.SetLocalVoxelID(pos - tmpChunk.Pos, _id);
        {
            Chunk c = FindChunk(pos);

            if (c is null)
                c = CreateChunk(_pos, 0);

            c.SetLocalVoxelID(pos - c.Pos, _id);
        }

    }
    internal short GetVoxelID(Vector3 _pos)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);

        Chunk c = FindChunk(pos);

        if (c is null)
            return -1;

        short id = c.GetVoxelID(pos - c.Pos);
        return id;
    }
    internal Voxel GetVoxel(Vector3 _pos)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);

        Chunk c = FindChunk(pos);

        if (c is null)
            return null;

        return c.GetLocalVoxel(pos - c.Pos);
    }
    internal void FastRefresh()
    {
        foreach (Chunk c in MainCollection.Values)
            c.FastRefresh();
    }
    internal bool ChunkExists(Vector3 _p, byte _l)
    {
        if (_l == 0)
            return MainCollection.TryGetValue(FloorToIntChunk(_p), out _);
        else
            CheckCollectionContainsLOD(_l);
        return LODCollection[_l].Contains(FloorToIntChunk(_p));
    }
    internal Chunk FindChunk(Vector3 _p)
    {
        Chunk c = null;
        MainCollection.TryGetValue(FloorToIntChunk(_p), out c);
        return c;
    }
    internal Chunk CreateChunk(Vector3 _p, byte _l)
    {
        CheckCollectionContainsLOD(_l);
        Vector3Int pos = FloorToIntChunk(_p);
        Chunk newChunk = new Chunk(this, _l, pos);
        newChunk.Master = this;
        AddChunk(newChunk, _l);
        return newChunk;
    }
    void AddChunk(Chunk _c, byte _l)
    {
        CheckCollectionContainsLOD(_l);
        if (!MainCollection.TryGetValue(_c.Pos, out _))
            MainCollection.Add(_c.Pos, _c);
    }
    internal void RemoveChunk(Chunk _c, byte _l)
    {
        _c.info.ResetAll();
        PrefabPool.Add(_c.info);
        if (_l == 0)
            MainCollection.Remove(_c.Pos);
        else
            LODCollection[_l].Remove(_c.Pos);
    }

    internal static Vector3Int FloorToIntChunk(Vector3 _p)
    {
        Vector3Int v = Vector3Int.FloorToInt(_p);
        v = Vector3Int.FloorToInt((Vector3)v / Chunk.SIZE);
        v *= Chunk.SIZE;

        return v;
    }
    void CheckCollectionContainsLOD(byte _l)
    {
        if (LODCollection.Count - 1 < _l)
            LODCollection.Add(_l, new List<Vector3Int>());
    }
}
