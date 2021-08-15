using UnityEngine;

public enum VoxelType { AIR = -1, GRASS = 0, STONE = 1, DIRT = 2, SAND = 3, CLAY = 4, WOOD = 5, LEAVE = 6 }
[RequireComponent(typeof(VoxelGeneration))]
public class NodeGeneration : MonoBehaviour
{
    float surface, detailMult, mountainBiome, desertBiome, oceanBiome;

    //Generates Nodes according to given World Position and LOD using efficient Noise and the remap functions.
    public virtual short Generation(int x, int y, int z, byte lod)
    {
        VoxelType voxel = VoxelType.AIR;
        y += 15;

        GetSurfaceHeigth(x, z);


        //Surface and underground
        if (y <= surface)
        {
            float caveValue = 0;
            if (lod == 0)
                caveValue = (float)NoiseS3D.Noise(x * 0.1f, y * 0.1f, z * 0.1f) * 2 * (float)NoiseS3D.Noise(x * 0.01f, y * 0.02f, z * 0.01f);

            if (y == surface && surface > 2)
                if (oceanBiome >= 0.1f && surface < 16)
                    voxel = VoxelType.SAND;
                else
                    voxel = desertBiome >= 0.5f
                        ? VoxelType.SAND
                        : VoxelType.GRASS;
            else if (y >= surface - 8 && surface > 6)
                voxel = desertBiome >= 0.5f
                    ? VoxelType.SAND
                    : VoxelType.DIRT;
            else if (y > detailMult * 10)
                voxel = VoxelType.CLAY;
            else if (caveValue < 0.45 && !((y < -17 + detailMult * 5) && (y > surface - 41)))
                voxel = (y == surface - 41) ? VoxelType.CLAY : VoxelType.STONE;
        }


        //trees and sky
        if (lod <= 2)
        {
            //Sky
            float skyValue = (float)NoiseS3D.Noise(x * 0.01f, y * 0.013f, z * 0.01f);

            if (y > surface + 40 && y < 120 && oceanBiome >= 0.1f)
                if (skyValue > 0.8f + detailMult * 0.2f + mountainBiome * 0.2f)
                    voxel = VoxelType.DIRT;


            //Trees
            float mask = Mathf.Clamp(Mathf.Pow(10, NoiseS3D.Noise(x * 0.5f, z * 0.5f)), 0, 1);
            float treeTrunk = NoiseS3D.Noise(x * 2, z * 3) * mask * 2 - 1;
            float treeLeaves = NoiseS3D.Noise(x * 0.2f, z * 0.2f) * mask * 2;

            if (y > surface)
                if (oceanBiome < 0.4f && desertBiome < 0.4f && surface > 15)
                {
                    if (treeTrunk >= 0.75f && y < surface + 8)
                        voxel = y == surface + 7
                            ? VoxelType.LEAVE
                            : VoxelType.WOOD;
                    if (y > surface + 5 && treeTrunk <= 0.925f && treeTrunk >= 0.475f && treeLeaves * Mathf.Clamp01(1 - Vector2.Distance(new Vector2(y, 0), new Vector2(surface + 7, 0)) / 5f) >= 0.25f)
                        voxel = VoxelType.LEAVE;
                }
        }


        return (short)voxel;
    }

    public float GetSurfaceHeigth(float x, float z)
    {
        surface = 0f;

        // Height data, regardless of biome
        float mountainContrib = NoiseS3D.Noise(x * 0.00666666f, z * 0.00666666f) * 40f;
        float desertContrib = 0.3f;
        float oceanContrib = 0.0f;
        float detailContrib = NoiseS3D.Noise(x * 0.05f, z * 0.05f, false) * 5f;

        // Biomes
        detailMult = NoiseS3D.Noise(x * 0.033333f, z * 0.033333f);
        mountainBiome = NoiseS3D.Noise(x * 0.005f, z * 0.005f);
        desertBiome = NoiseS3D.Noise(x * 0.003333f, z * 0.003333f) * NoiseS3D.Noise(x * 0.04f, z * 0.04f, false).Remap(-0.33f, 0.66f, 0.95f, 1.05f);
        oceanBiome = NoiseS3D.Noise(x * 0.0002f, z * 0.0002f);

        // Add biome contrib
        float mountainFinal = (mountainContrib * mountainBiome) + (detailContrib * detailMult) + 20;
        float desertFinal = (desertContrib * desertBiome) + (detailContrib * detailMult) + 20;
        float oceanFinal = (oceanContrib * oceanBiome);

        // Final contrib
        surface = Mathf.Lerp(mountainFinal, desertFinal, desertBiome); // Decide between mountain biome or desert biome
        surface = Mathf.Lerp(surface, oceanFinal, oceanBiome); // Decide between the previous biome or ocean biome (aka ocean biome overrides all biomes)

        return surface = Mathf.Floor(surface);
    }
}
