using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTerrain
{
	/// <summary>
	/// Stores references to materials and renderers needed for generating
	/// texture and mesh data to visualize in editor, and also for providing
	/// access to materials for setting shader parameters.
	/// </summary>
	public class TerrainDisplay : MonoBehaviour
	{
		public Renderer noiseRenderer;

		public MeshFilter meshFilter;
		public MeshRenderer meshRenderer;

		public bool useCustomShader = false;
		public Material standardMat;
		public Material customMat;
		public Material waterMat;

		private GameObject waterPlane;
		public GameObject WaterPlane
		{
			get
			{
				if (waterPlane == null)
				{
					var t = meshFilter.transform.Find("WaterPlane");
					waterPlane = t != null ? t.gameObject : null;
					if (waterPlane == null)
					{
						waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
						waterPlane.name = "WaterPlane";
						waterPlane.transform.parent = meshFilter.transform;
						waterPlane.GetComponent<MeshCollider>().enabled = false;
						waterPlane.GetComponent<MeshRenderer>().material = waterMat;
					}
				}
				return waterPlane;
			}
		}


		private void SetVisibility(bool forNoise)
		{
			noiseRenderer.gameObject.SetActive(forNoise);
			meshRenderer.gameObject.SetActive(!forNoise);
			WaterPlane.SetActive(!forNoise);
		}

		public void DrawNoiseMap(float[,] noiseMap)
		{
			int width = noiseMap.GetLength(0);
			int height = noiseMap.GetLength(1);
			Texture2D texture = new Texture2D(width, height);

			//iterate through the noisemap and use the (0,1) values to generate a black-white interpolation color array
			Color[] colorMap = new Color[width * height];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
				}
			}

			//apply the color visualization of the height map to the texture
			texture.SetPixels(colorMap);
			texture.Apply();

			//set the height of the display plane to match the noisemap size (divide by 10 because of plane's internal scaling)
			noiseRenderer.transform.localScale = new Vector3(width / 10f, 1, height / 10f);

			//set the material of the test renderer to the new height map texture 
			noiseRenderer.sharedMaterial.mainTexture = texture;

			SetVisibility(true);

		}

		public void DrawMesh(Mesh mesh, int width, int height, Color color)
		{
			meshFilter.sharedMesh = mesh;
			meshRenderer.sharedMaterial = useCustomShader ? customMat : standardMat;

			SetVisibility(false);

			if (!useCustomShader)
			{
				//simple, flat coloring for the standard reference shader
				Texture2D texture = new Texture2D(width, height);
				Color[] colorMap = new Color[width * height];
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						colorMap[y * width + x] = color;
					}
				}
				texture.SetPixels(colorMap);
				texture.Apply();

				meshRenderer.sharedMaterial.mainTexture = texture;

				WaterPlane.gameObject.SetActive(false);
			}
			else
			{
				WaterPlane.transform.position = new Vector3(0f, TerrainGenerator.Instance.Shader.WaterLevel, 0f);
				WaterPlane.transform.localScale = new Vector3(width / 10f, 1, height / 10f); //divide by 10 to compensate for plane primitive scale
			}
		}
	}
}