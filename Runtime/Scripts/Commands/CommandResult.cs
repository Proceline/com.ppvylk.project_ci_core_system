namespace ProjectCI.CoreSystem.Runtime.Commands
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public abstract class CommandResult
    {
        public string ResultId;
        public string OwnerId;

        public abstract void ApplyCommand();
        public virtual bool TryGetTarget(out string targetId)
        {
            targetId = string.Empty;
            return false;
        }
    }
} 