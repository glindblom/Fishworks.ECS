using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishworks.ECS
{
  public abstract class BaseMessage
  {
    private bool _aborted;
    public bool Aborted => _aborted;
    public string MessageType { get; set; }

    public void Abort()
    {
      _aborted = true;
    }
  }
}
