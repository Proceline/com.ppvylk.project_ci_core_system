using System;

namespace ProjectCI.CoreSystem.DependencyInjection
{
    /// <summary>
    /// Attribute to mark properties and fields for dependency injection
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class StaticInjectableTargetAttribute : Attribute
    {
        // Empty
    }
} 