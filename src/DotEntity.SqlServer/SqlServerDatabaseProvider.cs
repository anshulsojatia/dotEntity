using System.Data;
using System.Data.SqlClient;
using DotEntity.Providers;

namespace DotEntity.SqlServer
{
    public class SqlServerDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection Connection => new SqlConnection(DotEntityDb.ConnectionString);

        public string ProviderName => "System.Data.SqlClient";

        public string DatabaseName { get; } //not required here

        public IQueryGenerator QueryGenerator { get; set; }

        public ITypeMapProvider TypeMapProvider { get; set; }

        private int _maximumParameterPerQuery = 2100;

        public int MaximumParametersPerQuery
        {
            get => 2100;
            set => _maximumParameterPerQuery = value;
        }
    }
}
