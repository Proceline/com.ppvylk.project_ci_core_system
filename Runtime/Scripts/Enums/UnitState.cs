namespace ProjectCI.CoreSystem.Runtime.Enums
{
    /// <summary>
    /// Represents the current state of a unit
    /// </summary>
    public enum UnitState
    {
        /// <summary>
        /// Unit is idle and can perform actions
        /// </summary>
        Idle,
        
        /// <summary>
        /// Unit is currently moving
        /// </summary>
        Moving,
        
        /// <summary>
        /// Unit is using an ability
        /// </summary>
        UsingAbility,
        
        /// <summary>
        /// Unit is dead and cannot perform actions
        /// </summary>
        Dead
    }
} 