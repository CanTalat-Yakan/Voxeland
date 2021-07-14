using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VoxelMaster : MonoBehaviour
{
    public static readonly Queue<UnityAction> MainThread = new Queue<UnityAction>();
    public static readonly Queue<UnityAction> ColliderBuffer = new Queue<UnityAction>();
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
    internal Dictionary<byte, Dictionary<Vector3, Chunk>> Collection = new Dictionary<byte, Dictionary<Vector3, Chunk>>();
    internal Mesh DefaultCubeMesh;

    void OnValidate() { VoxelGeneration.UpdateNow = true; }
    void Awake()
    {
        var gob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DefaultCubeMesh = gob.GetComponent<MeshFilter>().sharedMesh;
        Destroy(gob);

        for (byte i = 0; i < LevelOfDetailsCount + 1; i++)
            Collection.Add(i, new Dictionary<Vector3, Chunk>());
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

        Chunk c = FindChunk(pos, 0);

        if (c is null)
            return;

        c.SetLocalVoxelID(pos - c.Pos, _id);
    }
    internal short GetLocalVoxelID(Vector3 _pos)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);

        Chunk c = FindChunk(pos, 0);

        if (c is null)
            return -1;

        short id = c.GetVoxelID(pos - c.Pos);
        return id;
    }
    internal Voxel GetVoxel(Vector3 _pos)
    {
        Vector3Int pos = Vector3Int.FloorToInt(_pos);

        Chunk c = FindChunk(pos, 0);

        if (c is null)
            return null;

        return c.GetLocalVoxel(pos - c.Pos);
    }
    internal void FastRefresh()
    {
        foreach (Chunk c in Collection[0].Values)
            c.FastRefresh();
    }
    internal bool ChunkExists(Vector3 _p, byte _l)
    {
        CheckCollectionContainsLOD(_l);
        Chunk c = null; Collection[_l].TryGetValue(FloorToInt(_p), out c);
        return c != null;
    }
    internal Chunk FindChunk(Vector3 _p, byte _l)
    {
        CheckCollectionContainsLOD(_l);
        Chunk c = null;
        Collection[_l].TryGetValue(FloorToInt(_p), out c);
        return c;
    }
    internal Chunk CreateChunk(Vector3 _p, byte _l)
    {
        CheckCollectionContainsLOD(_l);
        Vector3Int pos = FloorToInt(_p);
        Chunk newChunk = new Chunk(this, _l, pos.x, pos.y, pos.z);
        AddChunk(newChunk, _l);
        return newChunk;
    }
    void AddChunk(Chunk c, byte _l)
    {
        CheckCollectionContainsLOD(_l);
        if (Collection.Count - 1 < _l)
            Collection.Add(_l, new Dictionary<Vector3, Chunk>());

        if (Collection[_l].ContainsKey(c.Pos))
            return;

        c.Master = this;
        Collection[_l].Add(c.Pos, c);
    }
    internal void RemoveChunk(Chunk _c, byte _l)
    {
        Destroy(_c.ThisGameObject);
        Collection[_l].Remove(_c.Pos);
    }

    internal static Vector3Int FloorToInt(Vector3 _p)
    {
        Vector3Int v = Vector3Int.FloorToInt(_p);
        v = Vector3Int.FloorToInt((Vector3)v / Chunk.SIZE);
        v *= Chunk.SIZE;

        return v;
    }
    void CheckCollectionContainsLOD(byte _l)
    {
        if (Collection.Count - 1 < _l)
            Collection.Add(_l, new Dictionary<Vector3, Chunk>());
    }
}
