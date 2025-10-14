using System;
using System.Linq;
using System.Reflection;
using QoreDB.Common.Attributes;

namespace QoreDB.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for enums
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieves the string value from the <see cref="EnumStringAttribute"/> of an enum member
        /// </summary>
        /// <param name="value">The enum value to get the string for</param>
        /// <returns>The string specified in the attribute, or the default enum name if the attribute is not found</returns>
        public static string GetString(this Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            
            var attribute = fieldInfo?
                .GetCustomAttributes(typeof(EnumStringAttribute), false)
                .FirstOrDefault() as EnumStringAttribute;

            return attribute?.Value ?? value.ToString();
        }
    }
}
