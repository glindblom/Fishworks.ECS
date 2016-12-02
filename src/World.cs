using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fishworks.ECS
{
  /// <summary>
  /// Engine class of the ECS implementation. Handles entities, components and systems.
  /// </summary>
  public class World
  {
    private const int NoComponentsBitmask = 0;
    private const int StartingNumberOfEntities = 100;
    private const int EntityTableIncrementSize = 100;

    private const int ComponentRows = 0;
    private const int EntityColumns = 1;

    private readonly Dictionary<uint, bool> _entityInWorld;
    private readonly Dictionary<Type, int> _componentIndices;
    private IComponent[,] _entityTable;

    private readonly List<BaseSystem> _activeSystems;

    internal event EventHandler<EntityEventArgs> EntityAdded;
    internal event EventHandler<EntityEventArgs> EntityRemoved;
    internal event EventHandler<EntityEventArgs> EntityChanged;

    /// <summary>
    /// Returns the current size of the World's Entity Table. Note, this is
    /// not the same thing as the number of entities currently being handled,
    /// see <see cref="EntityCount">EntityCount</see>
    /// </summary>
    public int EntityTableSize => _entityTable.GetLength(EntityColumns);

    /// <summary>
    /// Returns the current amount of active entities.
    /// </summary>
    public int EntityCount => GetEntityCount();

    /// <summary>
    /// Creates a new instance of the World class.
    /// </summary>
    public World()
    {
      _entityInWorld = new Dictionary<uint, bool>();
      _componentIndices = new Dictionary<Type, int>();
      _activeSystems = new List<BaseSystem>();
      InitializeEntityTable();
    }

    /// <summary>
    /// Initializes the Entity Table
    /// </summary>
    private void InitializeEntityTable()
    {
      var components = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where typeof (IComponent).IsAssignableFrom(type) && !type.IsInterface
                        select type).ToArray();

      int componentIndex = 0;
      foreach (var component in components)
      {
        _componentIndices.Add(component, componentIndex);
        componentIndex++;
      }

      // set up entity-component table to contain 100 entities to start with
      _entityTable = new IComponent[componentIndex, StartingNumberOfEntities];

      for (uint i = 0; i < StartingNumberOfEntities; i++)
      {
        _entityInWorld.Add(i, false);
      }
    }

    /// <summary>
    /// Called to update the world. The function loops over all active systems
    /// and updates them.
    /// </summary>
    /// <param name="deltaTime">The current delta time (time between updates) of the simulation</param>
    public void Update(float deltaTime)
    {
      foreach (var system in _activeSystems)
      {
        system.Update(deltaTime);
        system.ProcessEntities();
      }
    }

    /// <summary>
    /// Create a new entity. If the entity table is filled, the method automatically
    /// increments its size.
    /// </summary>
    /// <returns>The entity created, for chaining purposes</returns>
    public Entity CreateEntity()
    {
      for (uint i = 0; i < _entityTable.GetLength(EntityColumns); i++)
      {
        var bitmask = GetEntityBitmask(i);
        if (bitmask == NoComponentsBitmask)
        {
          return new Entity(i, this).AddComponent(new EntityStatusComponent());
        }
      }

      IncrementEntityTable();
      return new Entity((uint)_entityTable.GetLength(EntityColumns) - EntityTableIncrementSize, this);
    }

    /// <summary>
    /// Marks the entity as added to the world, and invokes the EntityAdded event.
    /// </summary>
    /// <param name="entityId">The ID of the entity to add</param>
    public void AddEntityToWorld(uint entityId)
    {
      _entityInWorld[entityId] = true;
      EntityAdded?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    /// <summary>
    /// Adds a system to the world's active systems list, causing the system
    /// to automatically be updated when the world's Update method is called.
    /// </summary>
    /// <param name="system">The system to add</param>
    public void AddSystemToWorld(BaseSystem system)
    {
      _activeSystems.Add(system);
    }

    /// <summary>
    /// Removes a system from the world's active systems list.
    /// </summary>
    /// <param name="system">The system to remove</param>
    public void RemoveSystemFromWorld(BaseSystem system)
    {
      _activeSystems.Remove(system);
    }
    
    public void AddComponent<T>(uint entityId) where T : IComponent, new() => AddComponent(entityId, new T());
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="component"></param>
    public void AddComponent(uint entityId, IComponent component)
    {
      _entityTable[_componentIndices[component.GetType()], entityId] = component;
      if (_entityInWorld[entityId]) EntityAdded?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public void RemoveComponent<T>(uint entityId) where T : IComponent, new() => RemoveComponent(entityId, new T());
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="component"></param>
    public void RemoveComponent(uint entityId, IComponent component)
    {
      _entityTable[_componentIndices[component.GetType()], entityId] = null;
      if (_entityInWorld[entityId]) EntityChanged?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public IComponent GetComponent<T>(uint entityId) where T : IComponent => GetComponent(entityId, typeof (T));
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <param name="componentType"></param>
    /// <returns></returns>
    public IComponent GetComponent(uint entityId, Type componentType)
    {
      return _entityTable[_componentIndices[componentType], entityId];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    public void DestroyEntity(uint entityId)
    {
      int entityBitmask = GetEntityBitmask(entityId);
      int componentIndex;

      for (componentIndex = 0; componentIndex < _entityTable.GetLength(ComponentRows); componentIndex++)
      {
        _entityTable[componentIndex, entityId] = null;
      }

      _entityInWorld[entityId] = false;
      EntityRemoved?.Invoke(this, new EntityEventArgs(entityId, entityBitmask));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityId"></param>
    /// <returns></returns>
    public int GetEntityBitmask(uint entityId)
    {
      if (entityId > _entityTable.GetLength(EntityColumns))
        return -1;

      int numberOfComponents = _entityTable.GetLength(ComponentRows);
      int bitmask = 0;
      for (int componentIndex = 0; componentIndex < numberOfComponents; componentIndex++)
      {
        var component = _entityTable[componentIndex, entityId];
        if (component == null) continue;
        bitmask |= component.GetType().GetComponentBitmask();
      }

      return bitmask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public int GetComponentBitmask<T>() where T : IComponent
    {
      return typeof(T).GetComponentBitmask();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="componentType"></param>
    /// <returns></returns>
    public int GetComponentBitmask(Type componentType)
    {
      if (!typeof (IComponent).IsAssignableFrom(componentType))
        throw new Exception("Method should only be used with types assigned from IComponent");

      return componentType.GetComponentBitmask();
    }

    /// <summary>
    /// 
    /// </summary>
    private void IncrementEntityTable()
    {
      Stopwatch incrementTime = new Stopwatch();
      incrementTime.Start();

      var rows = _entityTable.GetLength(ComponentRows);
      var previousColumns = _entityTable.GetLength(EntityColumns);

      IComponent[,] newTable = new IComponent[rows, previousColumns + EntityTableIncrementSize];
      
      for (int i = 0; i < rows; i++)
      {
        for (int j = 0; j < previousColumns; j++)
        {
          newTable[i, j] = _entityTable[i, j];
        }
      }

      _entityTable = newTable;

      for (int i = previousColumns; i < previousColumns + EntityTableIncrementSize; i++)
      {
        _entityInWorld.Add((uint) i, false);
      }

      incrementTime.Stop();
      #if DEBUG
      Trace.WriteLine($"Increment table size took {incrementTime.ElapsedMilliseconds} ms, {incrementTime.ElapsedTicks} ticks");
      #endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int GetEntityCount()
    {
      int count = 0;
      for (int i = 0; i < _entityTable.GetLength(EntityColumns); i++)
      {
        count += GetEntityBitmask((uint) i) != NoComponentsBitmask ? 1 : 0;
      }
      return count;
    }
  }

  /// <summary>
  /// 
  /// </summary>
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
