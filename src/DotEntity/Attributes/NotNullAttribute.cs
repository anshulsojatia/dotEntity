using System;

namespace DotEntity.Attributes
{
    /// <summary>
    /// Specifies that a particular field should not be null
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute : Attribute
    {
        
    }
}