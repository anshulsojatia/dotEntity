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
using DotEntity.SqlServer;
using DotEntity.Tests.Data;

namespace DotEntity.Tests.PersistanceTests
{
    [TestFixture]
    public class SqlDatabasePersistanceTests : DotEntityTest
    {
        [OneTimeSetUp]
        public void Init()
        {
            DotEntityDb.Initialize(MsSqlConnectionString,
                new SqlServerDatabaseProvider());
            var createProduct = @"CREATE TABLE Product
(     Id INT NOT NULL IDENTITY(1,1),
     ProductName NVARCHAR(MAX) NOT NULL,
     ProductDescription NVARCHAR(MAX) NULL,
     DateCreated DATETIME NOT NULL,
     Price NUMERIC(18,0) NOT NULL,
     IsActive BIT NOT NULL,
PRIMARY KEY CLUSTERED (Id ASC));";

            EntitySet.Query(createProduct, null);

        }

        [OneTimeTearDown]
        public void End()
        {
            EntitySet.Query("DROP TABLE Product;", null);
        }

        [Test]
        public void SqlServerInsert_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "SqlServerInsert_Succeeds",
                ProductDescription = null,
                DateCreated = DateTime.Now,
                Price = 25
            };
            EntitySet<Product>.Insert(product);

            var savedProduct = EntitySet<Product>.Where(x => x.ProductName == "SqlServerInsert_Succeeds").SelectSingle();
            Assert.AreEqual(product.Id, savedProduct.Id);
        }

        [Test]
        public void SqlServer_Select_Simple_Succeeds()
        {
            var productCount = 5;
            var products = new List<Product>();
            for (var i = 0; i < productCount; i++)
            {
                products.Add(new Product()
                {
                    ProductName = $"SqlServer_Select_Simple_Succeeds {i + 1}",
                    ProductDescription = $"Some descriptoin won't hurt {i + 1}",
                    DateCreated = DateTime.Now,
                    Price = 10
                });
                EntitySet<Product>.Insert(products[i]);
            }

            var savedProducts = EntitySet<Product>.Where(x => x.ProductName.Contains("SqlServer_Select_Simple_Succeeds"))
                .Select();

            Assert.AreEqual(productCount, savedProducts.Count());
        }

        [Test]
        public void SqlServer_Select_Count_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "SqlServer_Select_Count_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "SqlServer_Select_Count_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var cnt = EntitySet<Product>.Where(x => x.ProductName.Contains("SqlServer_Select_Count_Succeeds")).Count();

            Assert.AreEqual(2, cnt);
        }

        [Test]
        public void SqlServer_Select_With_Matching_Count_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "SqlServer_Select_With_Matching_Count_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "SqlServer_Select_With_Matching_Count_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);

            var products = EntitySet<Product>
                .Where(x => x.ProductName.Contains("SqlServer_Select_With_Matching_Count_Succeeds"))
                .OrderBy(x => x.Id)
                .SelectWithTotalMatches(out int totalMatches, page: 1, count: 1);

            Assert.AreEqual(1, products.Count());
            Assert.AreEqual(2, totalMatches);
        }

        [Test]
        public void SqlServer_SingleQuery_Succeeds()
        {
            var productCount = 5;
            var ps = new List<Product>();
            for (var i = 0; i < productCount; i++)
            {
                ps.Add(new Product()
                {
                    ProductName = $"SqlServer_SingleQuery_Succeeds {i + 1}",
                    ProductDescription = $"Some descriptoin won't hurt {i + 1}",
                    DateCreated = DateTime.Now,
                    Price = 10
                });
                EntitySet<Product>.Insert(ps[i]);
            }

            var products = EntitySet<Product>.Query(@"SELECT * FROM PRODUCT WHERE ProductName LIKE '%' + @ProductName + '%';",
                new {ProductName = "SqlServer_SingleQuery_Succeeds"}).ToList();

            Assert.AreEqual(productCount, products.Count());
        }

        [Test]
        public void SqlServer_Update_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "SqlServerUpdate_Succeeds before",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            EntitySet<Product>.Insert(product);
            var id = product.Id;

            product.ProductName = "SqlServerUpdate_Succeeds after";
            product.Price = 50;
            EntitySet<Product>.Update(product);

            var savedProduct = EntitySet<Product>.Where(x => x.Id == id).SelectSingle();
            Assert.AreEqual("SqlServerUpdate_Succeeds after", savedProduct.ProductName);
            Assert.AreEqual(50, savedProduct.Price);
        }

        [Test]
        public void SqlServer_Update_Dynamic_Fields_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "SqlServer_Select_Simple_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "SqlServer_Select_Simple_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(product1);
            EntitySet<Product>.Insert(product2);
            var id = product2.Id;
            EntitySet<Product>.Update(new {ProductName = "SqlServer_Select_Simple_Succeeds Two Modified", Price = 50}, x => x.Id == id);

            var savedObject = EntitySet<Product>.Where(x => x.Id == id).SelectSingle();

            Assert.AreEqual("SqlServer_Select_Simple_Succeeds Two Modified", savedObject.ProductName);
            Assert.AreEqual(50, savedObject.Price);
        }

        [Test]
        public void SqlServer_Delete_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "SqlServer_Delete_Succeeds",
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