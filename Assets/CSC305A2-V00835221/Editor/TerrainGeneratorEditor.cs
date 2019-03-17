using UnityEngine;
using UnityEditor;
using CustomTerrain;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		//base.OnInspectorGUI();
		TerrainGenerator gen = (TerrainGenerator)target;

		if (DrawDefaultInspector())
		{
			if (gen.autoUpdate)
				gen.Redraw();
		}

		GUILayout.Space(20);
		GUILayout.Label("Height Map & Mesh");
		if (GUILayout.Button("Display Noise"))
		{
			gen.DrawNoise();
		}
		if (GUILayout.Button("Generate Mesh"))
		{
			gen.DrawMesh();
		}

		GUILayout.Label("Props");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Place Props"))
		{
			gen.DrawMesh();
			gen.PlaceProps();
		}
		if (GUILayout.Button("Clear Props"))
		{
			gen.ClearProps();
		}
		GUILayout.EndHorizontal();


		GUILayout.Label("Shader");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Apply Lighting"))
		{
			gen.Shader.ApplyLighting();
		}
		if (GUILayout.Button("Apply Terrain"))
		{
			gen.Shader.ApplyTerrain();
		}
		if (GUILayout.Button("Apply Textures"))
		{
			gen.Shader.ApplyTextures();
		}
		GUILayout.EndHorizontal();
	}
}
