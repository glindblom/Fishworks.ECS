using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Fishworks.ECS
{
  /// <summary>
  /// 
  /// </summary>
  public abstract class BaseSystem
  {
    protected Type[] ComponentsOfInterest;

    protected World World;
    protected int SystemBitmask;
    protected int ExclusionBitmask;
    protected Dictionary<uint, dynamic> Compositions = new Dictionary<uint, dynamic>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="componentTypes"></param>
    /// <param name="componentsToExlcude"></param>
    protected BaseSystem(World world, Type[] componentTypes, Type[] componentsToExlcude = null)
    {
      ComponentsOfInterest = componentTypes;
      World = world;

      foreach (var componentType in componentTypes)
      {
        SystemBitmask += componentType.GetComponentBitmask();
      }

      if (componentsToExlcude != null)
      {
        foreach (var componentType in componentsToExlcude)
        {
          ExclusionBitmask += componentType.GetComponentBitmask();
        }
      }

      world.EntityAdded += OnEntityAdded;
      world.EntityRemoved += OnEntityRemoved;
      world.EntityChanged += OnEntityChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deltaTime"></param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityComposition"></param>
    public abstract void ProcessEntity(dynamic entityComposition);

    /// <summary>
    /// 
    /// </summary>
    public virtual void ProcessEntities()
    {
      foreach (var composition in Compositions)
      {
        ProcessEntity(composition);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public virtual void OnEntityAdded(object sender, EntityEventArgs eventArgs)
    {
      if ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask)
      if ((ExclusionBitmask & eventArgs.EntityBitmask) != ExclusionBitmask)
      {
        Compositions.Add(eventArgs.EntityId, CreateComposition(eventArgs.EntityId));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public virtual void OnEntityRemoved(object sender, EntityEventArgs eventArgs)
    {
      if (Compositions.ContainsKey(eventArgs.EntityId))
      {
        Compositions.Remove(eventArgs.EntityId);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public virtual void OnEntityChanged(object sender, EntityEventArgs eventArgs)
    {
      bool contains = Compositions.ContainsKey(eventArgs.EntityId);
      bool ofInterest = ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask);
      bool exclude = ((ExclusionBitmask & eventArgs.EntityBitmask) == ExclusionBitmask);

      // The entity is registered, and is no longer of interest, remove the composition
      if (contains && !ofInterest)
      {
        Compositions.Remove(eventArgs.EntityId);
      }
      // The entity is registered, and should now be excluded, remove the composition
      else if (contains && exclude)
      {
        Compositions.Remove(eventArgs.EntityId);
      }
      // The entity is not registered, but is of interest and not to be excluded, add it
      else if (!contains && ofInterest && !exclude)
      {
        Compositions.Add(eventArgs.EntityId, CreateComposition(eventArgs.EntityId));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
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
