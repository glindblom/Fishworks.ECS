namespace Fishworks.ECS
{
  /// <summary>
  /// Abstract class meant to be extended. Used for system-to-system communication.
  /// </summary>
  public abstract class BaseMessage
  {
    private bool _aborted;

    /// <summary>
    /// Boolean representing if the message has been aborted.
    /// </summary>
    public bool Aborted => _aborted;

    /// <summary>
    /// String representing the message's type.
    /// </summary>
    public string MessageType { get; set; }

    /// <summary>
    /// Marks the message as aborted.
    /// </summary>
    public void Abort()
    {
      _aborted = true;
    }
  }
}
