using System;

namespace Fishworks.ECS
{
  /// <summary>
  /// Event args used when entities are added, changed, or removed from the world.
  /// </summary>
  public class EntityEventArgs : EventArgs
  {
    /// <summary>
    /// Initializes a new instance of the EntityEventArgs class.
    /// </summary>
    /// <param name="entityId">The ID of the entity that has been added/changed/removed.</param>
    /// <param name="entityBitmask">The bitmask of the added/changed/removed entity</param>
    public EntityEventArgs(uint entityId, int entityBitmask)
    {
      EntityId = entityId;
      EntityBitmask = entityBitmask;
    }

    /// <summary>
    /// The ID of an entity in the world.
    /// </summary>
    public uint EntityId { get; set; }

    /// <summary>
    /// The bitmask of an entity in the world.
    /// </summary>
    public int EntityBitmask { get; set; }
  }
}
