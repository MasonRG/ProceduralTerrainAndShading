using UnityEngine;

namespace CustomTerrain.Utilities
{
	/// <summary>
	/// Custom extensions class -- its just nicer to make these types of functions
	/// extensions. 
	/// </summary>
	public static class Extensions
	{

		/// <summary>
		/// Find the min and max values of the given array and normalize all values to be
		/// between 0 and 1.
		/// </summary>
		public static void Normalize(this float[,] arr)
		{
			float min = float.MaxValue;
			float max = float.MinValue;
			//find min and max of the array
			for (int i = 0; i < arr.GetLength(0); i++)
			{
				for (int j = 0; j < arr.GetLength(1); j++)
				{
					var val = arr[i, j];
					if (val < min) min = val;
					if (val > max) max = val;
				}
			}

			//normalize values
			for (int i = 0; i < arr.GetLength(0); i++)
			{
				for (int j = 0; j < arr.GetLength(1); j++)
				{
					arr[i, j] = Mathf.InverseLerp(min, max, arr[i, j]);
				}
			}
		}

		/// <summary>
		/// Convert the given 1D array to a 2D array using the given width and height.
		/// </summary>
		public static T[,] ConvertDimensionsOneToTwo<T>(this T[] array, int width, int height)
		{
			if (array.Length != width * height)
				throw new System.ArgumentException("Array does not have the appropriate number of elements. Must equal width * height.");

			T[,] newArray = new T[width, height];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int uniIndex = y * width + x;
					newArray[x, y] = array[uniIndex];
				}
			}

			return newArray;
		}

		/// <summary>
		/// Check if a value lies between the x and y values of this vector.
		/// </summary>
		public static bool WithinRange(this Vector2 v, float value)
		{
			var min = Mathf.Min(v.x, v.y);
			var max = Mathf.Max(v.x, v.y);
			return value >= min && value <= max;
		}
	}
}