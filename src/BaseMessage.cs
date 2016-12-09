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
