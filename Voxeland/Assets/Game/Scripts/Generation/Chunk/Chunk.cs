using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Jobs;
using UnityEngine;


public class Chunk
{
    static int curID = 0;
    internal const byte SIZE = 1 << 4;
    internal const byte MASK = SIZE - 1;
    internal byte LOD { get; private set; }
    internal int ID { get; private set; }
    internal bool m_visible;
    internal bool SetVisible
    {
        get { return m_visible; }
        set { if (value == m_visible) return; m_visible = value; info.Renderer.enabled = m_visible; FastRefresh(); }
    }
    internal bool Dirty { get; set; }
    internal bool Empty { get; private set; }
    internal GameObject ThisGameObject { get; private set; }
    internal Vector3Int Pos { get; private set; }
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

    //Initializes the Chunk with according LOD Factor and given Position
    internal Chunk(VoxelMaster _p, byte _l, Vector3Int _pos)
    {
        ID = curID; curID++;
        Master = _p;
        LOD = _l;
        Pos = _pos;

        info = Master.PrefabPool.Get();
        info.Mesh = new Mesh();
        info.Mesh.Optimize();
        info.Mesh.MarkDynamic();
        info.Manager.Chunk = this;

        ThisGameObject = info.gameObject;
        ThisGameObject.name = ($"ID:{ID}_{LOD} ({Pos})");
        ThisGameObject.tag = LOD.ToString();
        ThisGameObject.transform.position = Pos;
        ThisGameObject.transform.localScale *= (1 << LOD);
        ThisGameObject.transform.position *= 1 - LOD * 0.0005f;
        ThisGameObject.transform.localScale *= 1 - LOD * 0.001f;

        ThisGameObject.transform.SetParent(_p.transform);
        ThisGameObject.isStatic = true;
        ThisGameObject.SetActive(true);
        Dirty = false;

        nodes = new Voxel[SIZE, SIZE, SIZE];

        // info.Renderer.material.color = UnityEngine.Random.ColorHSV();
    }
    internal void Remove()
    {
        UnityEngine.Object.Destroy(info.Mesh);
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
        if (Dirty && m_visible)
            Refresh();
    }
    internal void Refresh()
    {
        if (Master.TerrainMaterial == null || Master.VoxelDictionary == null || Master.VoxelDictionary.VoxelInfo.Length <= 0)
            return;

        MeshBuilder builder = new MeshBuilder(this);

        info.Renderer.sharedMaterial = Master.TerrainMaterial;

        //Threading set of because unperforment and pool not working

        // if (Application.platform == RuntimePlatform.WebGLPlayer)
        builder.GenerateMesh();
        //     else
        //     {
        //         Thread t = new Thread(new ThreadStart(() => builder.GenerateMesh()))
        //         { Priority = System.Threading.ThreadPriority.BelowNormal };
        //         t.Start();
        //     }
    }





    //Incoming - Helper Function used in Chunk



    internal short GetLocalVoxelID(int _x, int _y, int _z)
    {
        //BitMask gets safeInput
        if ((_x & MASK) != _x ||
            (_y & MASK) != _y ||
            (_z & MASK) != _z)
            return Master.GetVoxelID(new Vector3(_x, _y, _z) + Pos);
        // return -1;

        Voxel v = nodes[_x, _y, _z];
        return v != null ? v.ID : (short)-1;
    }
    internal short GetVoxelID(Vector3Int _pos)
    {
        return GetLocalVoxelID(_pos.x, _pos.y, _pos.z);
    }

    internal Voxel GetLocalVoxel(int _x, int _y, int _z)
    {
        if ((_x & MASK) != _x ||
            (_y & MASK) != _y ||
            (_z & MASK) != _z)
            return Master.GetVoxel(new Vector3(_x, _y, _z) + Pos);
        // return null;

        return nodes[_x, _y, _z];
    }
    internal Voxel GetLocalVoxel(Vector3Int _pos)
    {
        return GetLocalVoxel(_pos.x, _pos.y, _pos.z);
    }

    internal void SetLocalVoxelID(int _x, int _y, int _z, short _id = 0)
    {
        if ((_x & MASK) != _x ||
            (_y & MASK) != _y ||
            (_z & MASK) != _z)
            return;

        if (_id == -1)
            nodes[_x, _y, _z] = null;
        else
            nodes[_x, _y, _z] = new Voxel((byte)_id);

        info.Manager.Dirty = true;
        Dirty = true;
    }
    internal void SetLocalVoxelID(Vector3Int _p, short _id = 0)
    {
        SetLocalVoxelID(_p.x, _p.y, _p.z, _id);
    }
}