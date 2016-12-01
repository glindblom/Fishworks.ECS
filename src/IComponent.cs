using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS
{
  public interface IComponent
  {
  }

  internal static class ComponentExtensions
  {
    private static Dictionary<Type, int> bitmasks = new Dictionary<Type, int>();
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
