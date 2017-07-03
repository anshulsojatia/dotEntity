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
using SpruceFramework.Enumerations;
using SpruceFramework.Providers;
using SpruceFramework.Tests.Data;
using SpruceFramework.Versioning;

namespace SpruceFramework.Tests
{
    [TestFixture]
    public class SqlDatabasePersistanceTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            Spruce.Initialize(
                @"Data Source=.\sqlexpress;Initial Catalog=ms;Integrated Security=False;Persist Security Info=False;User ID=iis_user;Password=iis_user",
                new SqlServerDatabaseProvider());

            Spruce.UpdateDatabaseToLatestVersion();
        }

        [OneTimeTearDown]
        public void End()
        {
            Spruce.UpdateDatabaseToVersion(null);
        }

        [Test]
        public void SqlServerInsert_Succeeds()
        {
            var product = new Product()
            {
                ProductName = "SqlServerInsert_Succeeds",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 25
            };
            SpruceTable<Product>.Insert(product);

            var savedProduct = SpruceTable<Product>.Where(x => x.ProductName == "SqlServerInsert_Succeeds").SelectSingle();
            Assert.AreEqual(product.Id, savedProduct.Id);
        }

        [Test]
        public void SqlServer_Select_Simple_Succeeds()
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

            SpruceTable<Product>.Insert(new [] { product1, product2});

            var products = SpruceTable<Product>.Where(x => x.ProductName.Contains("SqlServer_Select_Simple_Succeeds"))
                .Select();

            Assert.AreEqual(2, products.Count());
        }

        [Test]
        public void SqlServer_Select_Join_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "SqlServer_Select_Join_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "SqlServer_Select_Join_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            var product3 = new Product()
            {
                ProductName = "SqlServer_Select_Join_Succeeds Three",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };
            var categories = new []
            {
                new Category() { CategoryName = "First"},
                new Category() { CategoryName = "Second"},
                new Category() { CategoryName = "Third"},
            };

            SpruceTable<Product>.Insert(new[] { product1, product2, product3 });
            SpruceTable<Category>.Insert(categories);

            var productCategories = new[]
            {
                new ProductCategory() {CategoryId = categories[0].Id, ProductId = product1.Id},
                new ProductCategory() {CategoryId = categories[1].Id, ProductId = product2.Id},
                new ProductCategory() {CategoryId = categories[2].Id, ProductId = product2.Id},
            };
            SpruceTable<ProductCategory>.Insert(productCategories);

            var products = SpruceTable<Product>
                .Where(x => x.ProductName.Contains("SqlServer_Select_Join_Succeeds"))
                .Join<ProductCategory>("Id", "ProductId", joinType: JoinType.LeftOuter)
                .Join<Category>("CategoryId", "Id", joinType: JoinType.LeftOuter)
                .Relate<Category>((product, category) =>
                {
                   if(product.Categories == null)
                        product.Categories = new List<Category>();
                   product.Categories.Add(category);
                })
                .SelectNested().ToList();

            Assert.AreEqual(1, products[0].Categories.Count);
            Assert.AreEqual(2, products[1].Categories.Count);
            Assert.AreEqual(null, products[2].Categories);
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
            SpruceTable<Product>.Insert(product);
            var id = product.Id;

            product.ProductName = "SqlServerUpdate_Succeeds after";
            product.Price = 50;
            SpruceTable<Product>.Update(product);

            var savedProduct = SpruceTable<Product>.Where(x => x.Id == id).SelectSingle();
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

            SpruceTable<Product>.Insert(new[] { product1, product2 });
            var id = product2.Id;
            SpruceTable<Product>.Update(new {ProductName = "SqlServer_Select_Simple_Succeeds Two Modified", Price = 50}, x => x.Id == id);

            var savedObject = SpruceTable<Product>.Where(x => x.Id == id).SelectSingle();

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
            SpruceTable<Product>.Insert(product);
            var id = product.Id;
            SpruceTable<Product>.Delete(x => x.Id == id);

            var deletedProduct = SpruceTable<Product>.Where(x => x.Id == id).SelectSingle();
            Assert.AreEqual(null, deletedProduct);
        }
        public class DbVersion : IDatabaseVersion
        {
            public string VersionKey => "TestKey";
            public void Upgrade(ISpruceTransaction transaction)
            {
                Spruce.Database.CreateTables(new[]
                {
                    typeof(Product),
                    typeof(Category),
                    typeof(ProductCategory)
                }, transaction);

                Spruce.Database.CreateConstraint(Relation.Create<Product, ProductCategory>("Id", "ProductId"), transaction);
                Spruce.Database.CreateConstraint(Relation.Create<Category, ProductCategory>("Id", "CategoryId"), transaction);
            }

            public void Downgrade(ISpruceTransaction transaction)
            {
                Spruce.Database.DropConstraint(Relation.Create<Product, ProductCategory>("Id", "ProductId"), transaction);
                Spruce.Database.DropConstraint(Relation.Create<Category, ProductCategory>("Id", "CategoryId"), transaction);
                Spruce.Database.DropTable<ProductCategory>(transaction);
                Spruce.Database.DropTable<Product>(transaction);
                Spruce.Database.DropTable<Category>(transaction);
            }
        }
    }
}