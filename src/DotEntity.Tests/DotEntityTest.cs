using System;
using System.IO;
using System.Reflection;
using Microsoft.DotNet.PlatformAbstractions;

namespace DotEntity.Tests
{
    public class DotEntityTest
    {
        protected bool IsAppVeyor;
        protected bool IsDeploymentServer;
        protected string MySqlConnectionString;
        protected string MsSqlConnectionString;
        protected string SqliteConnectionString;
        protected string PostgreSqlConnectionString;
        protected const string ContextKey = "DotEntity.Tests.PersistanceTests";
        private readonly string _sqliteFile;
        public DotEntityTest()
        {
            _sqliteFile = ApplicationEnvironment.ApplicationBasePath + @"\TestDb\sqlite.db";
            IsAppVeyor = Environment.GetEnvironmentVariable("appveyor") == "true";
            IsDeploymentServer = Environment.GetEnvironmentVariable("env.dotentity_deployment") == "true";
            MySqlConnectionString = this.IsAppVeyor
                ? @"Server=127.0.0.1;Uid=root;Pwd=Password12!;Database=mytest;"
                : @"Server=127.0.0.1;Uid=root;Pwd=admin;Database=unittest;";

            MsSqlConnectionString = IsAppVeyor
                ? @"Server=(local)\SQL2017;Database=master;User ID=sa;Password=Password12!"
                : IsDeploymentServer
                    ? @"Data Source=.\SqlExpress;Initial Catalog=unittest_db;Integrated Security=True;"
                    : @"Data Source=.;Initial Catalog=unittest_db;Integrated Security=True;Persist Security Info=False;User ID=iis_user;Password=iis_user";

            PostgreSqlConnectionString = this.IsAppVeyor
                ? @"host=127.0.0.1;user id=postgres;password=Password12!;database=mytest;"
                : @"host=127.0.0.1;user id=postgres;password=admin;database=testdb;";

            SqliteConnectionString = $"Data Source={_sqliteFile};";
        }

        public void CreateSqliteFile()
        {
           
        }

        public void DeleteSqliteFile()
        {
            
        }
    }
}