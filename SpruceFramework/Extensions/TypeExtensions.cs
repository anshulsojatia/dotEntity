// #region Author Information
// // TypeCastExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SpruceFramework.Extensions
{
    internal static class TypeExtensions
    {
        internal static SpruceQueryManager AsSpruceQueryManager(this ISpruceQueryManager manager)
        {
            var asObj = manager as SpruceQueryManager;
            if(asObj == null)
                throw new Exception("Can't cast provided manager");

            return asObj;
        }

        internal static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static IEnumerable<PropertyInfo> GetDatabaseUsableProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => !x.GetAccessors()[0].IsVirtual);
        }
    }
}