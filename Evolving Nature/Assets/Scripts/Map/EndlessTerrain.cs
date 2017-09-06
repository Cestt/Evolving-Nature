using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

	public const float maxViewDst = 450;
	public Transform viewer;
	public Material mapMaterial;

	public static Vector2 viewerPosition;
	static MapGenerator mapGeneretor;
	int chunkSize;
	int chunkVisibleInViewDst;

	Dictionary<Vector2,TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start(){
		mapGeneretor = FindObjectOfType<MapGenerator> ();
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunkVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
	}

	void Update(){
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);
		UpdateVisibleChunks ();
	}

	void UpdateVisibleChunks(){

		foreach (TerrainChunk tempTC in terrainChunksVisibleLastUpdate) {
			tempTC.SetVisible (false);
		}
		terrainChunksVisibleLastUpdate.Clear ();

		int currentChunkCoordX = Mathf.RoundToInt (viewer.position.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewer.position.z / chunkSize);

		for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
					if (terrainChunkDictionary [viewedChunkCoord].IsVisible ()) {
						terrainChunksVisibleLastUpdate.Add (terrainChunkDictionary [viewedChunkCoord]);
					}
				} else {
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, transform, mapMaterial));
				}
			}
		}
	}

	public class TerrainChunk{

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;

		MapData mapData;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		public TerrainChunk(Vector2 coord, int size, Transform parent, Material material){
			position= coord * size;
			bounds = new Bounds(position, Vector2.one *size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObject = new GameObject("Terrain Chunk");
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;
			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			SetVisible(false);

			mapGeneretor.RequestMapData(OnMapDataReceived);
		}

		void OnMapDataReceived(MapData mapData){
			mapGeneretor.RequestMeshData (mapData, OnMeshDataReceived);
		}

		void OnMeshDataReceived(MeshData meshData){
			meshFilter.mesh = meshData.CreateMesh ();
		}

		public void UpdateTerrainChunk(){
			float viewerDistanceFromNearestEdge =  Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
			bool visible = viewerDistanceFromNearestEdge <= maxViewDst;
			SetVisible (visible);
		}

		public void SetVisible(bool visible){
			meshObject.SetActive (visible);
		}

		public bool IsVisible(){
			return meshObject.activeSelf;
		}
	}
}
