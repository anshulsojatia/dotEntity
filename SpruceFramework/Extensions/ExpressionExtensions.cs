// #region Author Information
// // ExpressionExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Collections.Generic;

namespace SpruceFramework
{
    internal static class ExpressionExtensions
    {
        public static bool In<T>(this T item, ICollection<T> values) where T : struct
        {
            return true;
        }

        public static bool In(this string item, ICollection<string> values)
        {
            return true;
        }
    }
}