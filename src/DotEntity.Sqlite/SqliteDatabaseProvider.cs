using System.Data;
#if NETSTANDARD15
using Microsoft.Data.Sqlite;
#else
using System.Data.SQLite;
#endif
using System.Linq;
using DotEntity.Providers;
using DotEntity.Sqlite.Internals.System;

namespace DotEntity.Sqlite
{
    public class SqliteDatabaseProvider : IDatabaseProvider
    {
#if NETSTANDARD15
        public IDbConnection Connection => new SqliteConnection(DotEntityDb.ConnectionString);
#else
        public IDbConnection Connection => new SQLiteConnection(DotEntityDb.ConnectionString);
#endif
        public string ProviderName => "Microsoft.Data.Sqlite";

        public string DatabaseName { get; }

        public bool IsDatabaseVersioned(string versionTableName)
        {
            var t = EntitySet<SqliteMaster>
                .Where(x => x.Tbl_Name == versionTableName && x.Type == "table")
                .Select();
            return t.Any();
        }

        private IDatabaseTableGenerator _databaseTableGenerator;
        public IDatabaseTableGenerator DatabaseTableGenerator
        {
            get
            {
                _databaseTableGenerator = _databaseTableGenerator ?? new SqliteTableGenerator();
                return _databaseTableGenerator;
            }
            set => _databaseTableGenerator = value;
        }

        public IQueryGenerator QueryGenerator { get; set; }

        public ITypeMapProvider TypeMapProvider { get; set; }

        public int MaximumParametersPerQuery { get; set; }

        public SqliteDatabaseProvider()
        {
            QueryGenerator = new SqliteQueryGenerator();
            TypeMapProvider = new SqliteTypeMapProvider();
            DotEntityDb.MapTableNameForType<SqliteMaster>("sqlite_master");
        }
    }
}