using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS
{
  public class EntityStatusComponent : IComponent
  {
    public bool Alive { get; set; }


    public EntityStatusComponent() : this(true) { }
    public EntityStatusComponent(bool alive = true)
    {
      Alive = alive;
    }
  }
}
