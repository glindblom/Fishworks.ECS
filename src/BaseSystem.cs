using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS
{
  public abstract class BaseSystem
  {

    protected Type[] ComponentsOfInterest;

    protected World World;
    protected int SystemBitmask;
    protected Dictionary<uint, dynamic> Compositions = new Dictionary<uint, dynamic>();

    protected BaseSystem(World world, params Type[] componentTypes)
    {
      ComponentsOfInterest = componentTypes;
      World = world;
      foreach (var componentType in componentTypes)
      {
        SystemBitmask |= componentType.GetComponentBitmask();
      }

      world.EntityAdded += OnEntityAdded;
      world.EntityRemoved += OnEntityRemoved;
      world.EntityChanged += OnEntityChanged;
    }

    public abstract void Update(float deltaTime);
    public abstract void ProcessEntity(dynamic entityComposition);

    public virtual void ProcessEntities()
    {
      foreach (var composition in Compositions)
      {
        ProcessEntity(composition);
      }
    }

    public virtual void OnEntityAdded(object sender, EntityEventArgs eventArgs)
    {
      if ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask)
      {
        Compositions.Add(eventArgs.EntityId, CreateComposition(eventArgs.EntityId));
      }
    }

    public virtual void OnEntityRemoved(object sender, EntityEventArgs eventArgs)
    {
      if ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask)
      {
        Compositions.Remove(eventArgs.EntityId);
      }
    }

    public virtual void OnEntityChanged(object sender, EntityEventArgs eventArgs)
    {
      bool contains = Compositions.ContainsKey(eventArgs.EntityId);
      bool ofInterest = ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask);

      // The entity is registered, and is no longer of interest, remove the composition
      if (contains && !ofInterest)
      {
        Compositions.Remove(eventArgs.EntityId);
      }
      // The entity is not registered, but is of interest, add it
      else if (!contains && ofInterest)
      {
        Compositions.Add(eventArgs.EntityId, CreateComposition(eventArgs.EntityId));
      }
    }

    private dynamic CreateComposition(uint entityId)
    {
      dynamic composition = new ExpandoObject();
      IDictionary<string, object> compositionProperties = composition;

      compositionProperties.Add("EntityId", entityId);

      foreach (var componentType in ComponentsOfInterest)
      {
        compositionProperties.Add(componentType.Name, World.GetComponent(entityId, componentType));
      }

      return composition;
    }
  }
}
