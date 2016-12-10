using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Fishworks.ECS.Extensions;

namespace Fishworks.ECS
{
  /// <summary>
  /// Controlling class of the Entity Component System engine. Handles everything related to entities and components.
  /// </summary>
  public class World
  {
    private const int NoComponentsBitmask = 0;
    private const int StartingNumberOfEntities = 100;
    private const int EntityTableIncrementSize = 100;

    private const int ComponentRows = 0;
    private const int EntityColumns = 1;

    private const int MessagesToTakeFromQueueEachFrame = 10;

    private int numberOfComponents;

    private readonly Dictionary<uint, bool> entityInWorld;
    private IComponent[,] entityTable;
    private readonly Dictionary<Type, int> componentIndices;

    private readonly List<BaseSystem> activeSystems;
    private readonly List<BaseSystem> allSystems;

    private readonly Queue<BaseMessage> messageQueue;
    private Thread messageHandlerThread;
    private bool hasMessages;

    internal event EventHandler<EntityEventArgs> EntityAdded;
    internal event EventHandler<EntityEventArgs> EntityRemoved;
    internal event EventHandler<EntityEventArgs> EntityChanged;

    public event EventHandler<BaseMessage> MessageSent;

    /// <summary>
    /// Gets the current size of the entity table. Note that this is not
    /// the same as the number of actual entities in the world.
    /// </summary>
    public int EntityTableSize => entityTable.GetLength(EntityColumns);
    /// <summary>
    /// Gets the current number of entities active in the world. Note that
    /// this is not the same as the current size of the entity table.
    /// </summary>
    public int EntityCount => entityInWorld.Values.Count(b => b);

    /// <summary>
    /// Initializes a new instance of the World class.
    /// </summary>
    public World()
    {
      entityInWorld = new Dictionary<uint, bool>();
      componentIndices = new Dictionary<Type, int>();
      activeSystems = new List<BaseSystem>();
      allSystems = new List<BaseSystem>();
      messageQueue = new Queue<BaseMessage>();

      InitializeEntityComponentTable();

      messageHandlerThread = new Thread(HandleMessages);
      messageHandlerThread.Start();
    }

    private void InitializeEntityComponentTable()
    {
      var components = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where typeof(IComponent).IsAssignableFrom(type) && !type.IsInterface
                        select type).ToArray();

      int componentIndex = 0;
      foreach (var component in components)
      {
        componentIndices.Add(component, componentIndex);
        componentIndex++;
      }

      entityTable = new IComponent[componentIndex, StartingNumberOfEntities];
      numberOfComponents = componentIndex;

      for (uint i = 0; i < StartingNumberOfEntities; i++)
      {
        entityInWorld.Add(i, false);
      }
    }

    /// <summary>
    /// Updates the World object, calling Update and ProcessEntities on all
    /// systems marked as Active in the world.
    /// </summary>
    /// <param name="deltaTime">The delta time (time between frames) of the simulation above Fishworks.ECS</param>
    public void Update(float deltaTime)
    {
      foreach (var system in activeSystems)
      {
        system.Update(deltaTime);
        system.ProcessEntities();
      }
    }

    /// <summary>
    /// Returns the first empty entity in the entity table. Automatically increments the
    /// entity table if no empty entity can be found, and then returns the first in the
    /// new table.
    /// </summary>
    /// <returns>The entity created, for chaining purposes</returns>
    public Entity CreateEntity()
    {
      for (uint i = 0; i < entityTable.GetLength(EntityColumns); i++)
      {
        var bitmask = GetEntityBitmask(i);
        if (bitmask == NoComponentsBitmask)
        {
          return new Entity(i, this).AddComponent(new EntityStatusComponent());
        }
      }

      IncrementEntityTable();
      return new Entity((uint)entityTable.GetLength(EntityColumns) - EntityTableIncrementSize, this);
    }

    /// <summary>
    /// Marks an entity as added to the world, and invokjes the EntityAdded event, notifying all systems
    /// of the new entity.
    /// </summary>
    /// <param name="entityId">The ID of the added entity</param>
    public void AddEntityToWorld(uint entityId)
    {
      entityInWorld[entityId] = true;
      EntityAdded?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    /// <summary>
    /// Adds a system to the world. If marked as active the system will automatically be updated by the world
    /// when the world's Update method is called.
    /// </summary>
    /// <param name="system">The system to add to the world</param>
    /// <param name="active">Whether or not the system should be marked as active (automatically updated) or passive (updated by the implementation above Fishworks.ECS)</param>
    /// <returns>The system added.</returns>
    public BaseSystem AddSystemToWorld(BaseSystem system, bool active = true)
    {
      if (active)
        activeSystems.Add(system);

      allSystems.Add(system);

      return system;
    }

    public void AddComponent<T>(uint entityId) where T : IComponent, new() => AddComponent(entityId, new T());
    /// <summary>
    /// Adds a component to the entity with the given ID. Note that an entity can only hold one unique component of every component type. Notifies all systems that the entity has changed.
    /// </summary>
    /// <param name="entityId">The ID of the entity to add the component to.</param>
    /// <param name="component">The component to add to the entity.</param>
    public void AddComponent(uint entityId, IComponent component)
    {
      entityTable[componentIndices[component.GetType()], entityId] = component;
      if (entityInWorld[entityId]) EntityAdded?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public void RemoveComponent<T>(uint entityId) where T : IComponent, new() => RemoveComponent(entityId, new T());
    /// <summary>
    /// Removes a component from the entity with the given ID. Notifies all systems that the entity has changed.
    /// </summary>
    /// <param name="entityId">The entity to remove the component from.</param>
    /// <param name="component">The component to remove from the entity.</param>
    public void RemoveComponent(uint entityId, IComponent component)
    {
      entityTable[componentIndices[component.GetType()], entityId] = null;
      if (entityInWorld[entityId]) EntityChanged?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public IComponent GetComponent<T>(uint entityId) where T : IComponent => GetComponent(entityId, typeof(T));
    /// <summary>
    /// Gets a component of the given type from the entity with the given ID.
    /// </summary>
    /// <param name="entityId">The ID of the entity to fetch the component from.</param>
    /// <param name="componentType">The type of component to fetch from the entity.</param>
    /// <returns></returns>
    public IComponent GetComponent(uint entityId, Type componentType)
    {
      return entityTable[componentIndices[componentType], entityId];
    }

    /// <summary>
    /// Returns all components from the entity with the given ID.
    /// </summary>
    /// <param name="entityId">The ID of the entity to get components from.</param>
    /// <returns></returns>
    public IComponent[] GetComponents(uint entityId)
    {
      IComponent[] result = entityTable.GetColumn((int)entityId).Where(component => component != null).ToArray();
      return result;
    }

    /// <summary>
    /// Returns all entities matching the given bitmask. If an exlusion bitmask is set, the method will ignore
    /// any entities matching the exclusion.
    /// </summary>
    /// <param name="bitmask">The bitmask to match entities with.</param>
    /// <param name="exclusionBitmask">(optional) The excluding bitmask to match entities with.</param>
    /// <returns></returns>
    public uint[] GetEntitiesMatchingBitmask(int bitmask, int exclusionBitmask = -1)
    {
      List<uint> result = new List<uint>();
      for (uint i = 0; i < entityTable.GetLength(EntityColumns); i++)
      {
        if (exclusionBitmask != -1 && (exclusionBitmask & GetEntityBitmask(i)) == exclusionBitmask)
          continue;

        if ((bitmask & GetEntityBitmask(i)) == bitmask && entityInWorld[i])
          result.Add(i);
      }
      return result.ToArray();
    }

    /// <summary>
    /// Destroy the entity with the given ID. Notifies all systems that the entity has been removed.
    /// </summary>
    /// <param name="entityId">The ID of the entity to destroy.</param>
    public void DestroyEntity(uint entityId)
    {
      int entityBitmask = GetEntityBitmask(entityId);
      int componentIndex;

      for (componentIndex = 0; componentIndex < entityTable.GetLength(ComponentRows); componentIndex++)
      {
        entityTable[componentIndex, entityId] = null;
      }

      entityInWorld[entityId] = false;
      EntityRemoved?.Invoke(this, new EntityEventArgs(entityId, entityBitmask));
    }

    /// <summary>
    /// Gets the bitmask of a given entity.
    /// </summary>
    /// <param name="entityId">The ID of the entity to fetch a bitmask for.</param>
    /// <returns></returns>
    public int GetEntityBitmask(uint entityId)
    {
      if (entityId > entityTable.GetLength(EntityColumns))
        return -1;

      int bitmask = 0;
      for (int componentIndex = 0; componentIndex < numberOfComponents; componentIndex++)
      {
        var component = entityTable[componentIndex, entityId];
        if (component == null) continue;
        bitmask |= component.GetType().GetComponentBitmask();
      }

      return bitmask;
    }

    /// <summary>
    /// Returns a bitmask for the given component type.
    /// </summary>
    /// <typeparam name="T">The type of component to fetch the bitmask from.</typeparam>
    /// <returns>The bitmask of the given component type.</returns>
    public int GetComponentBitmask<T>() where T : IComponent
    {
      return typeof(T).GetComponentBitmask();
    }

    /// <summary>
    /// Returns a bitmask for the given component type. NOTE: This method will throw an exception if given a type that does not inherit from IComponent.
    /// It is recommended to use the generic <see cref="GetComponentBitmask{T}"/> instead.
    /// </summary>
    /// <param name="componentType">The type of component to fetch the bitmask from.</param>
    /// <returns>The bitmask of the given component type.</returns>
    public int GetComponentBitmask(Type componentType)
    {
      if (!typeof(IComponent).IsAssignableFrom(componentType))
        throw new ArgumentException("Method should only be used with types assigned from IComponent");

      return componentType.GetComponentBitmask();
    }

    /// <summary>
    /// Sends a message between systems.
    /// </summary>
    /// <param name="message">The message to send.</param>
    public void SendMessage(BaseMessage message)
    {
      lock (messageQueue)
      {
        messageQueue.Enqueue(message);
      }
      if (hasMessages == false)
        hasMessages = true;
    }

    private void IncrementEntityTable()
    {
      var rows = entityTable.GetLength(ComponentRows);
      var previousColumns = entityTable.GetLength(EntityColumns);

      IComponent[,] newTable = new IComponent[rows, previousColumns + EntityTableIncrementSize];

      for (int i = 0; i < rows; i++)
      {
        for (int j = 0; j < previousColumns; j++)
        {
          newTable[i, j] = entityTable[i, j];
        }
      }

      entityTable = newTable;

      for (int i = previousColumns; i < previousColumns + EntityTableIncrementSize; i++)
      {
        entityInWorld.Add((uint)i, false);
      }
    }

    private void HandleMessages()
    {
      while (Thread.CurrentThread.IsAlive)
      {
        if (hasMessages)
        {
          int messageCount = messageQueue.Count;
          int messagesToTake = messageCount > MessagesToTakeFromQueueEachFrame
            ? MessagesToTakeFromQueueEachFrame
            : messageCount;

          BaseMessage[] messages;
          lock (messageQueue)
          {
            messages = messageQueue.Dequeue(messagesToTake);
          }
          foreach (var message in messages)
          {
            MessageSent?.Invoke(this, message);
          }

          hasMessages = messageQueue.Count > 0;
        }
        Thread.Sleep(16);
      }
    }
  }
}
