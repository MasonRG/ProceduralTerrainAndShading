using CustomTerrain.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTerrain
{
	/// <summary>
	/// Data container for storing information about props to be
	/// spawned on the terrain.
	/// </summary>
	[System.Serializable]
	public class PropData
	{
		public string name;
		public GameObject prefab; //the prefab to be instantiated
		public Vector2 size; //the size of the prop in the xz plane

		[Range(0, 1f)] public float chance; //the chance to place this prop at any viable cell
		[Range(0, 1)] public float minHeight; //the minimum height at which we can place
		[Range(0, 1)] public float maxHeight; //the maximum height
		[Range(0, 360)] public float minAngle; //the minimum angle slope we will allow placement on
		[Range(0, 360)] public float maxAngle; //the maximum angle slope

		public Vector2Int CellSize
		{
			get { return new Vector2Int(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y)); }
		}

		public Vector2 HeightRange
		{
			get { return new Vector2(minHeight, maxHeight) * TerrainGenerator.Instance.heightMultiplier; }
		}

		public Vector2 AngleRange
		{
			get { return new Vector2(minAngle, maxAngle); }
		}
	}




	/// <summary>
	/// Handler class for managing the instantiation and removal of props from the terrain.
	/// </summary>
	public class PropPopulator : MonoBehaviour
	{
		public int maxPropsAllowed = 500;///the prop limit - spawn no more than this value (save our PCs in case we set the chance to 1 or something)

		public bool drawNormalsDebug = false;
		public PropData[] props;



		public void OnDrawGizmosSelected()
		{
			if (!drawNormalsDebug)
				return;

			Vector3[,] vertexGrid = TerrainMesh.vertices.ConvertDimensionsOneToTwo(TerrainGenerator.Instance.mapSize.x, TerrainGenerator.Instance.mapSize.y);
			Gizmos.color = Color.red;
			for (int y = 1; y < 30; y++)
			{
				for (int x = 1; x < 30; x++)
				{
					Vector3 center = GetCenterPoint(vertexGrid, x, y, 1, 1);
					Vector3 normal = GetNormalVector(vertexGrid, x, y, 1, 1);
					Gizmos.DrawLine(center, center + normal);
				}
			}
		}



		private Transform propContainer;
		public Transform PropContainer
		{
			get
			{
				if (propContainer == null)
				{
					propContainer = transform.Find("PropContainer");
					if (propContainer == null)
					{
						propContainer = new GameObject("PropContainer").transform;
						propContainer.transform.parent = transform;
					}
				}
				return propContainer;
			}
		}

		//convenience accessors to hide the singleton instance access
		private Mesh TerrainMesh { get { return TerrainGenerator.Instance.Display.meshFilter.sharedMesh; } }
		private TerrainData[] TerrainData { get { return TerrainGenerator.Instance.Shader.data; } }


		public void PlaceProps()
		{
			int mapWidth = TerrainGenerator.Instance.mapSize.x;
			int mapHeight = TerrainGenerator.Instance.mapSize.y;

			int propCount = PropContainer.childCount;
			bool[,] occupancy = new bool[(mapWidth - 1), (mapHeight - 1)];
			Vector3[,] vertexGrid = TerrainMesh.vertices.ConvertDimensionsOneToTwo(mapWidth, mapHeight);

			for (int y = 1; y < mapHeight - 1; y++)
			{
				//Exceeded the prop limit
				if (propCount > maxPropsAllowed)
					break;

				for (int x = 1; x < mapWidth - 1; x++)
				{
					//Exceeded the prop limit
					if (propCount > maxPropsAllowed)
						break;

					//this cell is already occupied by a prop -> skip it
					if (occupancy[x, y])
						continue;

					//use a random index to start at so we dont bias the front of the array
					int propsVisited = 0;
					int randIndex = Random.Range(0, props.Length);

					while (propsVisited < props.Length)
					{
						var prop = props[randIndex];

						//Chance hit --> spawn the prop
						if (Random.value < prop.chance)
						{
							var cellSize = prop.CellSize;

							//validate that the cell will fit on the remaining space (dont hang over the edges of the map)
							if (x + cellSize.x < mapWidth && y + cellSize.y < mapHeight)
							{
								Vector3 center = GetCenterPoint(vertexGrid, x, y, cellSize.x, cellSize.y);

								//validate the height range
								if (prop.HeightRange.WithinRange(center.y))
								{
									//the surface normal at the center point
									Vector3 normal = GetNormalVector(vertexGrid, x, y, cellSize.x, cellSize.y);

									//validate angle range
									if (prop.AngleRange.WithinRange(Vector3.Angle(normal, Vector3.up)))
									{
										//spawn the object in the center of the space and orient it with the mesh normal at the center
										SpawnProp(prop.prefab, center, normal, propCount);

										MarkOccupany(occupancy, x, y, cellSize.x, cellSize.y);
										propCount++;

										//prop spawned -> exit the prop loop
										break;
									}
								}
							}
						}

						//increment index, wrap when needed
						randIndex++;
						if (randIndex == props.Length)
							randIndex = 0;

						propsVisited++;
					}
				}
			}
		}

		private GameObject SpawnProp(GameObject prefab, Vector3 center, Vector3 normal, int propCount)
		{
			// 1) Instantiation 
			var obj = Instantiate(prefab, PropContainer);
			obj.name = prefab.name + "_" + propCount;

			// 2) Positioning
			obj.transform.position = center;

			// 3) Orientation
			obj.transform.up = normal;

			return obj;
		}



		public void ClearProps()
		{
			if (Application.isPlaying)
			{
				Destroy(PropContainer.gameObject);
			}
			else
			{
				DestroyImmediate(PropContainer.gameObject);
			}
		}



		#region Helpers
		private void MarkOccupany(bool[,] arr, int x, int y, int sizeX, int sizeY)
		{
			for (int cy = 0; cy < sizeY; cy++)
			{
				for (int cx = 0; cx < sizeX; cx++)
				{
					arr[x + cx, y + cy] = true;
				}
			}
		}


		private Vector3 GetCenterPoint(Vector3[,] vertexGrid, int x, int y, int sizeX, int sizeY)
		{
			//corner positions of the cell block we will be centered at
			Vector3 bl = vertexGrid[x, y];
			Vector3 br = vertexGrid[x + sizeX, y];
			Vector3 tl = vertexGrid[x, y + sizeY];
			Vector3 tr = vertexGrid[x + sizeX, y + sizeY];

			//the center point of the cell block
			return BiLerpToCenter(bl, br, tl, tr);
		}


		private Vector3 GetNormalVector(Vector3[,] vertexGrid, int x, int y, int sizeX, int sizeY)
		{
			int cellX = x + sizeX / 2;
			int cellY = y + sizeY / 2;

			Vector3 C = vertexGrid[cellX, cellY];
			Vector3 L = vertexGrid[cellX - 1, cellY];
			Vector3 R = vertexGrid[cellX + 1, cellY];
			Vector3 A = vertexGrid[cellX, cellY + 1];
			Vector3 B = vertexGrid[cellX, cellY - 1];

			Vector3 n_AL = GetNormal(C, A, L);
			Vector3 n_LB = GetNormal(C, L, B);
			Vector3 n_BR = GetNormal(C, B, R);
			Vector3 n_RA = GetNormal(C, R, A);

			//the norm of the center point is the average of the norms of the 4 corners of the cell
			//return BiLerpToCenter(norm_bl, norm_br, norm_tl, norm_tr);
			return BiLerpToCenter(n_AL, n_LB, n_BR, n_RA);
		}

		private Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
		{
			return Vector3.Cross(b - a, c - a).normalized;
		}


		private Vector3 BiLerpToCenter(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight)
		{
			return Vector3.Lerp(
				Vector3.Lerp(bottomLeft, bottomRight, 0.5f),
				Vector3.Lerp(topLeft, topRight, 0.5f),
				0.5f);
		}
		#endregion


		private void OnValidate()
		{
			foreach (var prop in props)
			{
				if (prop.size.x < 0.01f) prop.size.x = 0.01f;
				if (prop.size.y < 0.01f) prop.size.y = 0.01f;
			}
		}
	}
}