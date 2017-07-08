// #region Author Information
// // EntityExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace DotEntity.Extensions
{
    public static class EntityExtensions
    {
        public static string GetKeyColumnName(this Type type)
        {
            var propertyInfos = type.GetProperties().Where(x => x.IsDefined(typeof(KeyAttribute)));
            var keyColumnName = propertyInfos.LastOrDefault()?.Name;
            if(keyColumnName == null)
                throw new Exception("No key column specified for entity");
            return keyColumnName;
        }
    }
}