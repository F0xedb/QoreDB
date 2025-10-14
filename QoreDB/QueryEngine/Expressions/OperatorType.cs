using QoreDB.Common.Attributes;

namespace QoreDB.QueryEngine.Expressions
{
    /// <summary>
    /// Defines the types of comparison operators supported in expressions
    /// </summary>
    public enum OperatorType
    {
        #region  Comparison
        /// <summary>
        /// Represents an equality comparison (=)
        /// </summary>
        [EnumString("=")]
        Equal,
        /// <summary>
        /// Represents a greater than comparison (>)
        /// </summary>
        [EnumString(">")]
        GreaterThan,
        /// <summary>
        /// Represents a less than comparison (<)
        /// </summary>
        [EnumString("<")]
        LessThan,
        /// <summary>
        /// Represents a greater than or equal to comparison (>=)
        /// </summary>
        [EnumString(">=")]
        GreaterThanOrEqual,
        /// <summary>
        /// Represents a less than or equal to comparison (<=)
        /// </summary>
        [EnumString("<=")]
        LessThanOrEqual,
        #endregion

        #region Logical
        /// <summary>
        /// Represents a logical OR operation
        /// </summary>
        [EnumString("OR")]
        Or,
        /// <summary>
        /// Represents a logical AND operation
        /// </summary>
        [EnumString("AND")]
        And,
        #endregion

        #region Arithmetic
        /// <summary>
        /// Represents an addition operation (+)
        /// </summary>
        [EnumString("+")]
        Add,
        /// <summary>
        /// Represents a subtraction operation (-)
        /// </summary>
        [EnumString("-")]
        Subtract,
        /// <summary>
        /// Represents a multiplication operation (*)
        /// </summary>
        [EnumString("*")]
        Multiply,
        /// <summary>
        /// Represents a division operation (/)
        /// </summary>
        [EnumString("/")]
        Divide
        #endregion
    }
}
