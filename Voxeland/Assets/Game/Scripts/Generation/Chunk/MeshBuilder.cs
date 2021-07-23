using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Quad
{
    public Vector3Int p;
    public Vector3Int[] v;
    public Vector3Int n;
    public Vector2[] uv;
    public Color32 c;
    int[] index;
    public int[] i { get => index is null ? index = MeshBuilder.GetLightLevel(n, c, v, p) : index; }
}
public enum FaceSide
{
    FRONT,
    LEFT,
    BACK,
    RIGHT,
    TOP,
    BOTTOM
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

                    adjVoxels = GetAdjacents(pos);

                    bool front = GetAir(FaceSide.FRONT);
                    bool left = GetAir(FaceSide.LEFT);
                    bool back = GetAir(FaceSide.BACK);
                    bool right = GetAir(FaceSide.RIGHT);
                    bool top = GetAir(FaceSide.TOP);
                    bool bottom = GetAir(FaceSide.BOTTOM);

                    {
                        front |= GetTransperency(FaceSide.FRONT);
                        left |= GetTransperency(FaceSide.LEFT);
                        back |= GetTransperency(FaceSide.BACK);
                        right |= GetTransperency(FaceSide.RIGHT);
                        top |= GetTransperency(FaceSide.TOP);
                        bottom |= GetTransperency(FaceSide.BOTTOM);
                    }

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


                    if (front)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.forward,
                            v = GetVertices(FaceSide.FRONT, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.front),
                            c = GetColor(1)
                        });

                    if (left)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.left,
                            v = GetVertices(FaceSide.LEFT, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.left),
                            c = GetColor(1)
                        });

                    if (back)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.back,
                            v = GetVertices(FaceSide.BACK, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.back),
                            c = GetColor(1)
                        });

                    if (right)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.right,
                            v = GetVertices(FaceSide.RIGHT, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.right),
                            c = GetColor(1)
                        });

                    if (top)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.up,
                            v = GetVertices(FaceSide.TOP, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.top),
                            c = GetColor(1)
                        });

                    if (bottom)
                        faces.Add(new Quad()
                        {
                            p = pos,
                            n = Vector3Int.down,
                            v = GetVertices(FaceSide.BOTTOM, pos),
                            uv = GetUVs(voxelInfo.VoxelTexture.bottom),
                            c = GetColor(1)
                        });
                }

        if (faces.Count == 0)
            return;

        SetMesh();
    }

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
                // colors[vertIndex] = MeshBuilder.GetLightColor(quad.c, quad.i[i]);
                uvs[vertIndex] = quad.uv[i];
                verts[vertIndex] = quad.v[i];
                vertIndex++;
            }
        }

        VoxelMaster.MainThread.Enqueue(() =>
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
            parent.GetLocalVoxel(_p + Vector3Int.down)};
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

    static Color32 GetLightColor(Color32 color, int lightLevel)
    {
        if (lightLevel != 0)
        {
            float lightModifier = 1.0f - lightLevel * 0.24f;

            color.r = (byte)(color.r * lightModifier);
            color.g = (byte)(color.g * lightModifier);
            color.b = (byte)(color.b * lightModifier);
        }

        return color;
    }

    internal static int[] GetLightLevel(Vector3 _n, Color32 _c, Vector3Int[] _v, Vector3Int _p)
    {
        int[] lightLevels = new int[4];

        // if (_n.Equals(Vector3.forward))
        //     for (int i = 0; i < 4; i++)
        //         lightLevels[i] = LightLevelZ(_v[i], _c, _p.x, _p.y, 0.25f);

        // else if (_n.Equals(Vector3.left))
        //     for (int i = 0; i < 4; i++)
        //         lightLevels[i] = LightLevelX(_v[i], _c, _p.y, _p.z, -0.25f);

        // else if (_n.Equals(Vector3.back))
        //     for (int i = 0; i < 4; i++)
        //         lightLevels[i] = LightLevelZ(_v[i], _c, _p.x, _p.y, -0.25f);

        // else if (_n.Equals(Vector3.right))
        //     for (int i = 0; i < 4; i++)
        //         lightLevels[i] = LightLevelX(_v[i], _c, _p.y, _p.z, 0.25f);

        // else if (_n.Equals(Vector3.up))
        //     for (int i = 0; i < 4; i++)
        //         lightLevels[i] = LightLevelY(_v[i], _c, _p.x, _p.z, 0.25f);

        // else if (_n.Equals(Vector3.down))
        //     for (int i = 0; i < 4; i++)
        //         lightLevels[i] = LightLevelY(_v[i], _c, _p.x, _p.z, -0.25f);

        return lightLevels;
    }

    // static int LightLevelX(Vector3Int vert, Color32 color, int localY, int localZ, float xOffset)
    // {
    //     int lightLevel = 0;
    //     vert.x += xOffset;

    //     if (!lightLevels.TryGetValue(vert, out lightLevel))
    //     {
    //         int sides = 0;
    //         int corner = 0;

    //         if (localY == iy)
    //         {
    //             if (localZ == iz)
    //             {
    //                 if (Parent(ix, iy + 1, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy + 1, iz + 1).IsSolid())
    //                     corner++;
    //             }
    //             else
    //             {
    //                 if (GetAdjVoxel(ix, iy + 1, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
    //                     corner++;
    //             }
    //         }
    //         else
    //         {
    //             if (localZ == iz)
    //             {
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy + 1, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
    //                     corner++;
    //             }
    //             else
    //             {
    //                 if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     corner++;
    //             }
    //         }

    //         if (sides == 2)
    //             lightLevel = 3;
    //         else
    //             lightLevel = sides + corner;

    //         lightLevels.Add(vert, lightLevel);
    //     }
    //     return lightLevel;
    // }

    // static int LightLevelY(Vector3Int vert, Color32 color, int localX, int localZ, float yOffset)
    // {
    //     int lightLevel = 0;
    //     vert.y += yOffset;

    //     if (!lightLevels.TryGetValue(vert, out lightLevel))
    //     {
    //         int ix = FastFloor(vert.x);
    //         int iy = FastRound(vert.y);
    //         int iz = FastFloor(vert.z);
    //         int sides = 0;
    //         int corner = 0;

    //         if (localX == ix)
    //         {
    //             if (localZ == iz)
    //             {
    //                 if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy, iz + 1).IsSolid())
    //                     corner++;
    //             }
    //             else
    //             {
    //                 if (GetAdjVoxel(ix + 1, iy, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
    //                     corner++;
    //             }
    //         }
    //         else
    //         {
    //             if (localZ == iz)
    //             {
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
    //                     corner++;
    //             }
    //             else
    //             {
    //                 if (GetAdjVoxel(ix, iy, iz + 1).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     corner++;
    //             }
    //         }

    //         if (sides == 2)
    //             lightLevel = 3;
    //         else
    //             lightLevel = sides + corner;

    //         lightLevels.Add(vert, lightLevel);
    //     }
    //     return lightLevel;
    // }

    // static int LightLevelZ(Vector3Int vert, Color32 color, int localX, int localY, float zOffset)
    // {
    //     int lightLevel = 0;
    //     vert.z += zOffset;

    //     if (!lightLevels.TryGetValue(vert, out lightLevel))
    //     {
    //         int ix = FastFloor(vert.x);
    //         int iy = FastFloor(vert.y);
    //         int iz = FastRound(vert.z);
    //         int sides = 0;
    //         int corner = 0;

    //         if (localX == ix)
    //         {
    //             if (localY == iy)
    //             {
    //                 if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy + 1, iz).IsSolid())
    //                     corner++;
    //             }
    //             else
    //             {
    //                 if (GetAdjVoxel(ix + 1, iy + 1, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
    //                     corner++;
    //             }
    //         }
    //         else
    //         {
    //             if (localY == iy)
    //             {
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy + 1, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
    //                     corner++;
    //             }
    //             else
    //             {
    //                 if (GetAdjVoxel(ix, iy + 1, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix + 1, iy, iz).IsSolid())
    //                     sides++;
    //                 if (GetAdjVoxel(ix, iy, iz).IsSolid())
    //                     corner++;
    //             }
    //         }

    //         if (sides == 2)
    //             lightLevel = 3;
    //         else
    //             lightLevel = sides + corner;

    //         lightLevels.Add(vert, lightLevel);
    //     }
    //     return lightLevel;
    // }

    static int FastFloor(float f) { return f >= 0.0f ? (int)f : (int)f - 1; }
    static int FastRound(float f) { return (f >= 0.0f) ? (int)(f + 0.5f) : (int)(f - 0.5f); }
}
