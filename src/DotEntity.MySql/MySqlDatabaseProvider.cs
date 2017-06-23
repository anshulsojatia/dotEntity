using System.Data;
using DotEntity.Providers;
using MySql.Data.MySqlClient;

namespace DotEntity.MySql
{
    public class MySqlDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection Connection => new MySqlConnection(DotEntityDb.ConnectionString);

        public string ProviderName => "MySql.Data";

        public string DatabaseName { get; }

        public MySqlDatabaseProvider(string databaseName)
        {
            DatabaseName = databaseName;
            QueryGenerator = new MySqlQueryGenerator();
            TypeMapProvider = new MySqlTypeMapProvider();
        }

        public IQueryGenerator QueryGenerator { get; set; }
        public ITypeMapProvider TypeMapProvider { get; set; }
        public int MaximumParametersPerQuery { get; set; }
    }
}
