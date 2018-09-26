/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (DotEntity.Database.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DotEntity.Extensions;

namespace DotEntity
{
    public partial class DotEntity
    {
        public static class Database
        {
            private static IDatabaseTableGenerator DatabaseTableGenerator => DotEntityDb.Provider.DatabaseTableGenerator;
            private static readonly IList<string> ProcessedTables = new List<string>();
            public static void CreateTable<T>(IDotEntityTransaction transaction) where T : class
            {
                var tableName = GetTableName<T>();
                Throw.IfTableCreated(WasTableCreated<T>(), tableName);
                //add to prcessed tables
                ProcessedTables.Add(tableName);
                var script = DatabaseTableGenerator.GetCreateTableScript<T>();
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void CreateTables(Type[] tableTypes, IDotEntityTransaction transaction)
            {
                foreach (var tableType in tableTypes)
                {
                    var tableName = GetTableName(tableType);
                    Throw.IfTableCreated(WasTableCreated(tableType), tableName);
                    var script = DatabaseTableGenerator.GetCreateTableScript(tableType);
                    transaction.Manager.AsDotEntityQueryManager().Do(script, null);
                }
            }

            public static void DropTable<T>(IDotEntityTransaction transaction) where T : class
            {
                var script = DatabaseTableGenerator.GetDropTableScript<T>();
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void DropTable(string tableName, IDotEntityTransaction transaction)
            {
                var script = DatabaseTableGenerator.GetDropTableScript(DotEntityDb.GlobalTableNamePrefix + tableName);
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void DropTables(Type[] tableTypes, IDotEntityTransaction transaction)
            {
                foreach (var tableType in tableTypes)
                {
                    var script = DatabaseTableGenerator.GetDropTableScript(tableType);
                    transaction.Manager.AsDotEntityQueryManager().Do(script, null);
                }

            }

            public static void CreateConstraint(Relation relation, IDotEntityTransaction transaction)
            {
                var script = DatabaseTableGenerator.GetCreateConstraintScript(relation);
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void DropConstraint(Relation relation, IDotEntityTransaction transaction)
            {
                var script = DatabaseTableGenerator.GetDropConstraintScript(relation);
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void AddColumn<T, T1>(string columnName, T1 value, IDotEntityTransaction transaction)
            {
                if (WasTableCreated<T>())
                    return; //don't do anything, as table was already created by some previous version
                var script = DatabaseTableGenerator.GetAddColumnScript<T, T1>(columnName, value, typeof(T).GetProperty(columnName));
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void DropColumn<T>(string columnName, IDotEntityTransaction transaction)
            {
                var tableType = typeof(T);
                var script = DatabaseTableGenerator.GetDropColumnScript(tableType, columnName);
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void AlterColumn<T>(Expression<Action<T, object>> columnExpression, IDotEntityTransaction transaction)
            {
                var tableType = typeof(T);
                var columnName = columnExpression.Name;
                var columnType = columnExpression.Type;
                var script = DatabaseTableGenerator.GetAlterColumnScript(tableType, columnName, columnType);
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void Query(string query, object parameters, IDotEntityTransaction transaction, bool isProcedure = false)
            {
                var manager = transaction.Manager.AsDotEntityQueryManager();
                if (isProcedure)
                    manager.DoProcedure(query, parameters);
                else
                    manager.Do(query, parameters);
            }

            #region Helpers
            private static bool WasTableCreated<T>()
            {
                return ProcessedTables.Contains(GetTableName<T>());
            }

            private static bool WasTableCreated(Type type)
            {
                return ProcessedTables.Contains(GetTableName(type));
            }
            private static string GetTableName<T>()
            {
                return GetTableName(typeof(T));
            }

            private static string GetTableName(Type type)
            {
                var tableName = DotEntityDb.GetTableNameForType(type);
                return tableName;
            }
            #endregion
        }
    }
}