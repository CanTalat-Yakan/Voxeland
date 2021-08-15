using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public struct Quad
{
    public Vector3Int p;
    public Vector3Int[] v;
    public Vector3Int n;
    public Vector2[] uv;
    public Color32 c;
    int[] index;
    public int[] i { get => index; set => index = value; }
}
public enum FaceSide
{
    FRONT, LEFT, BACK, RIGHT, TOP, BOTTOM,
    YCORNERXZ, YCORNER_XZ, YCORNER_X_Z, YCORNERX_Z,
    _YCORNERXZ, _YCORNER_XZ, _YCORNER_X_Z, _YCORNERX_Z,
    YSIDEX, YSIDEZ, YSIDE_X, YSIDE_Z,
    _YSIDEX, _YSIDEZ, _YSIDE_X, _YSIDE_Z
}

public class MeshBuilder
{
    List<Quad> faces = new List<Quad>();
    Voxel[] adjVoxels;
    Chunk parent;

    public MeshBuilder(Chunk _p)
    {
        parent = _p;
    }

    //Generates mesh by iterating through all Voxels inside Chunk that is same size three dimensional grid with a unit of 1 meter. LOD scales up gameObject
    internal void GenerateMesh()
    {
        Vector3Int pos;
        VoxelInfo voxelInfo;

        for (int x = 0; x < Chunk.SIZE; x++)
            for (int y = 0; y < Chunk.SIZE; y++)
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    if (!parent.m_visible)
                    {
                        parent.Dirty = true;
                        return;
                    }

                    if (parent.GetLocalVoxelID(x, y, z) == -1)
                        continue;

                    if ((voxelInfo = parent.GetLocalVoxel(x, y, z).Info) is null)
                        continue;

                    pos = new Vector3Int(x, y, z);

                    //All adjucent Blocks called one time to be used in generation and AO
                    adjVoxels = GetAdjacents(pos);

                    //Checks for Air Blocks to draw Quads
                    bool front = GetAir(FaceSide.FRONT);
                    bool left = GetAir(FaceSide.LEFT);
                    bool back = GetAir(FaceSide.BACK);
                    bool right = GetAir(FaceSide.RIGHT);
                    bool top = GetAir(FaceSide.TOP);
                    bool bottom = GetAir(FaceSide.BOTTOM);

                    //Checks for Transparent Voxel, like Leaves and displays the adjucent Voxel inside facing Quads
                    {
                        front |= GetTransperency(FaceSide.FRONT);
                        left |= GetTransperency(FaceSide.LEFT);
                        back |= GetTransperency(FaceSide.BACK);
                        right |= GetTransperency(FaceSide.RIGHT);
                        top |= GetTransperency(FaceSide.TOP);
                        bottom |= GetTransperency(FaceSide.BOTTOM);
                    }

                    //If the outside walls of the Chunks should be created, for seamless experience enable. Updating neighbour chunks not available
                    bool b = true;
                    if (true)
                    {
                        if (x == Chunk.SIZE - 1) right = b;
                        if (y == Chunk.SIZE - 1) top = b;
                        if (z == Chunk.SIZE - 1) front = b;
                        if (x == 0) left = b;
                        if (y == 0) bottom = b;
                        if (z == 0) back = b;
                    }

                    //Checks if Side is Visible and then creates Quad and adds it to List to be added in the Mesh buffer in the last step
                    if (front)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.forward,
                            v = GetVertices(FaceSide.FRONT, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.front),
                            c = GetColor(1),
                            i = GetLightLevel(Vector3Int.forward)
                        });

                    if (left)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.left,
                            v = GetVertices(FaceSide.LEFT, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.left),
                            c = GetColor(1),
                            i = GetLightLevel(Vector3Int.left)
                        });

                    if (back)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.back,
                            v = GetVertices(FaceSide.BACK, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.back),
                            c = GetColor(1),
                            i = GetLightLevel(Vector3Int.back)
                        });

                    if (right)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.right,
                            v = GetVertices(FaceSide.RIGHT, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.right),
                            c = GetColor(1),
                            i = GetLightLevel(Vector3Int.right)
                        });

                    if (top)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.up,
                            v = GetVertices(FaceSide.TOP, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.top),
                            c = GetColor(1),
                            i = GetLightLevel(Vector3Int.up)
                        });

                    if (bottom)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.down,
                            v = GetVertices(FaceSide.BOTTOM, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.bottom),
                            c = GetColor(1),
                            i = GetLightLevel(Vector3Int.down)
                        });
                }

        if (faces.Count == 0)
            return;

        SetMesh();
    }
    //Sets the Mesh Buffer with given Information of Quads and AO
    void SetMesh()
    {
        Vector3[] verts = new Vector3[faces.Count * 4];
        Vector3[] normals = new Vector3[faces.Count * 4];
        Vector2[] uvs = new Vector2[faces.Count * 4];
        Color32[] colors = new Color32[faces.Count * 4];
        int[] tris = new int[faces.Count * 6];
        int vertIndex = 0;
        int triIndex = 0;

        foreach (Quad quad in faces)
        {
            tris[triIndex++] = vertIndex;
            tris[triIndex++] = vertIndex + 1;
            tris[triIndex++] = vertIndex + 2;
            tris[triIndex++] = vertIndex;
            tris[triIndex++] = vertIndex + 2;
            tris[triIndex++] = vertIndex + 3;

            for (int i = 0; i < 4; i++)
            {
                normals[vertIndex] = quad.n;
                colors[vertIndex] = quad.c;
                colors[vertIndex] = GetLightColor(quad.c, quad.i[i]);//AO
                uvs[vertIndex] = quad.uv[i];
                verts[vertIndex] = quad.v[i];
                vertIndex++;
            }
        }

        VoxelMaster.MainThread.Enqueue(() => //Enques to MainThread because settings mesh is not possible in threading solutions
        {
            parent.info.Mesh = new Mesh
            {
                vertices = verts,
                normals = normals,
                triangles = tris,
                colors32 = colors,
                uv = uvs
            };
            parent.info.Filter.sharedMesh = parent.info.Mesh;

            if (parent.LOD == 0)
                VoxelMaster.ColliderBuffer.Enqueue(() => parent.info.Collider.sharedMesh = parent.info.Mesh);

            parent.Dirty = false;
        });
    }

    Voxel[] GetAdjacents(Vector3Int _p)
    {
        return new Voxel[] {
            parent.GetLocalVoxel(_p + Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.left),
            parent.GetLocalVoxel(_p + Vector3Int.back),
            parent.GetLocalVoxel(_p + Vector3Int.right),
            parent.GetLocalVoxel(_p + Vector3Int.up),
            parent.GetLocalVoxel(_p + Vector3Int.down),
            parent.GetLocalVoxel(_p + Vector3Int.up + Vector3Int.right + Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.up - Vector3Int.right + Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.up - Vector3Int.right - Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.up + Vector3Int.right - Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.down + Vector3Int.right + Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.down - Vector3Int.right + Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.down - Vector3Int.right - Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.down + Vector3Int.right - Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.up + Vector3Int.right),
            parent.GetLocalVoxel(_p + Vector3Int.up + Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.up - Vector3Int.right),
            parent.GetLocalVoxel(_p + Vector3Int.up - Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.down + Vector3Int.right),
            parent.GetLocalVoxel(_p + Vector3Int.down + Vector3Int.forward),
            parent.GetLocalVoxel(_p + Vector3Int.down - Vector3Int.right),
            parent.GetLocalVoxel(_p + Vector3Int.down - Vector3Int.forward)};
    }

    bool GetTransperency(FaceSide _s)
    {
        Voxel v = adjVoxels[(int)_s];
        return v is null ? false : v.Info.Transparent;
    }

    bool GetAir(FaceSide _s)
    {
        Voxel v = adjVoxels[(int)_s];
        return v is null ? true : v.ID == (short)VoxelType.AIR;
    }

    bool GetSolid(FaceSide _s)
    {
        return !GetAir(_s);
    }

    Vector2[] GetUVs(int id)
    {
        Vector2[] uv = new Vector2[4];
        float tiling = parent.Master.Tiling;

        int id2 = id + 1;
        float o = 1f / tiling;
        int i = 0;

        for (int y = 0; y < tiling; y++)
            for (int x = 0; x < tiling; x++)
                if (++i == id2)
                {
                    float padding = parent.Master.UVPadding / tiling;
                    uv[0] = new Vector2(x / tiling + padding, 1f - (y / tiling) - padding);
                    uv[1] = new Vector2(x / tiling + o - padding, 1f - (y / tiling) - padding);
                    uv[2] = new Vector2(x / tiling + o - padding, 1f - (y / tiling + o) + padding);
                    uv[3] = new Vector2(x / tiling + padding, 1f - (y / tiling + o) + padding);

                    return uv;
                }

        uv[0] = Vector2.zero;
        uv[1] = Vector2.zero;
        uv[2] = Vector2.zero;
        uv[3] = Vector2.zero;

        return uv;
    }

    Vector3Int[] GetVertices(FaceSide _s, Vector3Int _p)
    {
        Vector3Int[] vec = new Vector3Int[4];

        switch (_s)
        {
            case FaceSide.FRONT:
                vec = new Vector3Int[] {
                    new Vector3Int(1, 1, 1) + _p,
                    new Vector3Int(0, 1, 1) + _p,
                    new Vector3Int(0, 0, 1) + _p,
                    new Vector3Int(1, 0, 1) + _p};
                break;
            case FaceSide.LEFT:
                vec = new Vector3Int[] {
                    new Vector3Int(0, 1, 1) + _p,
                    new Vector3Int(0, 1, 0) + _p,
                    new Vector3Int(0, 0, 0) + _p,
                    new Vector3Int(0, 0, 1) + _p};
                break;
            case FaceSide.BACK:
                vec = new Vector3Int[] {
                    new Vector3Int(0, 1, 0) + _p,
                    new Vector3Int(1, 1, 0) + _p,
                    new Vector3Int(1, 0, 0) + _p,
                    new Vector3Int(0, 0, 0) + _p};
                break;
            case FaceSide.RIGHT:
                vec = new Vector3Int[] {
                    new Vector3Int(1, 1, 0) + _p,
                    new Vector3Int(1, 1, 1) + _p,
                    new Vector3Int(1, 0, 1) + _p,
                    new Vector3Int(1, 0, 0) + _p};
                break;
            case FaceSide.TOP:
                vec = new Vector3Int[] {
                    new Vector3Int(0, 1, 1) + _p,
                    new Vector3Int(1, 1, 1) + _p,
                    new Vector3Int(1, 1, 0) + _p,
                    new Vector3Int(0, 1, 0) + _p};
                break;
            case FaceSide.BOTTOM:
                vec = new Vector3Int[] {
                    new Vector3Int(0, 0, 0) + _p,
                    new Vector3Int(1, 0, 0) + _p,
                    new Vector3Int(1, 0, 1) + _p,
                    new Vector3Int(0, 0, 1) + _p};
                break;
            default: break;
        }

        return vec;
    }




    //Incoming AO Calculations
    //Checks existens ambient Voxels and calculates  if 2 sides and 1 Corner are there the sets the lightlevel accordingly

    Color32 GetColor(byte _d)
    {
        return new Color32(255, 255, 255, 255);

        // if (_d > 0)
        //     return Color32.Lerp(whiteColor, dirtColor, _d * 0.2f);
        // if (_d < 15f)
        //     return Color32.Lerp(grassColor, dirtColor, _d * 0.2f);

        // if (_d < 15f)
        //     return Color32.Lerp(dirtColor, stoneColor, Mathf.Pow(2, (_d - 5f) * 0.1f));

        // return stoneColor;
    }

    Color32 GetLightColor(Color32 color, int lightLevel)
    {
        if (lightLevel != 0)
        {
            float lightModifier = 1.0f - lightLevel * 0.18f;

            color.r = (byte)(color.r * lightModifier);
            color.g = (byte)(color.g * lightModifier);
            color.b = (byte)(color.b * lightModifier);
        }

        return color;
    }

    internal int[] GetLightLevel(Vector3 _n)
    {
        int[] lightLevels = new int[4];

        if (_n.Equals(Vector3.forward))
            for (int i = 0; i < 4; i++)
                lightLevels[i] = LightLevelZ(i, true); //1

        else if (_n.Equals(Vector3.left))
            for (int i = 0; i < 4; i++)
                lightLevels[i] = LightLevelX(i, false); //-1

        else if (_n.Equals(Vector3.back))
            for (int i = 0; i < 4; i++)
                lightLevels[i] = LightLevelZ(i, false); //-1

        else if (_n.Equals(Vector3.right))
            for (int i = 0; i < 4; i++)
                lightLevels[i] = LightLevelX(i, true); //1

        else if (_n.Equals(Vector3.up))
            for (int i = 0; i < 4; i++)
                lightLevels[i] = LightLevelY(i, true); //1

        else if (_n.Equals(Vector3.down))
            for (int i = 0; i < 4; i++)
                lightLevels[i] = LightLevelY(i, false); //-1

        return lightLevels;
    }

    int LightLevelX(int _i, bool _n)
    {
        int lightLevel = 0;

        int sides = 0;
        int corner = 0;

        if (_i == 0)
        {
            if (GetSolid(_n ? FaceSide.YSIDEX : FaceSide.YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDE_Z : FaceSide.YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNERX_Z : FaceSide.YCORNER_XZ)) corner++;
        }
        else if (_i == 1)
        {
            if (GetSolid(_n ? FaceSide.YSIDEX : FaceSide.YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDEZ : FaceSide.YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNERXZ : FaceSide.YCORNER_X_Z)) corner++;
        }
        else if (_i == 2)
        {
            if (GetSolid(_n ? FaceSide._YSIDEX : FaceSide._YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide._YSIDE_Z : FaceSide._YSIDEZ)) sides++;
            if (GetSolid(_n ? FaceSide._YCORNERX_Z : FaceSide._YCORNER_XZ)) corner++;
        }
        else if (_i == 3)
        {
            if (GetSolid(_n ? FaceSide._YSIDEX : FaceSide._YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide._YSIDEZ : FaceSide._YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide._YCORNERXZ : FaceSide._YCORNER_X_Z)) corner++;
        }

        if (sides == 2)
            lightLevel = 3;
        else
            lightLevel = sides + corner;

        return lightLevel;
    }
    int LightLevelY(int _i, bool _n)
    {
        int lightLevel = 0;

        int sides = 0;
        int corner = 0;

        if (_i == 0)
        {
            if (GetSolid(_n ? FaceSide.YSIDE_X : FaceSide.YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDEZ : FaceSide.YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNER_XZ : FaceSide.YCORNER_X_Z)) corner++;
        }
        else if (_i == 1)
        {
            if (GetSolid(_n ? FaceSide.YSIDEX : FaceSide.YSIDEX)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDEZ : FaceSide.YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNERXZ : FaceSide.YCORNERX_Z)) corner++;
        }
        else if (_i == 2)
        {
            if (GetSolid(_n ? FaceSide.YSIDEX : FaceSide.YSIDEX)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDE_Z : FaceSide.YSIDEZ)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNERX_Z : FaceSide.YCORNERXZ)) corner++;
        }
        else if (_i == 3)
        {
            if (GetSolid(_n ? FaceSide.YSIDE_X : FaceSide.YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDE_Z : FaceSide.YSIDEZ)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNER_X_Z : FaceSide.YCORNER_XZ)) corner++;
        }

        if (sides == 2)
            lightLevel = 3;
        else
            lightLevel = sides + corner;

        return lightLevel;
    }
    int LightLevelZ(int _i, bool _n)
    {
        int lightLevel = 0;

        int sides = 0;
        int corner = 0;

        if (_i == 0)
        {
            if (GetSolid(_n ? FaceSide.YSIDEX : FaceSide.YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDEZ : FaceSide.YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNERXZ : FaceSide.YCORNER_X_Z)) corner++;
        }
        else if (_i == 1)
        {
            if (GetSolid(_n ? FaceSide.YSIDE_X : FaceSide.YSIDEX)) sides++;
            if (GetSolid(_n ? FaceSide.YSIDEZ : FaceSide.YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide.YCORNER_XZ : FaceSide.YCORNERX_Z)) corner++;
        }
        else if (_i == 2)
        {
            if (GetSolid(_n ? FaceSide._YSIDE_X : FaceSide._YSIDEX)) sides++;
            if (GetSolid(_n ? FaceSide._YSIDEZ : FaceSide._YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide._YCORNER_XZ : FaceSide._YCORNERX_Z)) corner++;
        }
        else if (_i == 3)
        {
            if (GetSolid(_n ? FaceSide._YSIDEX : FaceSide._YSIDE_X)) sides++;
            if (GetSolid(_n ? FaceSide._YSIDEZ : FaceSide._YSIDE_Z)) sides++;
            if (GetSolid(_n ? FaceSide._YCORNERXZ : FaceSide._YCORNER_X_Z)) corner++;
        }

        if (sides == 2)
            lightLevel = 3;
        else
            lightLevel = sides + corner;

        return lightLevel;
    }
}
