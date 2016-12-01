using System;
using System.Collections.Generic;

namespace Fishworks.ECS
{
  public interface IComponent
  {
  }

  internal static class ComponentExtensions
  {
    private static readonly Dictionary<Type, int> Bitmasks = new Dictionary<Type, int>();
    private static int _componentIndex = 1;

    internal static int GetComponentBitmask(this Type type)
    {
      if (Bitmasks.ContainsKey(type))
        return Bitmasks[type];

      int bitmask = 1 << _componentIndex;
      Bitmasks.Add(type, bitmask);

      _componentIndex++;
      return bitmask;
    }
  }
}
