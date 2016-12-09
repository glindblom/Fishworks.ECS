using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS.Extensions
{
  public static class QueueExtensions
  {
    public static T[] Dequeue<T>(this Queue<T> queue, int numberOfObjectsToDequeue)
    {
      T[] result = new T[numberOfObjectsToDequeue];

      for (int i = 0; i < numberOfObjectsToDequeue; i++)
      {
        result[i] = queue.Dequeue();
      }

      return result;
    }
  }
}
