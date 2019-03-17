using UnityEngine;

namespace CustomTerrain.Utilities
{
	[System.Serializable]
	public class NoiseSettings
	{
		public int seed = 0;
		public float scale = 30;
		[Range(0, 10)] public int octaves = 4;
		[Range(0, 1)] public float persistance = 0.5f;
		[Range(1, 8)] public float lacunarity = 2;
		public Vector2 offset;
	}



	public static class Noise
	{
		public static float[,] GenerateWhiteNoiseMap(int width, int height)
		{
			float[,] noiseMap = new float[width, height];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					noiseMap[x, y] = Random.value;
				}
			}

			noiseMap.Normalize();
			return noiseMap;
		}

		public static float[,] GenerateNoiseMap(Vector2Int mapSize, NoiseSettings settings, bool useUnityPerlin = false)
		{
			var rand = new System.Random(settings.seed);
			Vector2[] offsets = new Vector2[settings.octaves];
			for (int i = 0; i < settings.octaves; i++)
			{
				offsets[i] = new Vector2(rand.Next(-10000, 10000) + settings.offset.x, rand.Next(-10000, 10000) + settings.offset.y);
			}

			float[,] noiseMap = new float[mapSize.x, mapSize.y];
			float halfW = mapSize.x / 2f;
			float halfH = mapSize.y / 2f;

			for (int y = 0; y < mapSize.y; y++)
			{
				for (int x = 0; x < mapSize.x; x++)
				{
					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;

					for (int i = 0; i < settings.octaves; i++)
					{
						float sampleX = (x - halfW) / settings.scale * frequency + offsets[i].x;
						float sampleY = (y - halfH) / settings.scale * frequency + offsets[i].y;

						//allow the option of using unity's perlin noise so that we can easily compare our implementation
						float value = useUnityPerlin ? Mathf.PerlinNoise(sampleX, sampleY) : MyPerlin.Noise(sampleX, sampleY);

						noiseHeight += value * amplitude;

						amplitude *= settings.persistance;
						frequency *= settings.lacunarity;
					}

					noiseMap[x, y] = noiseHeight;
				}
			}

			noiseMap.Normalize();
			return noiseMap;
		}
	}
}