using System;
using System.Collections.Generic;

namespace Fishworks.ECS.Extensions
{
  internal static class ComponentExtensions
  {
    private static readonly Dictionary<Type, int> bitmasks = new Dictionary<Type, int>();
    private static int componentIndex = 1;

    internal static int GetComponentBitmask(this Type type)
    {
      if (bitmasks.ContainsKey(type))
        return bitmasks[type];

      int bitmask = 1 << componentIndex;
      bitmasks.Add(type, bitmask);

      componentIndex++;
      return bitmask;
    }
  }
}
