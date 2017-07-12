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

namespace DotEntity.Extensions
{
    internal static class TypeExtensions
    {
        internal static DotEntityQueryManager AsDotEntityQueryManager(this IDotEntityQueryManager manager)
        {
            var asObj = manager as DotEntityQueryManager;
            Throw.IfArgumentNull(asObj, nameof(manager));
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