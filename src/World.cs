using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fishworks.ECS.Extensions;

namespace Fishworks.ECS
{
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

    internal event EventHandler<EntityEventArgs> EntityAdded;
    internal event EventHandler<EntityEventArgs> EntityRemoved;
    internal event EventHandler<EntityEventArgs> EntityChanged;

    public event EventHandler<BaseMessage> MessageInQueue;

    public int EntityCount => entityTable.GetLength(EntityColumns);

    public World()
    {
      entityInWorld = new Dictionary<uint, bool>();
      componentIndices = new Dictionary<Type, int>();
      activeSystems = new List<BaseSystem>();
      allSystems = new List<BaseSystem>();
      messageQueue = new Queue<BaseMessage>();
      InitializeEntityComponentTable();
    }

    private void InitializeEntityComponentTable()
    {
      var components = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        from type in assembly.GetTypes()
                        where typeof (IComponent).IsAssignableFrom(type) && !type.IsInterface
                        select type).ToArray();

      int componentIndex = 0;
      foreach (var component in components)
      {
        componentIndices.Add(component, componentIndex);
        componentIndex++;
      }

      // set up entity-component table to contain 100 entities to start with
      entityTable = new IComponent[componentIndex, StartingNumberOfEntities];
      numberOfComponents = componentIndex;

      for (uint i = 0; i < StartingNumberOfEntities; i++)
      {
        entityInWorld.Add(i, false);
      }
    }


    public void Update(float deltaTime)
    {
      foreach (var system in activeSystems)
      {
        system.Update(deltaTime);
        system.ProcessEntities();
      }

      ProcessMessages();
    }

    private void ProcessMessages()
    {
      int queueCount = messageQueue.Count;

      int messagesToTake = queueCount < MessagesToTakeFromQueueEachFrame ? queueCount : MessagesToTakeFromQueueEachFrame;

      BaseMessage[] messages = messageQueue.Dequeue(messagesToTake);
      foreach (var message in messages)
      {
        MessageInQueue?.Invoke(this, message);
      }
    }

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

    public void AddEntityToWorld(uint entityId)
    {
      entityInWorld[entityId] = true;
      EntityAdded?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public BaseSystem AddSystemToWorld(BaseSystem system, bool active = true)
    {
      if (active)
        activeSystems.Add(system);

      allSystems.Add(system);

      return system;
    }

    public void AddComponent<T>(uint entityId) where T : IComponent, new() => AddComponent(entityId, new T());
    public void AddComponent(uint entityId, IComponent component)
    {
      entityTable[componentIndices[component.GetType()], entityId] = component;
      if (entityInWorld[entityId]) EntityAdded?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public void RemoveComponent<T>(uint entityId) where T : IComponent, new() => RemoveComponent(entityId, new T());
    public void RemoveComponent(uint entityId, IComponent component)
    {
      entityTable[componentIndices[component.GetType()], entityId] = null;
      if (entityInWorld[entityId]) EntityChanged?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public IComponent GetComponent<T>(uint entityId) where T : IComponent => GetComponent(entityId, typeof (T));
    public IComponent GetComponent(uint entityId, Type componentType)
    {
      return entityTable[componentIndices[componentType], entityId];
    }

    public IComponent[] GetComponents(uint entityId)
    {
      IComponent[] result = entityTable.GetColumn((int) entityId).Where(component => component != null).ToArray();
      return result;
    }

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

    public int GetComponentBitmask<T>() where T : IComponent
    {
      return typeof(T).GetComponentBitmask();
    }

    public int GetComponentBitmask(Type componentType)
    {
      if (!typeof (IComponent).IsAssignableFrom(componentType))
        throw new Exception("Method should only be used with types assigned from IComponent");

      return componentType.GetComponentBitmask();
    }

    public void SendMessage(BaseMessage message)
    {
      messageQueue.Enqueue(message);
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
        entityInWorld.Add((uint) i, false);
      }
    }
  }

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
