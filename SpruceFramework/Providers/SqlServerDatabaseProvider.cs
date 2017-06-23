// #region Author Information
// // SqlServerDatabaseProvider.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.Data;
using System.Data.SqlClient;

namespace SpruceFramework.Providers
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection Connection => new SqlConnection(Spruce.ConnectionString);

        public string ProviderName => "System.Data.SqlClient";
    }
}