// #region Author Information
// // MySqlTableGeneratorTests.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using DotEntity.MySql;
using NUnit.Framework;
using DotEntity.Tests.Data;

namespace DotEntity.Tests.SqlGeneratorTests
{
    [TestFixture]
    public class MySqlTableGeneratorTests : DotEntityTest
    {
        private IDatabaseTableGenerator generator;
        [OneTimeSetUp]
        public void Init()
        {
            DotEntityDb.Initialize(MySqlConnectionString, new MySqlDatabaseProvider("mytest"));
            generator = DotEntityDb.Provider.DatabaseTableGenerator;
        }

        [Test]
        public void CreateTable_Succeeds()
        {
            var sql = generator.GetCreateTableScript<Product>();
            var expected = @"CREATE TABLE Product
(	 Id INT NOT NULL AUTO_INCREMENT,
	 ProductName TEXT NOT NULL,
	 ProductDescription TEXT NOT NULL,
	 DateCreated DATETIME NOT NULL,
	 Price NUMERIC(18,0) NOT NULL,
	 IsActive TINYINT(1) NOT NULL,
PRIMARY KEY (Id));";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DropTable_Succeeds()
        {
            var sql = generator.GetDropTableScript<Product>();
            var expected = @"DROP TABLE Product;";
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
            var sql = generator.GetCreateConstraintScript(relation);
            var expected = @"ALTER TABLE ProductCategory
ADD CONSTRAINT FK_Product_Id_ProductCategory_ProductId
FOREIGN KEY (ProductId) REFERENCES Product(Id);";

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
            var sql = generator.GetDropConstraintScript(relation);
            var expected = @"ALTER TABLE ProductCategory
DROP FOREIGN KEY FK_Product_Id_ProductCategory_ProductId;";
            Assert.AreEqual(expected, sql);
        }
    }
}