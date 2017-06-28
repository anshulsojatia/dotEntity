// #region Author Information
// // DataDeserializerExtensions.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;

namespace SpruceFramework.Extensions
{
    internal static class DataDeserializerExtensions
    {
        internal static string[] GetTypedColumnNames(this IDataDeserializer deserializer, string[] columns, Type type)
        {
            var typedColumns = new string[columns.Length];
            var typeName = type.Name;
            for (var i = 0; i < columns.Length; i++)
                typedColumns[i] = typeName + "." + columns[i];

            return typedColumns;
        }
    }
}