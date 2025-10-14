using System;

namespace QoreDB.Common.Attributes
{
    /// <summary>
    /// An attribute used to specify a string representation for an enum member
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class EnumStringAttribute : Attribute
    {
        /// <summary>
        /// Gets the string value associated with the enum member
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumStringAttribute"/> class
        /// </summary>
        /// <param name="value">The string value to associate with the enum member</param>
        public EnumStringAttribute(string value)
        {
            Value = value;
        }
    }
}
