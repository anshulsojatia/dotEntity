/**
 * Copyright(C) 2017-2021  Sojatia Infocrafts Private Limited
 * 
 * This file (DotEntityDb.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using DotEntity.Enumerations;
using DotEntity.Providers;
using DotEntity.Versioning;

#if DEBUG
[assembly: InternalsVisibleTo("DotEntity.Tests")]
#endif
namespace DotEntity
{
    public partial class DotEntityDb
    {
        public static string ConnectionString { get; set; }

        private static ConcurrentDictionary<Type, Tuple<string, string>> EntityTableNames { get; set; }

        public static SelectQueryMode SelectQueryMode { get; set; }

        public static string GlobalTableNamePrefix { get; set; } = "";

        static DotEntityDb()
        {
            EntityTableNames = new ConcurrentDictionary<Type, Tuple<string, string>>();
            QueryProcessor = new QueryProcessor();
            SelectQueryMode = SelectQueryMode.Explicit;
            ExcludedColumns = new ConcurrentDictionary<Type, string[]>();
        }

        public static void Initialize(string connectionString, IDatabaseProvider provider, SelectQueryMode selectQueryMode = SelectQueryMode.Explicit)
        {
            ConnectionString = connectionString;
            Provider = provider;
            Provider.QueryGenerator = Provider.QueryGenerator ?? new DefaultQueryGenerator();
            Provider.TypeMapProvider = Provider.TypeMapProvider ?? new DefaultTypeMapProvider();
            Provider.DatabaseTableGenerator = Provider.DatabaseTableGenerator ?? new DefaultDatabaseTableGenerator();
            SelectQueryMode = selectQueryMode;
            ResetVersions();
        }

        public static IDatabaseProvider Provider { get; internal set; }

        internal static QueryProcessor QueryProcessor { get; set; }

        public static void MapTableNameForType<T>(string tableName, string schema = "")
        {
            EntityTableNames.AddOrUpdate(typeof(T), Tuple.Create(schema, tableName), (type, s) => Tuple.Create(schema, tableName));
        }

        public static void Relate<TSource, TTarget>(string sourceColumnName, string destinationColumnName)
        {
            RelationMapper.Relate<TSource, TTarget>(sourceColumnName, destinationColumnName);
        }

        public static string GetTableNameForType<T>(bool withoutSchema = false)
        {
            var tType = typeof(T);
            return GetTableNameForType(tType, withoutSchema);
        }

        public static string GetTableNameForType(Type type, bool withoutSchema = false)
        {
            if (EntityTableNames.ContainsKey(type))
            {
                if (withoutSchema)
                {
                    return EntityTableNames[type].Item2;
                }
                else
                {
                    if (EntityTableNames[type].Item1 == "")
                    {
                        return EntityTableNames[type].Item2;
                    }
                }
                return EntityTableNames[type].Item1 + "." + EntityTableNames[type].Item2;
            }
            return (GlobalTableNamePrefix + type.Name);
        }

        private static ConcurrentDictionary<string, Queue<IDatabaseVersion>> _databaseVersions;

        private static void ResetVersions()
        {
            if(_databaseVersions != null)
                foreach(var kp in _databaseVersions)
                    while (_databaseVersions[kp.Key].Count > 0)
                    {
                        _databaseVersions[kp.Key].Dequeue();
                    }
        }
        public static void EnqueueVersions(string contextKey, params IDatabaseVersion[] versions)
        {
            _databaseVersions = _databaseVersions ?? new ConcurrentDictionary<string, Queue<IDatabaseVersion>>();
            if (!_databaseVersions.TryGetValue(contextKey, out Queue<IDatabaseVersion> queue))
            {
                queue = new Queue<IDatabaseVersion>();
                _databaseVersions.TryAdd(contextKey, queue);
            }
            foreach (var version in versions)
                queue.Enqueue(version);
        }

        public static void UpdateDatabaseToLatestVersion(string callingContextName)
        {
            Throw.IfNoDatabaseVersions(_databaseVersions[callingContextName]);
            var versionRunner = new VersionUpdater(callingContextName, _databaseVersions[callingContextName].ToArray());
            versionRunner.RunUpgrade();
        }

        public static void UpdateDatabaseToVersion(string callingContextName, string versionKey)
        {
            Throw.IfNoDatabaseVersions(_databaseVersions[callingContextName]);
            var versionRunner = new VersionUpdater(callingContextName, _databaseVersions[callingContextName].ToArray());
            versionRunner.RunDowngrade(versionKey);
        }

        public static IList<string> GetAppliedVersions(string callingContextName)
        {
            Throw.IfNoDatabaseVersions(_databaseVersions[callingContextName]);
            var versionRunner = new VersionUpdater(callingContextName);
            return versionRunner.GetAppliedVersions();
        }

        public static IDictionary<string, List<string>> GetAllAppliedVersions()
        {
            return VersionUpdater.GetAllAppliedVersions();
        }

        private static ConcurrentDictionary<Type, string[]> ExcludedColumns { get; }

        public static void IgnoreColumns<T>(params string[] columnNames)
        {
            if (columnNames.Length == 0)
                return;
            ExcludedColumns.TryAdd(typeof(T), columnNames);
        }

        public static string[] GetIgnoredColumns(Type type)
        {
            ExcludedColumns.TryGetValue(type, out string[] columns);
            return columns;
        }
    }
}