using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTerrain.Utilities
{
	[ExecuteInEditMode]
	public class MyDirectionalLight : MonoBehaviour
	{
		public TerrainShader shaderScript;

		private Vector3 lastEuler = Vector3.zero;


		void Update()
		{
			if (transform.eulerAngles != lastEuler)
			{
				shaderScript.ApplyLighting();
				lastEuler = transform.eulerAngles;
			}

		}
	}
}