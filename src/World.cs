using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS
{
  public class World
  {
    private const int NoComponentsBitmask = 0;
    private const int StartingNumberOfEntities = 100;
    private const int EntityTableIncrementSize = 100;

    private const int ComponentRows = 0;
    private const int EntityColumns = 1;

    private int numberOfComponents;

    private Dictionary<uint, bool> entityInWorld;
    private IComponent[,] entityComponentTable;
    private Dictionary<Type, int> componentIndices;

    private List<BaseSystem> activeSystems;
    private List<BaseSystem> passiveSystems; 

    internal event EventHandler<EntityEventArgs> EntityAdded;
    internal event EventHandler<EntityEventArgs> EntityRemoved;
    internal event EventHandler<EntityEventArgs> EntityChanged;

    public int EntityCount => entityComponentTable.GetLength(EntityColumns);

    public World()
    {
      entityInWorld = new Dictionary<uint, bool>();
      componentIndices = new Dictionary<Type, int>();
      activeSystems = new List<BaseSystem>();
      passiveSystems = new List<BaseSystem>();
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
      entityComponentTable = new IComponent[componentIndex, StartingNumberOfEntities];
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
    }

    public Entity CreateEntity()
    {
      for (uint i = 0; i < entityComponentTable.GetLength(EntityColumns); i++)
      {
        var bitmask = GetEntityBitmask(i);
        if (bitmask == NoComponentsBitmask)
        {
          return new Entity(i, this).AddComponent(new EntityStatusComponent());
        }
      }

      IncrementEntityTable();
      return new Entity((uint)entityComponentTable.GetLength(EntityColumns) - EntityTableIncrementSize, this);
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
      else
        passiveSystems.Add(system);

      return system;
    }

    public void AddComponent<T>(uint entityId) where T : IComponent, new() => AddComponent(entityId, new T());
    public void AddComponent(uint entityId, IComponent component)
    {
      entityComponentTable[componentIndices[component.GetType()], entityId] = component;
      if (entityInWorld[entityId]) EntityAdded?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public void RemoveComponent<T>(uint entityId) where T : IComponent, new() => RemoveComponent(entityId, new T());
    public void RemoveComponent(uint entityId, IComponent component)
    {
      entityComponentTable[componentIndices[component.GetType()], entityId] = null;
      if (entityInWorld[entityId]) EntityChanged?.Invoke(this, new EntityEventArgs(entityId, GetEntityBitmask(entityId)));
    }

    public IComponent GetComponent<T>(uint entityId) where T : IComponent => GetComponent(entityId, typeof (T));
    public IComponent GetComponent(uint entityId, Type componentType)
    {
      return entityComponentTable[componentIndices[componentType], entityId];
    }

    public void DestroyEntity(uint entityId)
    {
      int entityBitmask = GetEntityBitmask(entityId);
      int componentIndex;

      for (componentIndex = 0; componentIndex < entityComponentTable.GetLength(ComponentRows); componentIndex++)
      {
        entityComponentTable[componentIndex, entityId] = null;
      }

      entityInWorld[entityId] = false;
      EntityRemoved?.Invoke(this, new EntityEventArgs(entityId, entityBitmask));
    }

    public int GetEntityBitmask(uint entityId)
    {
      if (entityId > entityComponentTable.GetLength(EntityColumns))
        return -1;

      int bitmask = 0;
      for (int componentIndex = 0; componentIndex < numberOfComponents; componentIndex++)
      {
        var component = entityComponentTable[componentIndex, entityId];
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

    private void IncrementEntityTable()
    {
      Stopwatch incrementTime = new Stopwatch();
      incrementTime.Start();

      var rows = entityComponentTable.GetLength(ComponentRows);
      var previousColumns = entityComponentTable.GetLength(EntityColumns);

      IComponent[,] newTable = new IComponent[rows, previousColumns + EntityTableIncrementSize];
      
      for (int i = 0; i < rows; i++)
      {
        for (int j = 0; j < previousColumns; j++)
        {
          newTable[i, j] = entityComponentTable[i, j];
        }
      }

      entityComponentTable = newTable;

      for (int i = previousColumns; i < previousColumns + EntityTableIncrementSize; i++)
      {
        entityInWorld.Add((uint) i, false);
      }

      incrementTime.Stop();
      System.Diagnostics.Trace.WriteLine(string.Format("Increment table size took {0} ms, {1} ticks", incrementTime.ElapsedMilliseconds, incrementTime.ElapsedTicks));
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
