// #region Author Information
// // Spruce.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SpruceFramework.Providers;

[assembly: InternalsVisibleTo("SpruceFramework.Tests")]
namespace SpruceFramework
{
    public class Spruce
    {
        public static string ConnectionString { get; set; }

        public static string ProviderName { get; set; }

        private static ConcurrentDictionary<Type, string> EntityTableNames { get; set; }

        static Spruce()
        {
            EntityTableNames = new ConcurrentDictionary<Type, string>();
            QueryProcessor = new QueryProcessor();
            QueryGenerator = new SqlServerQueryGenerator();
        }

        public static void Initialize(string connectionString, string providerName)
        {
            ConnectionString = connectionString;
            ProviderName = providerName;
        }

        public static void Initialize(string connectionString, IDatabaseProvider provider)
        {
            ConnectionString = connectionString;
            Provider = provider;
            ProviderName = provider.ProviderName;
        }

        public static IDatabaseProvider Provider { get; internal set; }

        internal static IQueryGenerator QueryGenerator { get; set; }

        internal static IQueryProcessor QueryProcessor { get; set; }

        public static void MapTableNameForType<T>(string tableName)
        {
            EntityTableNames.AddOrUpdate(typeof(T), tableName, (type, s) => tableName);
        }

        public static string GetTableNameForType<T>()
        {
            var tType = typeof(T);
            return GetTableNameForType(tType);
        }

        public static string GetTableNameForType(Type type)
        {
            return EntityTableNames.ContainsKey(type) ? EntityTableNames[type] : type.Name;
        }
    }
}