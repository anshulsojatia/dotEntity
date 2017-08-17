// #region Author Information
// // SqlDatabasePersistanceTest.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using DotEntity.Enumerations;
using DotEntity.Sqlite;
using DotEntity.Tests.Data;

namespace DotEntity.Tests.PersistanceTests
{
    [TestFixture]
    public class SqliteDatabasePersistanceTests : DotEntityTest
    {
        [OneTimeSetUp]
        public void Init()
        {
            DotEntityDb.Initialize(SqliteConnectionString, new SqliteDatabaseProvider());
            var createProduct = @"CREATE TABLE Product
(     Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
     ProductName TEXT NOT NULL,
     ProductDescription TEXT NULL,
     DateCreated TEXT NOT NULL,
     Price NUMERIC NOT NULL,
     IsActive NUMERIC NOT NULL);";

            EntitySet.Query(createProduct, null);
        }

        [OneTimeTearDown]
        public void End()
        {
            EntitySet.Query("DROP TABLE Product;", null);
        }

        [Test]
        public void SqliteInsert_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "SqliteInsert_Succeeds",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            EntitySet<Product>.Insert(product);

            var savedProduct = EntitySet<Product>.Where(x => x.ProductName == "SqliteInsert_Succeeds").SelectSingle();
            Assert.AreEqual(product.Id, savedProduct.Id);
        }

        [Test]
        public void Sqlite_Select_Simple_Succeeds()
        {
            var productCount = 5;
            var products = new List<Product>();
            for (var i = 0; i < productCount; i++)
            {
                products.Add(new Product()
                {
                    ProductName = $"Sqlite_Select_Simple_Succeeds {i + 1}",
                    ProductDescription = $"Some descriptoin won't hurt {i + 1}",
                    DateCreated = DateTime.Now,
                    Price = 10
                });
                EntitySet<Product>.Insert(products[i]);
            }

            

            var savedProducts = EntitySet<Product>.Where(x => x.ProductName.Contains("Sqlite_Select_Simple_Succeeds"))
                .Select();

            Assert.AreEqual(productCount, savedProducts.Count());
        }

        [Test]
        public void Sqlite_Select_Count_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "Sqlite_Select_Count_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "Sqlite_Select_Count_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var cnt = EntitySet<Product>.Where(x => x.ProductName.Contains("Sqlite_Select_Count_Succeeds")).Count();

            Assert.AreEqual(2, cnt);
        }

        [Test]
        public void Sqlite_Select_With_Matching_Count_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "Sqlite_Select_With_Matching_Count_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "Sqlite_Select_With_Matching_Count_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var products = EntitySet<Product>
                .Where(x => x.ProductName.Contains("Sqlite_Select_With_Matching_Count_Succeeds"))
                .OrderBy(x => x.Id)
                .SelectWithTotalMatches(out int totalMatches, page: 1, count: 1);

            Assert.AreEqual(1, products.Count());
            Assert.AreEqual(2, totalMatches);
        }

        [Test]
        public void Sqlite_SingleQuery_Succeeds()
        {
            var productCount = 5;
            var ps = new List<Product>();
            for (var i = 0; i < productCount; i++)
            {
                ps.Add(new Product()
                {
                    ProductName = $"Sqlite_SingleQuery_Succeeds {i + 1}",
                    ProductDescription = $"Some descriptoin won't hurt {i + 1}",
                    DateCreated = DateTime.Now,
                    Price = 10
                });
                EntitySet<Product>.Insert(ps[i]);
            }

            var products = EntitySet<Product>.Query(@"SELECT * FROM PRODUCT WHERE ProductName LIKE @ProductName;",
                new {ProductName = "%Sqlite_SingleQuery_Succeeds%"}).ToList();

            Assert.AreEqual(productCount, products.Count());
        }

        [Test]
        public void Sqlite_Update_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "SqliteUpdate_Succeeds before",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            EntitySet<Product>.Insert(product);
            var id = product.Id;

            product.ProductName = "SqliteUpdate_Succeeds after";
            product.Price = 50;
            EntitySet<Product>.Update(product);

            var savedProduct = EntitySet<Product>.Where(x => x.Id == id).SelectSingle();
            Assert.AreEqual("SqliteUpdate_Succeeds after", savedProduct.ProductName);
            Assert.AreEqual(50, savedProduct.Price);
        }

        [Test]
        public void Sqlite_Update_Dynamic_Fields_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "Sqlite_Select_Simple_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "Sqlite_Select_Simple_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var id = product2.Id;
            EntitySet<Product>.Update(new {ProductName = "Sqlite_Select_Simple_Succeeds Two Modified", Price = 50}, x => x.Id == id);

            var savedObject = EntitySet<Product>.Where(x => x.Id == id).SelectSingle();

            Assert.AreEqual("Sqlite_Select_Simple_Succeeds Two Modified", savedObject.ProductName);
            Assert.AreEqual(50, savedObject.Price);
        }

        [Test]
        public void Sqlite_Delete_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "Sqlite_Delete_Succeeds",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            EntitySet<Product>.Insert(product);
            var id = product.Id;
            EntitySet<Product>.Delete(x => x.Id == id);

            var deletedProduct = EntitySet<Product>.Where(x => x.Id == id).SelectSingle();
            Assert.AreEqual(null, deletedProduct);
        }
    }
}