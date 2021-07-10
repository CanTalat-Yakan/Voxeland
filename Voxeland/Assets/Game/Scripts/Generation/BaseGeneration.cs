using UnityEngine;

public enum VoxelType { AIR = -1, GRASS = 0, STONE = 1, DIRT = 2, SAND = 3, CLAY = 4, WOOD = 5, LEAVE = 6 }
[RequireComponent(typeof(VoxelGeneration))]
public class BaseGeneration : MonoBehaviour
{
    public virtual short Generation(int x, int y, int z, byte lod)
    {
        // return y <= 0 ? (short)1 : (short)-1;
        y += 15;

        VoxelType voxel = VoxelType.AIR;


        float surface = 0f;

        // Height data, regardless of biome
        float mountainContrib = NoiseS3D.Noise(x / 150f, z / 150f).Remap(-0.33f, 0.66f, 0, 1) * 40f;
        float desertContrib = 0.3f;
        float oceanContrib = 0.3f;
        float detailContrib = NoiseS3D.Noise(x / 20f, z / 20f) * 5f;

        // Biomes
        float detailMult = NoiseS3D.Noise(x / 30f, z / 30f).Remap(-0.33f, 0.66f, 0, 1);
        float mountainBiome = NoiseS3D.Noise(x / 100f, z / 100f).Remap(-0.33f, 0.66f, 0, 1);
        float Biome = NoiseS3D.Noise(x / 100f, z / 100f).Remap(-0.33f, 0.66f, 0, 1);
        float desertBiome = NoiseS3D.Noise(x / 300f, z / 300f).Remap(-0.33f, 0.66f, 0, 1) * NoiseS3D.Noise(x / 25f, z / 25f).Remap(-0.33f, 0.66f, 0.95f, 1.05f);
        float oceanBiome = NoiseS3D.Noise(x / 500f, z / 500f).Remap(-0.33f, 0.66f, 0, 1);

        // Add biome contrib
        float mountainFinal = (mountainContrib * mountainBiome) + (detailContrib * detailMult) + 20;
        float desertFinal = (desertContrib * desertBiome) + (detailContrib * detailMult) + 20;
        float oceanFinal = (oceanContrib * oceanBiome);

        // Final contrib
        surface = Mathf.Lerp(mountainFinal, desertFinal, desertBiome); // Decide between mountain biome or desert biome
        surface = Mathf.Lerp(surface, oceanFinal, oceanBiome); // Decide between the previous biome or ocean biome (aka ocean biome overrides all biomes)

        surface = Mathf.Floor(surface);

        // Trees!
        float treeTrunk = Mathf.Pow(2, NoiseS3D.Noise(x / 0.3543f, z / 0.3543f)) - 1;
        float treeLeaves = NoiseS3D.Noise(x / 5f, z / 5f) - (1 - treeTrunk) * 0.2f;

        float ndValue = (float)NoiseS3D.Noise(x * 0.2f, y * 0.2f, z * 0.2f);
        // float ndValue2 = (float)NoiseS3D.Noise(x * 5f, y * 5f, z * 5f);
        float caveValue = ndValue - GameManager.Map(y, 35, 0, 0, 1);
        // float skyValue = ndValue2 - GameManager.Map(y, 55, 0, 0, 0.2f);

        //Surface
        if (y <= surface)
        {
            {
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
                else
                    voxel = VoxelType.STONE;
            }
            if (caveValue > 0.65f)
                voxel = VoxelType.AIR;
        }

        // if (y > surface * 4 + 40)
        //     if (skyValue > 0.65f)
        //         voxel = VoxelType.DIRT;

        //trees
        if (lod <= 2)
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


        return (short)voxel;
    }

    public float Perlin3D(float x, float y, float z)
    {
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        float ABC = AB + BC + AC + BA + CB + CA;
        return ABC / 6f;
    }

    public float Perlin3DLight(float x, float y, float z)
    {
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);

        float BA = Mathf.PerlinNoise(y, x);
        float CA = Mathf.PerlinNoise(z, x);

        float ABC = BC + AB + BA + CA;
        return ABC / 4f;
    }
}
