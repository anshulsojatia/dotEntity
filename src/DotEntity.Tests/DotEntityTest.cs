using System;

namespace DotEntity.Tests
{
    public class DotEntityTest
    {
        protected bool IsAppVeyor;
        protected string MySqlConnectionString;
        protected string MsSqlConnectionString;

        public DotEntityTest()
        {
            IsAppVeyor = Environment.GetEnvironmentVariable("appveyor") == "true";

            MySqlConnectionString = this.IsAppVeyor
                ? @"Server=127.0.0.1;Uid=root;Pwd=Password12!;Database=mytest;"
                : @"Server=127.0.0.1;Uid=root;Pwd=admin;Database=mytest;";

            MsSqlConnectionString = IsAppVeyor ? @"Server=(local)\SQL2016;Database=master;User ID=sa;Password=Password12!"
                : @"Data Source=.\sqlexpress;Initial Catalog=ms;Integrated Security=False;Persist Security Info=False;User ID=iis_user;Password=iis_user";
        }
    }
}