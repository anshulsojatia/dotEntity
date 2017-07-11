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
using DotEntity.SqlServer;
using DotEntity.Tests.Data;
using DotEntity.Versioning;

namespace DotEntity.Tests
{
    [TestFixture]
    public class SqlDatabasePersistanceTests
    {
        [OneTimeSetUp]
        public void Init()
        {
            DotEntityDb.Initialize(
                @"Data Source=.\sqlexpress;Initial Catalog=ms;Integrated Security=False;Persist Security Info=False;User ID=iis_user;Password=iis_user",
                new SqlServerDatabaseProvider());

            DotEntityDb.UpdateDatabaseToLatestVersion();
        }

        [OneTimeTearDown]
        public void End()
        {
            DotEntityDb.UpdateDatabaseToVersion(null);
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
            EntitySet<Product>.Insert(product);

            var savedProduct = EntitySet<Product>.Where(x => x.ProductName == "SqlServerInsert_Succeeds").SelectSingle();
            Assert.AreEqual(product.Id, savedProduct.Id);
        }

        [Test]
        public void SqlServer_Select_Simple_Succeeds()
        {
            var productCount = 500;
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
            }

            EntitySet<Product>.Insert(products.ToArray());

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

            EntitySet<Product>.Insert(new[] { product1, product2 });

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

            EntitySet<Product>.Insert(new[] { product1, product2 });

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
            var productCount = 500;
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
            }

            EntitySet<Product>.Insert(ps.ToArray());

            var products = EntitySet<Product>.Query(@"SELECT * FROM PRODUCT WHERE ProductName LIKE '%' + @ProductName + '%';",
                new {ProductName = "SqlServer_SingleQuery_Succeeds"}).ToList();

            Assert.AreEqual(productCount, products.Count());
        }


        [Test]
        public void SqlServer_MultiQuery_Succeeds()
        {
            var product1 = new Product()
            {
                ProductName = "SqlServer_MultiQuery_Succeeds One",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 10
            };

            var product2 = new Product()
            {
                ProductName = "SqlServer_MultiQuery_Succeeds Two",
                ProductDescription = "Some descriptoin won't hurt",
                DateCreated = DateTime.Now,
                Price = 20
            };

            EntitySet<Product>.Insert(new[] { product1, product2 });

            IEnumerable<Product> products;
            var totalMatches = 0;
            using (var result = EntitySet.Query(@"SELECT * FROM PRODUCT WHERE ProductName LIKE '%' + @ProductName + '%';
                        SELECT COUNT(*) FROM PRODUCT WHERE ProductName LIKE '%' + @ProductName + '%';", new { ProductName = "SqlServer_MultiQuery_Succeeds" }))
            {
                products = result.SelectAllAs<Product>();
                totalMatches = result.SelectScalerAs<int>();
            }

            Assert.AreEqual(2, products.Count());
            Assert.AreEqual(2, totalMatches);
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

            EntitySet<Product>.Insert(new[] { product1, product2, product3 });
            EntitySet<Category>.Insert(categories);

            var productCategories = new[]
            {
                new ProductCategory() {CategoryId = categories[0].Id, ProductId = product1.Id},
                new ProductCategory() {CategoryId = categories[1].Id, ProductId = product2.Id},
                new ProductCategory() {CategoryId = categories[2].Id, ProductId = product2.Id},
            };
            EntitySet<ProductCategory>.Insert(productCategories);

            var products = EntitySet<Product>
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

            EntitySet<Product>.Insert(new[] { product1, product2 });
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
        public class DbVersion : IDatabaseVersion
        {
            public string VersionKey => "TestKey";
            public void Upgrade(IDotEntityTransaction transaction)
            {
                DotEntity.Database.CreateTables(new[]
                {
                    typeof(Product),
                    typeof(Category),
                    typeof(ProductCategory)
                }, transaction);

                DotEntity.Database.CreateConstraint(Relation.Create<Product, ProductCategory>("Id", "ProductId"), transaction);
                DotEntity.Database.CreateConstraint(Relation.Create<Category, ProductCategory>("Id", "CategoryId"), transaction);
            }

            public void Downgrade(IDotEntityTransaction transaction)
            {
                DotEntity.Database.DropConstraint(Relation.Create<Product, ProductCategory>("Id", "ProductId"), transaction);
                DotEntity.Database.DropConstraint(Relation.Create<Category, ProductCategory>("Id", "CategoryId"), transaction);
                DotEntity.Database.DropTable<ProductCategory>(transaction);
                DotEntity.Database.DropTable<Product>(transaction);
                DotEntity.Database.DropTable<Category>(transaction);
            }
        }
    }
}