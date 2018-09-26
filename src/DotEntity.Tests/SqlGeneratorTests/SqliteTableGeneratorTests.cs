#if NET451 || NETSTANDARD15
using System;
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
            var expected = @"CREATE TABLE [Product]" + Environment.NewLine +
                           "(\t [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + Environment.NewLine +
                           "\t [ProductName] TEXT NOT NULL," + Environment.NewLine +
                           "\t [ProductDescription] TEXT NULL," + Environment.NewLine +
                           "\t [DateCreated] TEXT NOT NULL," + Environment.NewLine +
                           "\t [Price] NUMERIC NOT NULL," + Environment.NewLine +
                           "\t [IsActive] NUMERIC NOT NULL);";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DropTable_Succeeds()
        {
            var generator = new SqliteTableGenerator();
            var sql = generator.GetDropTableScript<Product>();
            var expected = @"DROP TABLE IF EXISTS [Product];";
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
            var generator = new SqliteTableGenerator();
            var sql = generator.GetCreateConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory] RENAME TO [ProductCategory_temporary];" +
                           Environment.NewLine +
                           "CREATE TABLE [ProductCategory]" + Environment.NewLine +
                           "(\t [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + Environment.NewLine +
                           "\t [ProductId] INTEGER NOT NULL," + Environment.NewLine +
                           "\t [CategoryId] INTEGER NOT NULL," + Environment.NewLine +
                           "FOREIGN KEY([ProductId]) REFERENCES [Product]([Id]));" + Environment.NewLine +
                           "INSERT INTO [ProductCategory] SELECT * FROM [ProductCategory_temporary];" +
                           Environment.NewLine +
                           "DROP TABLE [ProductCategory_temporary];";

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
            var generator = new SqliteTableGenerator();
            var sql = generator.GetDropConstraintScript(relation);
            var expected = @"ALTER TABLE [ProductCategory] RENAME TO [ProductCategory_temporary];" +
                           Environment.NewLine +
                           "CREATE TABLE [ProductCategory]" + Environment.NewLine +
                           "(\t [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," + Environment.NewLine +
                           "\t [ProductId] INTEGER NOT NULL," + Environment.NewLine +
                           "\t [CategoryId] INTEGER NOT NULL);" + Environment.NewLine +
                           "INSERT INTO [ProductCategory] SELECT * FROM [ProductCategory_temporary];" +
                           Environment.NewLine +
                           "DROP TABLE [ProductCategory_temporary];";
            Assert.AreEqual(expected, sql);
        }
    }
}
#endif