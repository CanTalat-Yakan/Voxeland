using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder
{
    Chunk parent;

    public MeshBuilder(Chunk _p)
    {
        parent = _p;
    }

    internal void GenerateMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uv, List<Color32> colors)
    {
        int faceCount = 0;
        int o = 0;

        for (int x = 0; x < Chunk.SIZE; x++)
            for (int y = 0; y < Chunk.SIZE; y++)
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    if (!parent.visible)
                    {
                        parent.Dirty = true;
                        return; // Abort mesh construction if not visible whilst construction
                    }

                    if (parent.GetVoxelID(x, y, z) == -1)
                        continue;

                    VoxelInfo voxelInfo = parent.GetVoxel(x, y, z).Info;

                    o = vertices.Count; // offset
                    Vector3Int pos = new Vector3Int(x, y, z);

                    bool front = parent.GetVoxelID(x, y, z - 1) == -1 || (!voxelInfo.Transparent && parent.GetVoxel(x, y, z - 1).Info.Transparent);
                    bool back = parent.GetVoxelID(x, y, z + 1) == -1 || (!voxelInfo.Transparent && parent.GetVoxel(x, y, z + 1).Info.Transparent);
                    bool left = parent.GetVoxelID(x - 1, y, z) == -1 || (!voxelInfo.Transparent && parent.GetVoxel(x - 1, y, z).Info.Transparent);
                    bool right = parent.GetVoxelID(x + 1, y, z) == -1 || (!voxelInfo.Transparent && parent.GetVoxel(x + 1, y, z).Info.Transparent);
                    bool top = parent.GetVoxelID(x, y + 1, z) == -1 || (!voxelInfo.Transparent && parent.GetVoxel(x, y + 1, z).Info.Transparent);
                    bool bottom = parent.GetVoxelID(x, y - 1, z) == -1 || (!voxelInfo.Transparent && parent.GetVoxel(x, y - 1, z).Info.Transparent);

                    if (voxelInfo == null)
                        return;

                    faceCount = 0;

                    // VERTICES

                    if (front)
                    {
                        vertices.AddRange(GetVertices(FaceSide.FRONT, pos));
                        uv.AddRange(GetUVs(voxelInfo.VoxelTexture.front));
                        colors.AddRange(GetColors(1));
                        faceCount++;
                    }
                    if (left)
                    {
                        vertices.AddRange(GetVertices(FaceSide.LEFT, pos));
                        uv.AddRange(GetUVs(voxelInfo.VoxelTexture.left));
                        colors.AddRange(GetColors(1));
                        faceCount++;
                    }
                    if (back)
                    {
                        vertices.AddRange(GetVertices(FaceSide.BACK, pos));
                        uv.AddRange(GetUVs(voxelInfo.VoxelTexture.back));
                        colors.AddRange(GetColors(1));
                        faceCount++;
                    }
                    if (right)
                    {
                        vertices.AddRange(GetVertices(FaceSide.RIGHT, pos));
                        uv.AddRange(GetUVs(voxelInfo.VoxelTexture.right));
                        colors.AddRange(GetColors(1));
                        faceCount++;
                    }
                    if (top)
                    {
                        vertices.AddRange(GetVertices(FaceSide.TOP, pos));
                        uv.AddRange(GetUVs(voxelInfo.VoxelTexture.top));
                        colors.AddRange(GetColors(1));
                        faceCount++;
                    }
                    if (bottom)
                    {
                        vertices.AddRange(GetVertices(FaceSide.BOTTOM, pos));
                        uv.AddRange(GetUVs(voxelInfo.VoxelTexture.bottom));
                        colors.AddRange(GetColors(1));
                        faceCount++;
                    }

                    // TRIANGLES

                    for (int i = 0; i < faceCount; i++)
                    {
                        int o2 = i * 4;
                        triangles.AddRange(new int[]{
                                0+o+o2, 1+o+o2, 2+o+o2,
                                2+o+o2, 3+o+o2, 0+o+o2,
                            });
                    }
                }

        VoxelMaster.MainThread.Enqueue(() =>
        {
            FinishRefresh(
                vertices.ToArray(),
                triangles.ToArray(),
                uv.ToArray(),
                colors.ToArray());
        });
    }
    internal void FinishRefresh(Vector3[] vertices, int[] triangles, Vector2[] uv, Color32[] colors)
    {
        if (parent.info.Mesh == null || parent.info.Filter == null || parent.info.Collider == null) return;


        parent.info.Mesh.Clear();

        parent.info.Mesh.vertices = vertices;
        parent.info.Mesh.triangles = triangles;
        parent.info.Mesh.uv = uv;
        parent.info.Mesh.colors32 = colors;

        parent.info.Mesh.RecalculateNormals();

        parent.info.Filter.sharedMesh = parent.info.Mesh;

            parent.info.Collider.sharedMesh = null;
            parent.info.Collider.sharedMesh = parent.info.Mesh;
        VoxelMaster.ColliderBuffer.Enqueue(() =>
        {
        });

        parent.Dirty = false;
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
                    float padding = parent.Master.UVPadding / tiling; // Adding a little padding to prevent UV bleeding (to fix)
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

    Vector3[] GetVertices(FaceSide _s, Vector3Int _p)
    {
        Vector3[] vec = new Vector3[4];

        switch (_s)
        {
            case FaceSide.FRONT:
                vec = new Vector3[] {
                    new Vector3(0, 1, 0) + _p,
                    new Vector3(1, 1, 0) + _p,
                    new Vector3(1, 0, 0) + _p,
                    new Vector3(0, 0, 0) + _p};
                break;
            case FaceSide.LEFT:
                vec = new Vector3[] {
                    new Vector3(0, 1, 1) + _p,
                    new Vector3(0, 1, 0) + _p,
                    new Vector3(0, 0, 0) + _p,
                    new Vector3(0, 0, 1) + _p};
                break;
            case FaceSide.BACK:
                vec = new Vector3[] {
                    new Vector3(1, 1, 1) + _p,
                    new Vector3(0, 1, 1) + _p,
                    new Vector3(0, 0, 1) + _p,
                    new Vector3(1, 0, 1) + _p};
                break;
            case FaceSide.RIGHT:
                vec = new Vector3[] {
                    new Vector3(1, 1, 0) + _p,
                    new Vector3(1, 1, 1) + _p,
                    new Vector3(1, 0, 1) + _p,
                    new Vector3(1, 0, 0) + _p};
                break;
            case FaceSide.TOP:
                vec = new Vector3[] {
                    new Vector3(0, 1, 1) + _p,
                    new Vector3(1, 1, 1) + _p,
                    new Vector3(1, 1, 0) + _p,
                    new Vector3(0, 1, 0) + _p};
                break;
            case FaceSide.BOTTOM:
                vec = new Vector3[] {
                    new Vector3(0, 0, 0) + _p,
                    new Vector3(1, 0, 0) + _p,
                    new Vector3(1, 0, 1) + _p,
                    new Vector3(0, 0, 1) + _p};
                break;
            default: break;
        }

        return vec;
    }

    Color32[] GetColors(byte _d)
    {
        Color32[] colors = new Color32[4];

        colors[0] = DensityColor(_d);
        colors[1] = DensityColor(_d);
        colors[2] = DensityColor(_d);
        colors[3] = DensityColor(_d);

        return colors;
    }

    Color32 grassColor = new Color32(112, 150, 48, 255);
    Color32 dirtColor = new Color32(97, 75, 66, 255);
    Color32 stoneColor = new Color32(150, 150, 150, 255);
    Color32 DensityColor(byte _d)
    {
        Color32 whiteColor = new Color32(255, 255, 255, 255);
        Color32 grassColor = new Color32(112, 150, 48, 255);
        Color32 dirtColor = new Color32(97, 75, 66, 255);
        Color32 stoneColor = new Color32(150, 150, 150, 255);

        if (_d > 0)
            return Color32.Lerp(whiteColor, dirtColor, _d * 0.2f);
        if (_d < 15f)
            return Color32.Lerp(grassColor, dirtColor, _d * 0.2f);

        if (_d < 15f)
            return Color32.Lerp(dirtColor, stoneColor, Mathf.Pow(2, (_d - 5f) * 0.1f));

        return stoneColor;
    }
}
