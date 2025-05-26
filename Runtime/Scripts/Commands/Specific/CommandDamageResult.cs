namespace ProjectCI.CoreSystem.Runtime.Commands
{
    /// <summary>
    /// The result of a command execution, can be sent to frontend for animation.
    /// </summary>
    public class CommandDamageResult : CommandResult
    {
        public int BeforeDamage;
        public int AfterDamage;
    }
} 