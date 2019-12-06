using System;
using System.IO;
#if NETSTANDARD15
using Microsoft.DotNet.PlatformAbstractions;
#endif
using System.Reflection;

namespace DotEntity.Tests
{
    public class DotEntityTest
    {
        protected bool IsAppVeyor;
        protected bool IsDeploymentServer;
        protected string MySqlConnectionString;
        protected string MsSqlConnectionString;
        protected string SqliteConnectionString;
        protected const string ContextKey = "DotEntity.Tests.PersistanceTests";
        private readonly string _sqliteFile;
        public DotEntityTest()
        {
#if NETSTANDARD15
            _sqliteFile = ApplicationEnvironment.ApplicationBasePath + @"\TestDb\sqlite.db";
#else
            _sqliteFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"\TestDb\sqlite.db");

#endif
            IsAppVeyor = Environment.GetEnvironmentVariable("appveyor") == "true";
            IsDeploymentServer = Environment.GetEnvironmentVariable("env.dotentity_deployment") == "true";
            MySqlConnectionString = this.IsAppVeyor
                ? @"Server=127.0.0.1;Uid=root;Pwd=Password12!;Database=mytest;"
                : @"Server=127.0.0.1;Uid=root;Pwd=admin;Database=unittest;";

            MsSqlConnectionString = IsAppVeyor
                ? @"Server=(local)\SQL2016;Database=master;User ID=sa;Password=Password12!"
                : IsDeploymentServer
                    ? @"Data Source=.\SqlExpress;Initial Catalog=unittest_db;Integrated Security=True;"
                    : @"Data Source=.;Initial Catalog=unittest_db;Integrated Security=True;Persist Security Info=False;User ID=iis_user;Password=iis_user";

            
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