namespace Fishworks.ECS.Extensions
{
  internal static class ArrayExtensions
  {
    /// <summary>
    /// Returns an entire column of a two-dimensional array.
    /// </summary>
    /// <typeparam name="T">The Type of array to work with.</typeparam>
    /// <param name="array">The two-dimensional array.</param>
    /// <param name="columnIndex">The index of the column to return.</param>
    /// <returns>The column as an one-dimensional array.</returns>
    public static T[] GetColumn<T>(this T[,] array, int columnIndex)
    {
      int length = array.GetLength(1);
      T[] result = new T[length];

      for (int i = 0; i < length; i++)
      {
        result[i] = array[i, columnIndex];
      }

      return result;
    }

    /// <summary>
    /// Returns an entire row of a two-dimensional array.
    /// </summary>
    /// <typeparam name="T">The Type of array to work with.</typeparam>
    /// <param name="array">The two-dimensional array.</param>
    /// <param name="columnIndex">The index of the row to return.</param>
    /// <returns>The row as an one-dimensional array.</returns>
    public static T[] GetRow<T>(this T[,] array, int rowIndex)
    {
      int length = array.GetLength(0);
      T[] result = new T[length];

      for (int i = 0; i < length; i++)
      {
        result[i] = array[rowIndex, i];
      }

      return result;
    }
  }
}
