#if NET451 || NETSTANDARD15
using DotEntity.Sqlite;
using NUnit.Framework;
using DotEntity.Tests.Data;

namespace DotEntity.Tests.SqlGeneratorTests
{
    [TestFixture]
    public class SqliteTableGeneratorTests : DotEntityTest
    {
        [OneTimeSetUp]
        public void Init()
        {
            DotEntityDb.Initialize(SqliteConnectionString, new SqliteDatabaseProvider());
        }

        [Test]
        public void CreateTable_Succeeds()
        {
            var generator = new SqliteTableGenerator();
            var sql = generator.GetCreateTableScript<Product>();
            var expected = @"CREATE TABLE [Product]
(	 [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	 [ProductName] TEXT NOT NULL,
	 [ProductDescription] TEXT NULL,
	 [DateCreated] TEXT NOT NULL,
	 [Price] NUMERIC NOT NULL,
	 [IsActive] NUMERIC NOT NULL);";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DropTable_Succeeds()
        {
            var generator = new SqliteTableGenerator();
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
            var generator = new SqliteTableGenerator();
            var sql = generator.GetCreateConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory] RENAME TO [ProductCategory_temporary];
CREATE TABLE [ProductCategory]
(	 [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	 [ProductId] INTEGER NOT NULL,
	 [CategoryId] INTEGER NOT NULL,
FOREIGN KEY([ProductId]) REFERENCES [Product]([Id]));
INSERT INTO [ProductCategory] SELECT * FROM [ProductCategory_temporary];
DROP TABLE [ProductCategory_temporary];";

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
            var generator = new SqliteTableGenerator();
            var sql = generator.GetDropConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory] RENAME TO [ProductCategory_temporary];
CREATE TABLE [ProductCategory]
(	 [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	 [ProductId] INTEGER NOT NULL,
	 [CategoryId] INTEGER NOT NULL);
INSERT INTO [ProductCategory] SELECT * FROM [ProductCategory_temporary];
DROP TABLE [ProductCategory_temporary];";
            Assert.AreEqual(expected, sql);
        }
    }
}
#endif