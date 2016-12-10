using System.Collections.Generic;
using System.Dynamic;

namespace Fishworks.ECS.Extensions
{
  public static class DynamicExtensions
  {
    /// <summary>
    /// Adds a property to a dynamic ExpandoObject.
    /// </summary>
    /// <param name="dynObject">The ExpandoObject.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value of the property.</param>
    public static void AddProperty(this ExpandoObject dynObject, string propertyName, object value)
    {
      IDictionary<string, object> dynObjectProperties = dynObject;
      dynObjectProperties.Add(propertyName, value);
    }
  }
}
