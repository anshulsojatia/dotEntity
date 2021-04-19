using System.Data;
using DotEntity.Providers;
using System.Linq;
using DotEntity.PostgreSql.Internals.System;
using Npgsql;

namespace DotEntity.PostgreSql
{
    public class PostgreSqlDatabaseProvider : IDatabaseProvider
    {
        public IDbConnection Connection => new NpgsqlConnection(DotEntityDb.ConnectionString); 

        public string ProviderName => "Npgsql";

        public string DatabaseName { get; }

        public PostgreSqlDatabaseProvider() : this("")
        {
            
        }
        public PostgreSqlDatabaseProvider(string databaseName)
        {

            DatabaseName = databaseName;
            QueryGenerator = new PostgreSqlQueryGenerator();
            TypeMapProvider = new PostgreSqlTypeMapProvider();
            DotEntityDb.MapTableNameForType<InformationSchema.Tables>("INFORMATION_SCHEMA.TABLES");
            DotEntityDb.MapTableNameForType<InformationSchema.Columns>("INFORMATION_SCHEMA.COLUMNS");
        }

        public bool IsDatabaseVersioned(string versionTableName)
        {
            var databaseName = DatabaseName;
            if (string.IsNullOrEmpty(databaseName))
                databaseName = Connection.Database;

            var t = EntitySet<InformationSchema.Tables>
                .Where(x => x.table_name == versionTableName && x.table_catalog == databaseName && x.table_schema == "public")
                .Select();
            return t.Any();
        }

        private IDatabaseTableGenerator _databaseTableGenerator;
        public IDatabaseTableGenerator DatabaseTableGenerator
        {
            get
            {
                _databaseTableGenerator = _databaseTableGenerator ?? new PostgreSqlTableGenerator();
                return _databaseTableGenerator;
            }
            set => _databaseTableGenerator = value;
        }

        public IQueryGenerator QueryGenerator { get; set; }
        public ITypeMapProvider TypeMapProvider { get; set; }
        public int MaximumParametersPerQuery { get; set; }

        public string SafeEnclose(string objectName)
        {
            return $"\"{objectName}\"";
        }
    }
}
