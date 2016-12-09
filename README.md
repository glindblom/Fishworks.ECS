# Fishworks.ECS
Simple implementation of Entity Component Systems, aimed at game development. Completely engine- and framework-agnostic as long as C# can be used in the environment.

>Entity-component system (ECS) is an architectural pattern that is mostly used in game development. An ECS follows the Composition over inheritance principle that allows greater flexibility in defining entities where every object in a game's scene is an entity (e.g. enemies, bullets, vehicles, etc.). Every Entity consists of one or more components which add additional behavior or functionality. Therefore, the behavior of an entity can be changed at runtime by adding or removing components. This eliminates the ambiguity problems of deep and wide inheritance hierarchies that are difficult to understand, maintain and extend.
> (courtesy of Wikipedia)

Fishworks.ECS is a very simple, pooled, implementation of the ECS pattern, with primarily three components:

### World.cs

Controlling class for the ECS pattern. Handles creation and pooling of entities, component fetching/creation, and more. This would be the entry point for using it in your own game.

```
class Game()
{
  public void Initialize()
  {
    world = new World();
    
    // add systems
    
    // add entities
  }
  
  public void Run()
  {
    while(shouldRun)
    {
      deltaTime = currentFrameTime - previousFrameTime;
      world.Update(deltaTime);
    }
  }
}
```

### BaseSystem.cs

Controlling class for entity compositions. This is meant to be extended for actual implementations. Uses a dynamically created composition object based upon the components marked as of interest to the system.

```
public MovementSystem : BaseSystem
{
  private float deltaTime;
  
  public MovementSystem(World world) : base(world, new Type[] { typeof(Transform), typeof(Movement) })
  {
  }
  
  public override void Update(float deltaTime)
  {
    this.deltaTime = deltaTime;
  }
  
  public override void ProcessEntity(dynamic entityComposition)
  {
    Transform transform = entityComposition.Transform;
    Movement movement = entityComposition.Movement;
    
    transform.Position += movement.Velocity * movement.MaxVelocity * deltaTime;
  }
}
```

Behind the scenes the BaseSystem uses bitmasks to determine if an entity is of interest to the system. If the entity is of interest a local composition is built based upon those components:
```
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
```

### IComponent.cs

Flagging interface meant to mark an object as a component. Components are purely containers for data, as defined by the ECS pattern. Any actual logic is always placed in systems.

```
public Transform : IComponent
{
  public Vector2 Position { get; set; }
  public float Rotation { get; set; }
}

public Movement : IComponent
{
  public Vector2 Velocity { get; set; }
  public float MaxVelocity { get; set; }
}
```
