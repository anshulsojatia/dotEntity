namespace DotEntity.Enumerations
{
    /// <summary>
    /// Specifies the column selection mode for select queries
    /// </summary>
    public enum SelectQueryMode
    {
        /// <summary>
        /// Specifies that queries should be generated like SELECT * FROM ...
        /// </summary>
        Wildcard,
        /// <summary>
        /// Specifies that queries should be generated like SELECT Col1, Col2.. FROM ...
        /// </summary>
        Explicit
    }
}