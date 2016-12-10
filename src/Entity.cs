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

    /// <summary>
    /// Adds a component to the entity.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <returns>The entity for chaining purposes.</returns>
    public Entity AddComponent(IComponent component)
    {
      _world.AddComponent(Id, component);
      return this;
    }

    /// <summary>
    /// Adds a component to the entity.
    /// </summary>
    /// <typeparam name="T">The generic IComponent to add.</typeparam>
    /// <returns>The entity for chaining purposes.</returns>
    public Entity AddComponent<T>() where T : IComponent, new ()
    {
      _world.AddComponent<T>(Id);
      return this;
    }

    /// <summary>
    /// Removes a component from the entity.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <returns>The entity for chaining purposes.</returns>
    public Entity RemoveComponent(IComponent component)
    {
      _world.RemoveComponent(Id, component);
      return this;
    }

    /// <summary>
    /// Removes a component from the entity.
    /// </summary>
    /// <typeparam name="T">The generic IComponent to add.</typeparam>
    /// <returns>The entity for chaining purposes.</returns>
    public Entity RemoveComponent<T>() where T : IComponent, new ()
    {
      _world.RemoveComponent<T>(Id);
      return this;
    }

    /// <summary>
    /// Marks the entity as added to the world.
    /// </summary>
    /// <returns>The entity for chaining purposes.</returns>
    public Entity AddToWorld()
    {
      _world.AddEntityToWorld(Id);
      return this;
    }

    /// <summary>
    /// Destroys the entity.
    /// </summary>
    public void Destroy()
    {
      _world.DestroyEntity(Id);
    }
  }
}
