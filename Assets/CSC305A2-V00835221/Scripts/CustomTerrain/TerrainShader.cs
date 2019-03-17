using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTerrain
{
	/// <summary>
	/// Holds data for describing height-based terrain region.
	/// </summary>
	[System.Serializable]
	public class TerrainData
	{
		public string name;
		public bool drawAsColour = true;
		public bool blendEnabled = true;

		public Texture2D texture;
		public float textureScale = 1;
		public Color tint = Color.black;

		[Range(0, 1)]
		public float tintStrength = 0;

		[Range(0, 1)]
		public float startHeight = 0;

		[Range(0, 1)]
		public float blendStrength = 0;
	}

	/// <summary>
	/// Holds data for blinn-phong shading settings.
	/// </summary>
	[System.Serializable]
	public class BlinnPhongData
	{
		public Color lightColor = new Color(1, 1, 0.8f, 1);
		public Color specularColor = new Color(1, 1, 1);

		[Space()]
		public bool ambientEnabled = true;
		public bool diffuseEnabled = true;
		public bool specularEnabled = true;

		[Space()]
		[Range(0, 1)] public float ambientIntensity = 0.1f;
		[Range(0, 1)] public float diffuseIntensity = 1.0f;
		[Range(0, 1)] public float specularIntensity = 0.13f;
		[Range(1, 20)] public int specularShininess = 15;
	}


	/// <summary>
	/// Stores configuration for terrain shading parameters, including lighting, water effects, 
	/// terrain painting, and distance desaturation.
	/// </summary>
	public class TerrainShader : MonoBehaviour
	{
		const int TEXTURE_SIZE = 512;
		const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB565;

		[Header("Auto-Update Options")]
		public bool autoUpdateLighting = true;
		public bool autoUpdateTerrain = true;
		public bool autoUpdateTextures = true;

		[Header("Blinn-Phong Parameters")]
		public Transform lightTransform;
		public BlinnPhongData terrainLighting;
		public BlinnPhongData waterLighting;

		[Header("Water Parameters")]
		public Texture2D waterTexture;
		public float waterTextureScale = 1;
		public Color waterTintColour = Color.white;
		[Range(0f, 1f)] public float waterTintStrength = 0.0f;
		[Range(0f, 0.5f)] public float waterTransparency = 0.25f;
		public float waterMoveSpeed;
		public float waterRippleSpeed;
		[Range(0, 2)] public float waterRippleHeight;

		[Header("Terrain Data")]
		[Range(0, 1)] public float desaturationDistance = 1.0f;
		[Range(0, 1)] public float desaturationStrength = 0.8f;
		public TerrainData[] data;

		public float WaterLevel
		{
			get
			{
				if (data == null || data.Length < 2)
					return 0;

				return data[1].startHeight * TerrainGenerator.Instance.heightMultiplier + waterRippleHeight / 2f;
			}
		}

		public Material TerrainMat
		{
			get { return TerrainGenerator.Instance.Display.customMat; }
		}

		public Material WaterMat
		{
			get { return TerrainGenerator.Instance.Display.waterMat; }
		}



		public void ApplyAll()
		{
			ApplyTerrain();
			ApplyTextures();
			ApplyLighting();
		}

		public void ApplyTerrain()
		{
			//build arrays for shader
			int n = data.Length;
			Color[] colours = new Color[n];
			float[] colourStrengths = new float[n];
			float[] startHeights = new float[n];
			float[] blendStrengths = new float[n];
			float[] textureScales = new float[n];
			for (int i = 0; i < n; i++)
			{

				colours[i] = data[i].tint;
				colourStrengths[i] = data[i].drawAsColour ? 1 : data[i].tintStrength;
				startHeights[i] = data[i].startHeight;
				blendStrengths[i] = !data[i].blendEnabled ? 0 : data[i].blendStrength;
				textureScales[i] = data[i].textureScale;
			}

			TerrainMat.SetInt("_DataCount", n);

			TerrainMat.SetColorArray("_DataColours", colours);
			TerrainMat.SetFloatArray("_DataColourStrengths", colourStrengths);
			TerrainMat.SetFloatArray("_DataStartHeights", startHeights);
			TerrainMat.SetFloatArray("_DataBlendStrengths", blendStrengths);
			TerrainMat.SetFloatArray("_DataTextureScales", textureScales);
			TerrainMat.SetFloat("_MaxHeight", TerrainGenerator.Instance.heightMultiplier);

			ApplyDesaturation();

			ApplyWater();
		}


		/// Applying texture changes is slow - so keep it separate and only do it by request.
		public void ApplyTextures()
		{
			Texture2DArray array = new Texture2DArray(TEXTURE_SIZE, TEXTURE_SIZE, data.Length, TEXTURE_FORMAT, true);
			for (int i = 0; i < data.Length; i++)
			{
				array.SetPixels(data[i].texture.GetPixels(), i);
			}
			array.Apply();

			TerrainMat.SetTexture("_DataTextures", array);
		}

		private void ApplyWater()
		{
			WaterMat.SetTexture("_MainTex", waterTexture);
			WaterMat.SetFloat("_TextureScale", waterTextureScale);
			WaterMat.SetVector("_TintColour", waterTintColour);
			WaterMat.SetFloat("_TintStrength", waterTintStrength);
			WaterMat.SetFloat("_Transparency", waterTransparency);
			WaterMat.SetFloat("_MoveSpeed", waterMoveSpeed);
			WaterMat.SetFloat("_RippleSpeed", waterRippleSpeed);
			WaterMat.SetFloat("_RippleHeight", waterRippleHeight);
		}

		private void ApplyDesaturation()
		{
			var maxDist = Mathf.Max(TerrainGenerator.Instance.mapSize.x, TerrainGenerator.Instance.mapSize.y) * desaturationDistance;

			TerrainMat.SetFloat("_MaxDistance", maxDist);
			TerrainMat.SetFloat("_MaxDesaturation", desaturationStrength);

			var propHandler = FindObjectOfType<PropPopulator>();
			if (propHandler == null)
				return;

			foreach (var prop in propHandler.props)
			{
				foreach (var mat in prop.prefab.GetComponent<MeshRenderer>().sharedMaterials)
				{
					mat.SetFloat("_MaxDistance", maxDist);
					mat.SetFloat("_MaxDesaturation", desaturationStrength);
				}
			}
		}


		public void ApplyLighting()
		{
			ApplyLighting_Internal(TerrainMat, terrainLighting);
			ApplyLighting_Internal(WaterMat, waterLighting);
		}


		private void ApplyLighting_Internal(Material mat, BlinnPhongData data)
		{
			mat.SetVector("_LightDir", -lightTransform.forward);
			mat.SetVector("_LightColor", data.lightColor);
			mat.SetFloat("_Ka", data.ambientEnabled ? data.ambientIntensity : 0);
			mat.SetFloat("_Kd", data.diffuseEnabled ? data.diffuseIntensity : 0);
			mat.SetFloat("_Ks", data.specularEnabled ? data.specularIntensity : 0);
			mat.SetFloat("_Shininess", data.specularShininess);
			mat.SetVector("_SpecColor", data.specularColor);
		}


	}
}