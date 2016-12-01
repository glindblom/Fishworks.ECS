using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS
{
  /// <summary>
  /// Container class that just represents an Id for the entity world to use.
  /// Has implementation for basic functionality, that only calls its world and
  /// then returns itself to enable chaining.
  /// </summary>
  public class Entity
  {
    /// <summary>
    /// The entity's unique id
    /// </summary>
    public readonly uint Id;
    private readonly World _world;

    internal Entity(uint id, World world)
    {
      Id = id;
      _world = world;
    }

    public Entity AddComponent(IComponent component)
    {
      _world.AddComponent(Id, component);
      return this;
    }

    public Entity AddComponent<T>() where T : IComponent, new ()
    {
      _world.AddComponent<T>(Id);
      return this;
    }

    public Entity RemoveComponent(IComponent component)
    {
      _world.RemoveComponent(Id, component);
      return this;
    }

    public Entity RemoveComponent<T>() where T : IComponent, new ()
    {
      _world.RemoveComponent<T>(Id);
      return this;
    }

    public Entity AddToWorld()
    {
      _world.AddEntityToWorld(Id);
      return this;
    }

    public void Destroy()
    {
      _world.DestroyEntity(Id);
    }
  }
}
