using System;
using System.Collections.Generic;
using System.Dynamic;
using Fishworks.ECS.Extensions;

namespace Fishworks.ECS
{
  /// <summary>
  /// Abstract base class for Systems. Meant to be extended in implementations.
  /// </summary>
  public abstract class BaseSystem
  {
    /// <summary>
    /// The types of components the system is interested in.
    /// </summary>
    protected Type[] ComponentsOfInterest;

    /// <summary>
    /// The world that the system belongs to.
    /// </summary>
    protected World World;

    /// <summary>
    /// The bitmask of the system, combined from the <see cref="ComponentsOfInterest"/> field. ANy entity that matches this bitmask will be added to the system.
    /// </summary>
    protected int SystemBitmask;

    /// <summary>
    /// The exclusion bitmask of the system. Any entity that matches this bitmask will not be added to the system. Or be removed from it if already added.
    /// </summary>
    protected int ExclusionBitmask;

    /// <summary>
    /// The compositions of the system, created from added entities and their components.
    /// </summary>
    protected Dictionary<uint, dynamic> Compositions = new Dictionary<uint, dynamic>();

    private readonly List<uint> entitiesToRemove = new List<uint>();
    private readonly List<dynamic> entitiesToAdd = new List<dynamic>();

    private bool processing;

    /// <summary>
    /// Initializes a new instance of the BaseSystem class.
    /// </summary>
    /// <param name="world">The world the system belongs to.</param>
    /// <param name="componentsOfInterest">The component composition the system is interested in.</param>
    /// <param name="componentsToExclude">The component composition the system will exclude.</param>
    protected BaseSystem(World world, Type[] componentsOfInterest, Type[] componentsToExclude = null)
    {
      ComponentsOfInterest = componentsOfInterest;
      World = world;

      foreach (var componentType in componentsOfInterest)
      {
        SystemBitmask += componentType.GetComponentBitmask();
      }

      if (componentsToExclude != null)
      {
        foreach (var componentType in componentsToExclude)
        {
          ExclusionBitmask += componentType.GetComponentBitmask();
        }
      }

      world.EntityAdded += OnEntityAdded;
      world.EntityRemoved += OnEntityRemoved;
      world.EntityChanged += OnEntityChanged;

      GetEntitiesFromWorld();
    }

    private void GetEntitiesFromWorld()
    {
      uint[] entityIds = World.GetEntitiesMatchingBitmask(SystemBitmask);
      foreach (uint entityId in entityIds)
      {
        Compositions.Add(entityId, CreateComposition(entityId));
      }
    }

    /// <summary>
    /// Abstract method that will be called by the world each update cycle.
    /// </summary>
    /// <param name="deltaTime">The delta time (time between frames) of the simulation.</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Called by the world each update cycle. Enumerates the composition list and calls <see cref="ProcessEntity(dynamic)"/>.
    /// </summary>
    public virtual void ProcessEntities()
    {
      processing = true;
      foreach (var composition in Compositions.Values)
      {
        ProcessEntity(composition);
      }
      processing = false;

      if (entitiesToRemove.Count > 0 || entitiesToAdd.Count > 0)
        UpdateLists();
    }

    private void UpdateLists()
    {
      foreach (uint entityId in entitiesToRemove)
        Compositions.Remove(entityId);

      foreach (dynamic entityComposition in entitiesToAdd)
        Compositions.Add(entityComposition.EntityId, entityComposition);

      entitiesToRemove.Clear();
      entitiesToAdd.Clear();
    }

    /// <summary>
    /// Abstract method that will be called for each composition in the system, each update cycle.
    /// </summary>
    /// <param name="entityComposition">The composition to handle.</param>
    public abstract void ProcessEntity(dynamic entityComposition);

    /// <summary>
    /// Virtual method invoked by the world when an entity has been added to the world.
    /// </summary>
    /// <param name="sender">The world.</param>
    /// <param name="eventArgs">The <see cref="EntityEventArgs"/> object containing the entity's ID and bitmask.</param>
    public virtual void OnEntityAdded(object sender, EntityEventArgs eventArgs)
    {
      if ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask)
      {
        if (processing)
        {
          entitiesToAdd.Add(CreateComposition(eventArgs.EntityId));
          return;
        }
        Compositions.Add(eventArgs.EntityId, CreateComposition(eventArgs.EntityId));
      }
    }

    /// <summary>
    /// Virtual method invoked by the world when an entity has been removed from the world.
    /// </summary>
    /// <param name="sender">The world.</param>
    /// <param name="eventArgs">The <see cref="EntityEventArgs"/> object containing the entity's ID and bitmask.</param>
    public virtual void OnEntityRemoved(object sender, EntityEventArgs eventArgs)
    {
      if ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask)
      {
        if (processing)
        {
          entitiesToRemove.Add(eventArgs.EntityId);
          return;
        }
        Compositions.Remove(eventArgs.EntityId);
      }
    }

    /// <summary>
    /// Virtual method invoked by the world when an entity has changed.
    /// </summary>
    /// <param name="sender">The world.</param>
    /// <param name="eventArgs">The <see cref="EntityEventArgs"/> object containing the entity's ID and bitmask.</param>
    public virtual void OnEntityChanged(object sender, EntityEventArgs eventArgs)
    {
      bool contains = Compositions.ContainsKey(eventArgs.EntityId);
      bool ofInterest = ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask);
      bool excluded = ((ExclusionBitmask & eventArgs.EntityBitmask) == ExclusionBitmask);

      if (contains && !ofInterest)
      {
        if (processing)
        {
          entitiesToRemove.Add(eventArgs.EntityId);
          return;
        }
        Compositions.Remove(eventArgs.EntityId);
      }
      else if (contains && excluded)
      {
        if (processing)
        {
          entitiesToRemove.Add(eventArgs.EntityId);
          return;
        }
        Compositions.Remove(eventArgs.EntityId);
      }
      else if (!contains && ofInterest && !excluded)
      {
        if (processing)
        {
          entitiesToAdd.Add(CreateComposition(eventArgs.EntityId));
          return;
        }
        Compositions.Add(eventArgs.EntityId, CreateComposition(eventArgs.EntityId));
      }
    }

    private dynamic CreateComposition(uint entityId)
    {
      dynamic composition = new ExpandoObject();

      (composition as ExpandoObject).AddProperty("EntityId", entityId);
      foreach (var componentType in ComponentsOfInterest)
      {
        (composition as ExpandoObject).AddProperty(componentType.Name, World.GetComponent(entityId, componentType));
      }

      return composition;
    }
  }
}
