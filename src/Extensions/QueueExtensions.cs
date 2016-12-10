using System.Collections.Generic;

namespace Fishworks.ECS.Extensions
{
  public static class QueueExtensions
  {
    /// <summary>
    /// Dequeues multiple elements from a generic Queue object.
    /// </summary>
    /// <typeparam name="T">The generic type of Queue</typeparam>
    /// <param name="queue">The Queue</param>
    /// <param name="numberOfObjectsToDequeue">The number of elements to dequeue.</param>
    /// <returns>The elements dequeued as an array.</returns>
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
