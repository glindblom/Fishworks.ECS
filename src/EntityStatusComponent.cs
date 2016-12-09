namespace Fishworks.ECS
{
  public class EntityStatusComponent : IComponent
  {
    public bool Alive { get; set; }

    public EntityStatusComponent()
    {
      Alive = true;
    }
  }
}
