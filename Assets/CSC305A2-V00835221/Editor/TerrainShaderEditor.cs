using CustomTerrain;
using UnityEditor;

[CustomEditor(typeof(TerrainShader))]
public class TerrainShaderEditor : Editor
{
	public override void OnInspectorGUI()
	{
		//base.OnInspectorGUI();
		TerrainShader shader = (TerrainShader)target;

		if (DrawDefaultInspector())
		{
			if (shader.autoUpdateLighting)
			{
				shader.ApplyLighting();
			}
			if (shader.autoUpdateTerrain)
			{
				shader.ApplyTerrain();
			}
			if (shader.autoUpdateTextures)
			{
				shader.ApplyTextures();
			}
		}
	}
}
