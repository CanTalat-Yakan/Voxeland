using UnityEngine;

namespace VoxelEngine
{
	public class TerrainGeneratorSIMD_Caves : TerrainGeneratorSIMD
	{
		public float caveRatio = .88f;
		
		public Color32 stoneMinColor = new Color32(150, 150, 150, 255);
		public Color32 stoneMaxColor = new Color32(100, 100, 100, 255);

		public override void Awake()
		{
			SetInterpBitStep(2);
			SetNoiseArraySize(1);
		}

		public override void GenerateChunk(Chunk chunk)
		{
			Voxel[] voxelData = chunk.voxelData;

			int index = 0;

			float[] caveNoise = GetInterpNoise(0, chunk.chunkPos);

			for (int x = 0; x < interpSize; x++)
			{
				for (int y = 0; y < interpSize; y++)
				{
					for (int z = 0; z < interpSize; z++)
					{
						caveNoise[index] = (caveRatio - caveNoise[index]) * 32f;

						index++;
					}
				}
			}

			index = 0;

			for (int x = 0; x < Chunk.SIZE; x++)
			{
				for (int y = 0; y < Chunk.SIZE; y++)
				{
					for (int z = 0; z < Chunk.SIZE; z++)
					{
						ChunkFillUpdate(chunk, voxelData[index++] = new Voxel(VoxelInterpLookup(x, y, z, caveNoise)));
					}
				}
			}
		}

		public override Color32 DensityColor(Voxel voxel)
		{
			return Color32.Lerp(stoneMinColor, stoneMaxColor, voxel.density);
		}
	}
}