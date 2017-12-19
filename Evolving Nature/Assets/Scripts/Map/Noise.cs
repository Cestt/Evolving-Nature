using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise {

	public enum NormalizeMode {Local, Global};

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeigth, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode){

		float[,] noisemap = new float[mapWidth, mapHeigth];

		System.Random prng = new System.Random (seed);
		Vector2[] octaveOffSets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;// More frequency more changes in the values(terrain)

		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next (-100000, 100000) + offset.x;
			float offsetY = prng.Next (-100000, 100000) - offset.y;
			octaveOffSets [i] = new Vector2 (offsetX, offsetY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		if (scale <= 0)
			scale = 0.0001f;

		float maxLocalNoiseHeigth = float.MinValue;
		float minLocalNoiseHeight = float.MaxValue;

		float halfWidth = mapWidth / 2f;
		float halfHeigth = mapHeigth / 2f;
		
		for (int y = 0; y < mapHeigth; y++) {
			for (int x = 0; x < mapWidth; x++) {

				amplitude = 1;
				frequency = 1;// More frequency more changes in the values(terrain)
				float noiseHeigth = 0;

				for (int i = 0; i < octaves; i++) {
					float sampleX = (x - halfWidth + octaveOffSets[i].x) / scale * frequency ;
					float sampleY = (y - halfHeigth + octaveOffSets[i].y) / scale * frequency ;

					float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;// Values between 0 and 1 we multiply by 2 and sustract 1 to make values between -1  and 1;
					noiseHeigth += perlinValue * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}
				if (noiseHeigth > maxLocalNoiseHeigth) {
					maxLocalNoiseHeigth = noiseHeigth;
				} else if (noiseHeigth < minLocalNoiseHeight) {
					minLocalNoiseHeight = noiseHeigth;
				}

				noisemap [x, y] = noiseHeigth;

			}
		}

		for (int y = 0; y < mapHeigth; y++) {
			for (int x = 0; x < mapWidth; x++) {
				if (normalizeMode == NormalizeMode.Local) {
					noisemap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeigth, noisemap [x, y]);
				} else {
					float normalizeHeigth = (noisemap [x, y] + 1) / (maxPossibleHeight / 1.5f);
					noisemap [x, y] = Mathf.Clamp(normalizeHeigth, 0, int.MaxValue);
				}

			}
		}

		return noisemap;
	}
}
