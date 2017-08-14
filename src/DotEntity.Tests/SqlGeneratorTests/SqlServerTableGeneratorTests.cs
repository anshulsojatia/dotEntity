// #region Author Information
// // SqlServerTableGeneratorTests.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

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
            var expected = @"CREATE TABLE [Product]
(	 [Id] INT NOT NULL IDENTITY(1,1),
	 [ProductName] NVARCHAR(MAX) NOT NULL,
	 [ProductDescription] NVARCHAR(MAX) NOT NULL,
	 [DateCreated] DATETIME NOT NULL,
	 [Price] NUMERIC(18,0) NOT NULL,
	 [IsActive] BIT NOT NULL,
PRIMARY KEY CLUSTERED ([Id] ASC));";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DropTable_Succeeds()
        {
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetDropTableScript<Product>();
            var expected = @"DROP TABLE [Product];";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void CreateForeignKeyConstraint_Succeeds()
        {
            var relation = new Relation
            {
                SourceType = typeof(Product),
                DestinationType = typeof(ProductCategory),
                SourceColumnName = "Id",
                DestinationColumnName = "ProductId"
            };
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetCreateConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory]
ADD CONSTRAINT [FK_Product_Id_ProductCategory_ProductId]
FOREIGN KEY ([ProductId]) REFERENCES [Product]([Id]);";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DeleteForeignKeyConstraint_Succeeds()
        {
            var relation = new Relation
            {
                SourceType = typeof(Product),
                DestinationType = typeof(ProductCategory),
                SourceColumnName = "Id",
                DestinationColumnName = "ProductId"
            };
            var generator = new DefaultDatabaseTableGenerator();
            var sql = generator.GetDropConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory]
DROP CONSTRAINT [FK_Product_Id_ProductCategory_ProductId];";
            Assert.AreEqual(expected, sql);
        }
    }
}