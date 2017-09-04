using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator {

	public static Texture2D TextureFromColorMap(Color[] colormap, int width, int heigth){
		Texture2D texture = new Texture2D (width, heigth);

		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colormap);
		texture.Apply ();
		return texture;
	}

	public static Texture2D TextureFromHeigthMap(float[,] heigthmap){
		int width = heigthmap.GetLength (0);
		int heigth = heigthmap.GetLength (1);

		Color[] colorMap = new Color[width * heigth];
		for (int i = 0; i < heigth; i++) {
			for (int j = 0; j < width; j++) {
				colorMap [i * width + j] = Color.Lerp (Color.black, Color.white, heigthmap [j, i]);
			}
		}

		return TextureFromColorMap (colorMap, width, heigth);
	}
}
