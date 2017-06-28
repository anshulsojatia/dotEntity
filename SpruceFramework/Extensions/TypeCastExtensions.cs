// #region Author Information
// // TypeCastExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;

namespace SpruceFramework.Extensions
{
    public static class TypeCastExtensions
    {
        internal static SpruceQueryManager AsSpruceQueryManager(this ISpruceQueryManager manager)
        {
            var asObj = manager as SpruceQueryManager;
            if(asObj == null)
                throw new Exception("Can't cast provided manager");

            return asObj;
        }
    }
}