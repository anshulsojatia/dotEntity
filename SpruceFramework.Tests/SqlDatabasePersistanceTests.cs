// #region Author Information
// // SqlDatabasePersistanceTest.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using NUnit.Framework;
using SpruceFramework.Providers;
using SpruceFramework.Tests.Data;

namespace SpruceFramework.Tests
{
    [TestFixture]
    public class SqlDatabasePersistanceTests
    {
        public SqlDatabasePersistanceTests()
        {
            Spruce.Initialize(
                @"Data Source=.\sqlexpress;Initial Catalog=spruce_framework;Integrated Security=False;Persist Security Info=False;User ID=iis_user;Password=iis_user",
                new SqlServerDatabaseProvider());

        }

        [Test]
        public void SqlServerInsert_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "My first product",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            SpruceTable<Product>.Insert(product);
        }

        [Test]
        public void SqlServer_Select_Simple_Succeeds()
        {
            var products1 = SpruceTable<Product>.Where(x => true).Select();
            var products2 = SpruceTable<Product>.Where(x => true).Select();
        }
    }
}