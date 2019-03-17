using CustomTerrain.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTerrain
{
	/// <summary>
	/// The main component of the CustomTerrain. Defines map, noise, and mesh parameters and controls
	/// for generating content in editor.
	/// </summary>
	[RequireComponent(typeof(TerrainDisplay))]
	[RequireComponent(typeof(TerrainShader))]
	public class TerrainGenerator : MonoBehaviour
	{
		#region Lazy Accessors
		private static TerrainGenerator instance;
		public static TerrainGenerator Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<TerrainGenerator>();
				return instance;
			}
		}

		private TerrainDisplay display;
		public TerrainDisplay Display
		{
			get
			{
				if (display == null)
					display = FindObjectOfType<TerrainDisplay>();
				return display;
			}
		}

		private TerrainShader shader;
		public TerrainShader Shader
		{
			get
			{
				if (shader == null)
					shader = FindObjectOfType<TerrainShader>();
				return shader;
			}
		}
		#endregion


		public enum NoiseType { White, UnityPerlin, MyPerlin }

		[Header("Main Properties")]
		public Vector2Int mapSize = new Vector2Int(250, 250);
		public bool autoUpdate = true;

		[Header("Noise Properties")]
		public NoiseType noiseType = NoiseType.White;
		public NoiseSettings noiseParams;

		[Header("Mesh Properties")]
		public Color meshDisplayColor = new Color(0.85f, 0.92f, 1.0f);
		public float heightMultiplier = 1f;
		public AnimationCurve heightChangeCurve = AnimationCurve.Linear(0, 0, 1, 1);



		public void Start()
		{
			PlaceProps();
		}



		private bool showingNoise;
		public void Redraw()
		{
			UnityEditor.EditorApplication.update -= Redraw;

			if (showingNoise) DrawNoise();
			else DrawMesh();
		}

		public float[,] GetHeightMap()
		{
			if (noiseType == NoiseType.White) return Noise.GenerateWhiteNoiseMap(mapSize.x, mapSize.y);
			else return Noise.GenerateNoiseMap(mapSize, noiseParams, noiseType == NoiseType.UnityPerlin);
		}

		public void DrawNoise()
		{
			showingNoise = true;
			Display.DrawNoiseMap(GetHeightMap());
		}

		public void DrawMesh()
		{
			showingNoise = false;
			Display.DrawMesh(MeshGenerator.GenerateMesh(GetHeightMap(), heightMultiplier, heightChangeCurve), mapSize.x, mapSize.y, meshDisplayColor);

			Shader.ApplyAll();
		}


		// -----------------------
		// ---- Terrain Props ----
		// -----------------------
		public void PlaceProps()
		{
			DrawMesh();
			FindObjectOfType<PropPopulator>().PlaceProps();
		}


		public void ClearProps()
		{
			FindObjectOfType<PropPopulator>().ClearProps();
		}


		private void OnValidate()
		{
			if (mapSize.x < 1) mapSize.x = 1;
			if (mapSize.y < 1) mapSize.y = 1;
			if (noiseParams.scale < 1) noiseParams.scale = 1;

			UnityEditor.EditorApplication.update += Redraw;
		}
	}
}