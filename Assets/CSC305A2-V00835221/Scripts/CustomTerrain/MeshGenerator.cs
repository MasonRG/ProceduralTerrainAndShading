using UnityEngine;

namespace CustomTerrain
{
	public static class MeshGenerator
	{
		public static Mesh GenerateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightChangeCurve)
		{
			int width = heightMap.GetLength(0);
			int height = heightMap.GetLength(1);

			float topLeftX = (width - 1) / -2f;
			float topLeftZ = (height - 1) / 2f;

			MyMesh myMesh = new MyMesh(width, height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					//x and z coordinates are shifted to center the mesh origin
					var X = topLeftX + x;
					var Z = topLeftZ - y;

					//y coordinate is the perlin noise value we generated times some scale factor, since the noise is only (0,1)
					var Y = heightMap[x, y] * heightMultiplier * heightChangeCurve.Evaluate(heightMap[x, y]);

					//uv coordinates are simply a (0,1) value indicating relative position within the mesh
					var U = (float)x / width;
					var V = (float)y / height;

					myMesh.AddVertexAndUv(X, Y, Z, U, V);

					//ignore vertices along right and bottom edge as they do not originate triangles
					if (x < width - 1 && y < height - 1)
					{
						/// for current vertex i... we create the triangles to the right and below that i belongs to,
						/// adding indices in clockwise order: [i, i+w+1, i+w], [i+w+1, i, i+1]
						/// -----------------
						///   i __ i+1
						///    |\ |
						/// i+w|_\|i+w+1
						/// ----------------

						var index = y * width + x;
						myMesh.AddTriangle(index, index + width + 1, index + width);
						myMesh.AddTriangle(index + width + 1, index, index + 1);
					}
				}
			}

			//Return our new mesh!
			return myMesh.Create();
		}
	}


	public class MyMesh
	{
		public readonly Vector3[] vertices;
		public readonly Vector2[] uvs;
		public readonly int[] triangles;

		private int vertexIndex;
		private int triangleIndex;


		public MyMesh(int meshWidth, int meshHeight)
		{
			//number of vertices is simply the size of the mesh
			vertices = new Vector3[meshWidth * meshHeight];

			//uv for each vertex
			uvs = new Vector2[meshWidth * meshHeight];

			//number of triangles is the number of cells in the mesh, which is the size when excluding the
			//vertices at the end of rows and at the end of columns
			//then this is multiplied by 6 since each cell is made up of 2 triangles, and each triangle consists
			//of 3 indices into the vertices array.
			triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
		}

		public void AddVertexAndUv(float x, float y, float z, float u, float v)
		{
			vertices[vertexIndex] = new Vector3(x, y, z);
			uvs[vertexIndex] = new Vector2(u, v);
			vertexIndex++;
		}

		public void AddTriangle(int a, int b, int c)
		{
			triangles[triangleIndex + 0] = a;
			triangles[triangleIndex + 1] = b;
			triangles[triangleIndex + 2] = c;

			triangleIndex += 3;
		}

		public Mesh Create()
		{
			//Generate new Unity mesh from current data
			Mesh mesh = new Mesh
			{
				vertices = vertices,
				uv = uvs,
				triangles = triangles
			};

			//Recalculate normals before returning to fix lighting artifacts
			mesh.RecalculateNormals();

			return mesh;
		}
	}
}