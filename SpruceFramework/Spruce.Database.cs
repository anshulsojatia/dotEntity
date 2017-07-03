// #region Author Information
// // Spruce.Database.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Linq.Expressions;
using SpruceFramework.Extensions;

namespace SpruceFramework
{
    public partial class Spruce
    {
        public static class Database
        {
            private static readonly IDatabaseTableGenerator DatabaseTableGenerator = Spruce.Provider.DatabaseTableGenerator;

            public static void CreateTable<T>(ISpruceTransaction transaction) where T : class
            {
                var script = DatabaseTableGenerator.GetCreateTableScript<T>();
                transaction.Manager.AsSpruceQueryManager().Do(script, null);
            }

            public static void CreateTables(Type[] tableTypes, ISpruceTransaction transaction)
            {
                foreach (var tableType in tableTypes)
                {
                    var script = DatabaseTableGenerator.GetCreateTableScript(tableType);
                    transaction.Manager.AsSpruceQueryManager().Do(script, null);
                }
            }

            public static void DropTable<T>(ISpruceTransaction transaction) where T : class
            {
                var script = DatabaseTableGenerator.GetDropTableScript<T>();
                transaction.Manager.AsSpruceQueryManager().Do(script, null);
            }

            public static void DropTables(Type[] tableTypes, ISpruceTransaction transaction)
            {
                foreach (var tableType in tableTypes)
                {
                    var script = DatabaseTableGenerator.GetDropTableScript(tableType);
                    transaction.Manager.AsSpruceQueryManager().Do(script, null);
                }

            }

            public static void CreateConstraint(Relation relation, ISpruceTransaction transaction)
            {
                var script = DatabaseTableGenerator.GetCreateConstraintScript(relation);
                transaction.Manager.AsSpruceQueryManager().Do(script, null);
            }

            public static void DropConstraint(Relation relation, ISpruceTransaction transaction)
            {
                var script = DatabaseTableGenerator.GetDropConstraintScript(relation);
                transaction.Manager.AsSpruceQueryManager().Do(script, null);
            }

            public static void AddColumn<T>(Expression<Action<T, object>> columnExpression, ISpruceTransaction transaction)
            {
                var tableType = typeof(T);
                var columnName = columnExpression.Name;
                var columnType = columnExpression.Type;
                var script = DatabaseTableGenerator.GetAddColumnScript(tableType, columnName, columnType);
                transaction.Manager.AsSpruceQueryManager().Do(script, null);
            }

            public static void DropColumn<T>(string columnName, ISpruceTransaction transaction)
            {
                var tableType = typeof(T);
                var script = DatabaseTableGenerator.GetDropColumnScript(tableType, columnName);
                transaction.Manager.AsSpruceQueryManager().Do(script, null);
            }

            public static void AlterColumn<T>(Expression<Action<T, object>> columnExpression, ISpruceTransaction transaction)
            {
                var tableType = typeof(T);
                var columnName = columnExpression.Name;
                var columnType = columnExpression.Type;
                var script = DatabaseTableGenerator.GetAlterColumnScript(tableType, columnName, columnType);
                transaction.Manager.AsSpruceQueryManager().Do(script, null);
            }

        }
    }
}