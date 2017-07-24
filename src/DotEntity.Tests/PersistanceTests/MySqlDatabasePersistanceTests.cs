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
using DotEntity.MySql;
using DotEntity.Tests.Data;

namespace DotEntity.Tests.PersistanceTests
{
    [TestFixture]
    public class MySqlDatabasePersistanceTests : DotEntityTest
    {
        [OneTimeSetUp]
        public void Init()
        {
            DotEntityDb.Initialize(MySqlConnectionString, new MySqlDatabaseProvider("mytest"));

            var createProduct = @"CREATE TABLE Product
(     Id INT NOT NULL AUTO_INCREMENT,
     ProductName TEXT NOT NULL,
     ProductDescription TEXT NOT NULL,
     DateCreated DATETIME NOT NULL,
     Price NUMERIC(18,0) NOT NULL,
     IsActive TINYINT(1) NOT NULL,
PRIMARY KEY (Id));";


            EntitySet.Query(createProduct, null);
        }

        [OneTimeTearDown]
        public void End()
        {
            EntitySet.Query("DROP TABLE Product;", null);
        }

        [Test]
        public void MySqlInsert_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "MySqlInsert_Succeeds",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            EntitySet<Product>.Insert(product);

            var savedProduct = EntitySet<Product>.Where(x => x.ProductName == "MySqlInsert_Succeeds").SelectSingle();
            Assert.AreEqual(product.Id, savedProduct.Id);
        }

        [Test]
        public void MySql_Select_Simple_Succeeds()
        {
            var productCount = 5;
            var products = new List<Product>();
            for (var i = 0; i < productCount; i++)
            {
                products.Add(new Product()
                {
                    ProductName = $"MySql_Select_Simple_Succeeds {i + 1}",
                    ProductDescription = $"Some descriptoin won't hurt {i + 1}",
                    DateCreated = DateTime.Now,
                    Price = 10
                });
                EntitySet<Product>.Insert(products[i]);
            }

            var savedProducts = EntitySet<Product>.Where(x => x.ProductName.Contains("MySql_Select_Simple_Succeeds"))
                .Select();

            Assert.AreEqual(productCount, savedProducts.Count());
        }

        [Test]
        public void MySql_Select_Count_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "MySql_Select_Count_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "MySql_Select_Count_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var cnt = EntitySet<Product>.Where(x => x.ProductName.Contains("MySql_Select_Count_Succeeds")).Count();

            Assert.AreEqual(2, cnt);
        }

        [Test]
        public void MySql_Select_With_Matching_Count_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "MySql_Select_With_Matching_Count_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "MySql_Select_With_Matching_Count_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var products = EntitySet<Product>
                .Where(x => x.ProductName.Contains("MySql_Select_With_Matching_Count_Succeeds"))
                .OrderBy(x => x.Id)
                .SelectWithTotalMatches(out int totalMatches, page: 1, count: 1);

            Assert.AreEqual(1, products.Count());
            Assert.AreEqual(2, totalMatches);
        }

        [Test]
        public void MySql_SingleQuery_Succeeds()
        {
            var productCount = 5;
            var ps = new List<Product>();
            for (var i = 0; i < productCount; i++)
            {
                ps.Add(new Product()
                {
                    ProductName = $"MySql_SingleQuery_Succeeds {i + 1}",
                    ProductDescription = $"Some descriptoin won't hurt {i + 1}",
                    DateCreated = DateTime.Now,
                    Price = 10
                });
                EntitySet<Product>.Insert(ps[i]);
            }
            var products = EntitySet<Product>.Query(@"SELECT * FROM PRODUCT WHERE ProductName LIKE @ProductName;",
                new {ProductName = "%MySql_SingleQuery_Succeeds%"}).ToList();

            Assert.AreEqual(productCount, products.Count());
        }


        [Test]
        public void MySql_Update_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "MySqlUpdate_Succeeds before",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            EntitySet<Product>.Insert(product);
            var id = product.Id;

            product.ProductName = "MySqlUpdate_Succeeds after";
            product.Price = 50;
            EntitySet<Product>.Update(product);

            var savedProduct = EntitySet<Product>.Where(x => x.Id == id).SelectSingle();
            Assert.AreEqual("MySqlUpdate_Succeeds after", savedProduct.ProductName);
            Assert.AreEqual(50, savedProduct.Price);
        }

        [Test]
        public void MySql_Update_Dynamic_Fields_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "MySql_Select_Simple_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "MySql_Select_Simple_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };


            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var id = product2.Id;
            EntitySet<Product>.Update(new {ProductName = "MySql_Select_Simple_Succeeds Two Modified", Price = 50}, x => x.Id == id);

            var savedObject = EntitySet<Product>.Where(x => x.Id == id).SelectSingle();

            Assert.AreEqual("MySql_Select_Simple_Succeeds Two Modified", savedObject.ProductName);
            Assert.AreEqual(50, savedObject.Price);
        }

        [Test]
        public void MySql_Delete_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "MySql_Delete_Succeeds",
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