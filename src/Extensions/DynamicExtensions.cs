using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
