using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeigth, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset){

		float[,] noisemap = new float[mapWidth, mapHeigth];

		System.Random prng = new System.Random (seed);
		Vector2[] octaveOffSets = new Vector2[octaves];

		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) + offset.y;
			octaveOffSets [i] = new Vector2 (offsetX, offsetY);
		}

		if (scale <= 0)
			scale = 0.0001f;

		float maxNoiseHeigth = float.MinValue;
		float minNoiseHeigth = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeigth = mapHeigth / 2f;
		
		for (int y = 0; y < mapHeigth; y++) {
			for (int x = 0; x < mapWidth; x++) {

				float amplitude = 1;
				float frequency = 1;// More frequency more changes in the values(terrain)
				float noiseHeigth = 0;

				for (int i = 0; i < octaves; i++) {
					float sampleX = (x - halfWidth) / scale * frequency + octaveOffSets[i].x;
					float sampleY = (y - halfHeigth) / scale * frequency + octaveOffSets[i].y;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;// Values between 0 and 1 we multiply by 2 and sustract 1 to make values between -1  and 1;
					noiseHeigth += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}
				if (noiseHeigth > maxNoiseHeigth) {
					maxNoiseHeigth = noiseHeigth;
				} else if (noiseHeigth < minNoiseHeigth) {
					minNoiseHeigth = noiseHeigth;
				}

				noisemap [x, y] = noiseHeigth;

			}
		}

		for (int y = 0; y < mapHeigth; y++) {
			for (int x = 0; x < mapWidth; x++) {
				noisemap [x, y] = Mathf.InverseLerp (minNoiseHeigth, maxNoiseHeigth, noisemap [x, y]);
			}
		}

		return noisemap;
	}
}
