using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS.Extensions
{
  internal static class ArrayExtensions
  {
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
  }
}
