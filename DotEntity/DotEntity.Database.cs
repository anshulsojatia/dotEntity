// #region Author Information
// // DotEntity.Database.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Linq.Expressions;
using DotEntity.Extensions;

namespace DotEntity
{
    public partial class DotEntity
    {
        public static class Database
        {
            private static readonly IDatabaseTableGenerator DatabaseTableGenerator = DotEntityDb.Provider.DatabaseTableGenerator;

            public static void CreateTable<T>(IDotEntityTransaction transaction) where T : class
            {
                var script = DatabaseTableGenerator.GetCreateTableScript<T>();
                transaction.Manager.AsDotEntityQueryManager().Do(script, null);
            }

            public static void CreateTables(Type[] tableTypes, IDotEntityTransaction transaction)
            {
                foreach (var tableType in tableTypes)
                {
                    var script = DatabaseTableGenerator.GetCreateTableScript(tableType);
                    transaction.Manager.AsDotEntityQueryManager().Do(script, null);
                }
            }

            public static void DropTable<T>(IDotEntityTransaction transaction) where T : class
            {
                var script = DatabaseTableGenerator.GetDropTableScript<T>();
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

            public static void AddColumn<T>(Expression<Action<T, object>> columnExpression, IDotEntityTransaction transaction)
            {
                var tableType = typeof(T);
                var columnName = columnExpression.Name;
                var columnType = columnExpression.Type;
                var script = DatabaseTableGenerator.GetAddColumnScript(tableType, columnName, columnType);
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

        }
    }
}