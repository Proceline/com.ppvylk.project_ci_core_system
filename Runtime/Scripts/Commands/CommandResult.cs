namespace ProjectCI.CoreSystem.Runtime.Commands
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class CommandResult
    {
        public string ResultId;
        public string OwnerId;
        public string TargetId;
        public string CommandType; // e.g. "Damage", "Heal"
        public int Value; // e.g. damage or heal amount
        public string ExtraInfo; // Optional, for custom info
    }
} 