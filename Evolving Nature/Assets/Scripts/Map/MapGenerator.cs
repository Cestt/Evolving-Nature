﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode{ NoiseMap, ColorMap, Mesh }
	public DrawMode drawMode;

	public const int mapChunkSize = 241;
	[Range(0,6)]
	public int levelOfDetail;
	public float noiseScale;

	public int octaves;
	[Range (0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public float meshHeigthMultiplier;
	public AnimationCurve meshHeigthCurve;

	public bool autoUpdate;

	public TerrainType[] regions;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	public void DrawMapInEditor(){
		MapData mapData = GenerateMapData ();
		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture (TextureGenerator.TextureFromHeigthMap (mapData.heightMap));
		} else if (drawMode == DrawMode.ColorMap) {
			display.DrawTexture (TextureGenerator.TextureFromColorMap (mapData.colorMap, mapChunkSize, mapChunkSize));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeigthMultiplier, meshHeigthCurve, levelOfDetail),TextureGenerator.TextureFromColorMap (mapData.colorMap, mapChunkSize, mapChunkSize)) ;
		}
	}

	public void RequestMapData(Action<MapData> callback){
		ThreadStart threadStart = delegate {
			MapDataThread (callback);
		};

		new Thread (threadStart).Start ();
	}

	void MapDataThread(Action<MapData> callback){//External Thread
		MapData mapData = GenerateMapData ();
		lock (mapDataThreadInfoQueue) {
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
		}

	}

	public void RequestMeshData(MapData mapData, Action<MeshData> callback){
		ThreadStart threadStart = delegate {
			MeshDataThread(mapData, callback);
		};

		new Thread (threadStart).Start ();
	}

	void MeshDataThread(MapData mapData, Action<MeshData> callback){//External Thread
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeigthMultiplier,meshHeigthCurve, levelOfDetail);
		lock (meshDataThreadInfoQueue) {
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}

	}

	void Update(){
		if (mapDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0) {
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue ();
				threadInfo.callback (threadInfo.parameter);
			}
		}
	}

	 MapData GenerateMapData(){
		float[,] noiseMap = Noise.GenerateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
		Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

		for (int y = 0; y < mapChunkSize; y++) {
			for (int x = 0; x < mapChunkSize; x++) {
				float currentHeigth = noiseMap [x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeigth <= regions [i].heigth) {
						colorMap [y * mapChunkSize + x] = regions [i].color;
						break;
					}
				}
			}
		}

		return new MapData (noiseMap, colorMap);
	}

	void OnValidate(){
		if (lacunarity < 1)
			lacunarity = 1;
		if (octaves < 0)
			octaves = 0;
	}

	struct MapThreadInfo<T> {
		
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo (Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
		
	}
		
}

[System.Serializable]
public struct TerrainType{

	public string name;
	public float heigth;
	public Color color;
}

public struct MapData {
	public readonly float[,] heightMap;
	public readonly Color[] colorMap;

	public MapData(float[,] heightMap, Color[] colorMap){
		this.heightMap = heightMap;
		this.colorMap = colorMap;
	}
}
