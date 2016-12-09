using System.Collections.Generic;
using System.Dynamic;

namespace Fishworks.ECS.Extensions
{
  public static class DynamicExtensions
  {
    public static void AddProperty(this ExpandoObject dynObject, string propertyName, object value)
    {
      IDictionary<string, object> dynObjectProperties = dynObject;
      dynObjectProperties.Add(propertyName, value);
    }
  }
}
