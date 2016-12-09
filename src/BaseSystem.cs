using System;
using System.Collections.Generic;
using System.Dynamic;
using Fishworks.ECS.Extensions;

namespace Fishworks.ECS
{
  public abstract class BaseSystem
  {

    protected Type[] ComponentsOfInterest;

    protected World World;
    protected int SystemBitmask;
    protected int ExclusionBitmask;
    protected Dictionary<uint, dynamic> Compositions = new Dictionary<uint, dynamic>();
    private readonly List<uint> entitiesToRemove = new List<uint>();
    private readonly List<dynamic> entitiesToAdd = new List<dynamic>(); 

    private bool processing;

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
    }

    public abstract void Update(float deltaTime);
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

    public abstract void ProcessEntity(dynamic entityComposition);

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

    public virtual void OnEntityChanged(object sender, EntityEventArgs eventArgs)
    {
      bool contains = Compositions.ContainsKey(eventArgs.EntityId);
      bool ofInterest = ((SystemBitmask & eventArgs.EntityBitmask) == SystemBitmask);

      if (contains && !ofInterest)
      {
        if (processing)
        {
          entitiesToRemove.Add(eventArgs.EntityId);
          return;
        }
        Compositions.Remove(eventArgs.EntityId);
      }
      else if (!contains && ofInterest)
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
