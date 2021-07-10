using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel
{
    internal Chunk Parent { get; set; }
    internal VoxelInfo Info { get; private set; }
    internal short ID { get; private set; }
    internal byte Density { get; private set; }

    internal Voxel(Chunk _p, short _id = 0, byte _d = 1)
    {
        ID = _id;
        Parent = _p;

        if (_p != null && _p.Master != null)
            SetVoxelInfo(_p.Master.VoxelDictionary);
    }
    void SetVoxelInfo(VoxelDictionary _dic)
    {
        for (int i = 0; i < _dic.VoxelInfo.Length; i++)
        {
            VoxelInfo current = _dic.VoxelInfo[i];
            if (ID == current.ID)
            {
                this.Info = current;
                return;
            }
        }
    }

    public override string ToString() { return (Info != null ? Info.VoxelName : "NONE"); }
}
