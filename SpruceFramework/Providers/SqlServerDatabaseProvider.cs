// #region Author Information
// // SqlServerDatabaseProvider.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Data;
using System.Data.SqlClient;
using System.Linq;
using SpruceFramework.Internals.System;

namespace SpruceFramework.Providers
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection Connection => new SqlConnection(Spruce.ConnectionString);

        public string ProviderName => "System.Data.SqlClient";

        public bool IsDatabaseVersioned(string tableName)
        {
            var t = SpruceTable<InformationSchema.Tables>.Where(x => x.TABLE_NAME == tableName)
                .OrderBy(x => x.TABLE_CATALOG)
                .Select(1, 1);
            return t.Any();
        }

        public SqlServerDatabaseProvider()
        {
            DatabaseTableGenerator = new DefaultDatabaseTableGenerator();
            Spruce.MapTableNameForType<InformationSchema.Tables>("INFORMATION_SCHEMA.TABLES");
            Spruce.MapTableNameForType<InformationSchema.Columns>("INFORMATION_SCHEMA.COLUMNS");
        }

        public IDatabaseTableGenerator DatabaseTableGenerator { get; }
    }
}