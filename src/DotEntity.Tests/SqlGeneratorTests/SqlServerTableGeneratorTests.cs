// #region Author Information
// // SqlServerTableGeneratorTests.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using DotEntity.SqlServer;
using NUnit.Framework;
using DotEntity.Tests.Data;

namespace DotEntity.Tests.SqlGeneratorTests
{
    [TestFixture]
    public class SqlServerTableGeneratorTests : DotEntityTest
    {
        [OneTimeSetUp]
        public void Init()
        {
            DotEntityDb.Initialize(MsSqlConnectionString, new SqlServerDatabaseProvider());
        }

        [Test]
        public void CreateTable_Succeeds()
        {
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetCreateTableScript<Product>();
            var expected = @"CREATE TABLE [Product]" + Environment.NewLine +
                           "(\t [Id] INT NOT NULL IDENTITY(1,1)," + Environment.NewLine +
                           "\t [ProductName] NVARCHAR(MAX) NOT NULL," + Environment.NewLine +
                           "\t [ProductDescription] NVARCHAR(MAX) NULL," + Environment.NewLine +
                           "\t [DateCreated] DATETIME NOT NULL," + Environment.NewLine +
                           "\t [Price] NUMERIC(18,5) NOT NULL," + Environment.NewLine +
                           "\t [IsActive] BIT NOT NULL," + Environment.NewLine +
                           "PRIMARY KEY CLUSTERED ([Id] ASC));";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void CreateTable_WithCustom_Prefix_Succeeds()
        {
            DotEntityDb.GlobalTableNamePrefix = "app_";
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetCreateTableScript<Product>();
            var expected = @"CREATE TABLE [app_Product]" + Environment.NewLine +
                           "(\t [Id] INT NOT NULL IDENTITY(1,1)," + Environment.NewLine +
                           "\t [ProductName] NVARCHAR(MAX) NOT NULL," + Environment.NewLine +
                           "\t [ProductDescription] NVARCHAR(MAX) NULL," + Environment.NewLine +
                           "\t [DateCreated] DATETIME NOT NULL," + Environment.NewLine +
                           "\t [Price] NUMERIC(18,5) NOT NULL," + Environment.NewLine +
                           "\t [IsActive] BIT NOT NULL," + Environment.NewLine +
                           "PRIMARY KEY CLUSTERED ([Id] ASC));";
            Assert.AreEqual(expected, sql);

            sql = generator.GetDropTableScript<Product>();
            expected = @"IF OBJECT_ID('app_Product', 'U') IS NOT NULL DROP TABLE [app_Product];";
            Assert.AreEqual(expected, sql);

            DotEntityDb.GlobalTableNamePrefix = "";
        }

        [Test]
        public void DropTable_Succeeds()
        {
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetDropTableScript<Product>();
            var expected = @"IF OBJECT_ID('Product', 'U') IS NOT NULL DROP TABLE [Product];";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void CreateForeignKeyConstraint_Succeeds()
        {
            var relation = new Relation {
                SourceType = typeof(Product),
                DestinationType = typeof(ProductCategory),
                SourceColumnName = "Id",
                DestinationColumnName = "ProductId"
            };
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetCreateConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory]" + Environment.NewLine +
                           "ADD CONSTRAINT [FK_Product_Id_ProductCategory_ProductId]" + Environment.NewLine +
                           "FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]);";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DeleteForeignKeyConstraint_Succeeds()
        {
            var relation = new Relation {
                SourceType = typeof(Product),
                DestinationType = typeof(ProductCategory),
                SourceColumnName = "Id",
                DestinationColumnName = "ProductId"
            };
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetDropConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory]" + Environment.NewLine +
                           "DROP CONSTRAINT [FK_Product_Id_ProductCategory_ProductId];";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void CreateIndex_Succeeds()
        {
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetCreateIndexScript<Product>(new[] {nameof(Product.DateCreated)});
            var expected = "CREATE INDEX Idx_DateCreated ON [Product] ([DateCreated])";
            Assert.AreEqual(expected, sql);

            sql = generator.GetCreateIndexScript<Product>(new[] { nameof(Product.DateCreated) }, true);
            expected = "CREATE UNIQUE INDEX Idx_DateCreated ON [Product] ([DateCreated])";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DropIndex_Succeeds()
        {
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetDropIndexScript<Product>(new[] { nameof(Product.DateCreated) });
            var expected = "DROP INDEX [Product].Idx_DateCreated";
            Assert.AreEqual(expected, sql);

        }
    }
}