using System;

namespace Fishworks.ECS
{
  public class EntityEventArgs : EventArgs
  {
    public EntityEventArgs(uint entityId, int entityBitmask)
    {
      EntityId = entityId;
      EntityBitmask = entityBitmask;
    }

    public uint EntityId { get; set; }
    public int EntityBitmask { get; set; }
  }
}
