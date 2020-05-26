using System;

namespace SterlingDB.Serialization
{
    /// <summary>
    ///     Attribute to tag a property, class, etc. that should not be serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Field |
                    AttributeTargets.Enum | AttributeTargets.Struct)]
    public class SterlingIgnoreAttribute : Attribute
    {
    }
}