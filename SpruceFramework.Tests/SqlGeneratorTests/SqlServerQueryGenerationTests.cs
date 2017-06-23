using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using SpruceFramework.Enumerations;
using SpruceFramework.Extensions;
using SpruceFramework.Tests.Data;

namespace SpruceFramework.Tests.SqlGeneratorTests
{
    [TestFixture]
    public class SqlServerQueryGenerationTests
    {
        private readonly IQueryGenerator generator;

        public SqlServerQueryGenerationTests()
        {
            generator = new SqlServerQueryGenerator();
        }

        [Test]
        public void SelectGeneration_WithoutAnything_Valid()
        {
            var sql = generator.GenerateSelect<Product>(out IList<QueryParameter> queryParameters);
            var expected = "SELECT * FROM Product";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithWhere_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy"
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy" || product.Id > 5
            });
            var expected = "SELECT * FROM Product WHERE (ProductName = @ProductName) OR (Id > @Id)";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR2_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy" || product.ProductName == "Rnadom" || product.ProductName == "Crap" || product.Id > 1
            });
            var expected = "SELECT * FROM Product WHERE (((ProductName = @ProductName) OR (ProductName = @ProductName2)) OR (ProductName = @ProductName3)) OR (Id > @Id)";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR3_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => (product.ProductName == "Ice Candy" || product.ProductName == "Rnadom") && product.Id > 1
            });
            var expected = "SELECT * FROM Product WHERE ((ProductName = @ProductName) OR (ProductName = @ProductName2)) AND (Id > @Id)";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR4_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => (product.ProductName == "Ice Candy" || product.ProductName == "Rnadom") && (product.Id < 5 || product.Id > 10)
            });
            var expected = "SELECT * FROM Product WHERE ((ProductName = @ProductName) OR (ProductName = @ProductName2)) AND ((Id < @Id) OR (Id > @Id2))";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR5_With_Variable_Valid()
        {
            var value1 = "Ice Candy";
            var value2 = "Random";
            var value3 = 5;
            var value4 = 10;
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => (product.ProductName == value1 || product.ProductName == value2) && (product.Id < value3 || product.Id > value4)
            });
            var expected = "SELECT * FROM Product WHERE ((ProductName = @ProductName) OR (ProductName = @ProductName2)) AND ((Id < @Id) OR (Id > @Id2))";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithWhere_As_Bool_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => true
            });
            var expected = "SELECT * FROM Product WHERE 1 = 1";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithMemberExpressionWhere_Valid()
        {
            var str = "Ice Candy";
            var sql = generator.GenerateSelect<Product>(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == str,
                product => product.DateCreated == DateTime.Now
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated = @DateCreated";
            Assert.AreEqual(expected, sql);
        }


        [Test]
        public void SelectGenerator_WithFunctionExpressionWhere_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => product.DateCreated == DateTime.Now
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated = @DateCreated";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_Contains_In_Where_Valid()
        {
            var lst = new List<int> {1, 2, 3, 4};
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => product.Id.In(lst)
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND Id IN (@InParam_1,@InParam_2,@InParam_3,@InParam_4)";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_NotIn_In_Where_Valid()
        {
            var lst = new List<int> { 1, 2, 3, 4 };
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => !product.Id.In(lst)
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND Id NOT IN (@InParam_1,@InParam_2,@InParam_3,@InParam_4)";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_NotIn_Explicit_Pass_Int_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => !product.Id.In(new List<int> { 1, 2, 3, 4 })
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND Id NOT IN (@InParam_1,@InParam_2,@InParam_3,@InParam_4)";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_In_Explicit_Pass_String_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.In(new List<string> { "a", "b", "c", "d" })
            });
            var expected = "SELECT * FROM Product WHERE ProductName IN (@InParam_1,@InParam_2,@InParam_3,@InParam_4)";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_Contains2_In_Where_Valid()
        {
            var str = "Spruce Framework";
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters,new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.Contains(GetName())
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE '%' + @ProductName + '%'";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_StartsWith_In_Where_Valid()
        {
            var str = "Spruce Framework";
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.StartsWith(GetName())
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE @ProductName + '%'";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_StartsWith2_In_Where_Valid()
        {
            var str = "Spruce Framework";
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.StartsWith(str)
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE @ProductName + '%'";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_With_StartsWith3_In_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.StartsWith("a")
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE @ProductName + '%'";
            Assert.AreEqual(expected, sql);
        }



        [Test]
        public void SelectGenerator_With_NotStartsWith_In_Where_Valid()
        {
            var str = "Spruce Framework";
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => !product.ProductName.StartsWith(GetName())
            });
            var expected = "SELECT * FROM Product WHERE ProductName NOT LIKE @ProductName + '%'";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithFunctionExpressionWhereAndOrderBy_Valid()
        {
            var where = new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => product.DateCreated > DateTime.Now
            };

            var orderBy = new Dictionary<Expression<Func<Product, object>>, RowOrder>
            {
                {product => product.ProductName, RowOrder.Ascending}
            };
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, where, orderBy);
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated > @DateCreated ORDER BY ProductName";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void SelectGenerator_WithFunctionExpressionAndWhereAndMultipleOrderBy_Valid()
        {
            var date = DateTime.Now;
            var where = new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => product.DateCreated != date
            };

            var orderBy = new Dictionary<Expression<Func<Product, object>>, RowOrder>
            {
                {product => product.ProductName, RowOrder.Ascending},
                {product => product.Price, RowOrder.Descending }
            };
            var sql = generator.GenerateSelect(out IList<QueryParameter> queryParameters, where, orderBy);
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated != @DateCreated ORDER BY ProductName, Price DESC";
            Assert.AreEqual(expected, sql);
        }

        private string GetName()
        {
            return "Ice Candy";
        }

        [Test]
        public void InsertGenerator_EntityType_Valid()
        {
            var p = new Product();
            var sql = generator.GenerateInsert(p, out IList<QueryParameter> queryParameters);
            var expected = "INSERT INTO Product (ProductName,ProductDescription,DateCreated,Price,IsActive) OUTPUT inserted.ID VALUES (@ProductName,@ProductDescription,@DateCreated,@Price,@IsActive)";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void InsertGenerator_DynamicType_Valid()
        {
            var sql = generator.GenerateInsert("User", new { UserName = "JohnSmith", FirstName = "John", DateOfBirth = DateTime.Now}, out IList<QueryParameter> queryParameters);
            var expected = "INSERT INTO User (UserName,FirstName,DateOfBirth) OUTPUT inserted.ID VALUES (@UserName,@FirstName,@DateOfBirth)";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void UpdateGenerator_EntityType_Valid()
        {
            var sql = generator.GenerateUpdate<Product>(x => x.Id == 4, out IList<QueryParameter> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName,ProductDescription = @ProductDescription,DateCreated = @DateCreated,Price = @Price,IsActive = @IsActive WHERE Id = @Id";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void UpdateGenerator_DynamicType_Valid()
        {
            var sql = generator.GenerateUpdate("Product", new { ProductName = "x", ProductDescription="y", DateCreated = DateTime.Now, Price = 1.2d}, new { Id = 5}, out IList<QueryParameter> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName,ProductDescription = @ProductDescription,DateCreated = @DateCreated,Price = @Price WHERE Id = @Id";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void UpdateGenerator_EntityType_With_Multiple_Where_Valid()
        {
            var sql = generator.GenerateUpdate<Product>(x => x.Id == 4 && x.DateCreated > DateTime.Now, out IList<QueryParameter> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName,ProductDescription = @ProductDescription,DateCreated = @DateCreated,Price = @Price,IsActive = @IsActive WHERE (Id = @Id) AND (DateCreated > @DateCreated)";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void UpdateGenerator_DynamicType_With_Multiple_Where_Valid()
        {
            var sql = generator.GenerateUpdate("Product", new { ProductName = "x", ProductDescription = "y", DateCreated = DateTime.Now, Price = 1.2d }, new { Id = 5, DateCreated = DateTime.Now }, out IList<QueryParameter> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName,ProductDescription = @ProductDescription,DateCreated = @DateCreated,Price = @Price WHERE Id = @Id AND DateCreated = @DateCreated";

            Assert.AreEqual(expected, sql);
        }



        [Test]
        public void DeleteGenerator_EntityType_Valid()
        {
            var sql = generator.GenerateDelete<Product>(x => x.Price > 5, out IList<QueryParameter> queryParameters);
            var expected = "DELETE FROM Product WHERE Price > @Price";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DeleteGenerator_DynamicType_Valid()
        {
            var sql = generator.GenerateDelete("Product", new { Price = 5}, out IList<QueryParameter> queryParameters);
            var expected = "DELETE FROM Product WHERE Price = @Price";
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void QueryGenerator_ManualSelect_Valid()
        {

            var expected = "SELECT * FROM Product WHERE Id=@Id AND IsActive=@IsActive";
            var sql = generator.Query(expected, new {Id = 5, IsActive = false}, out IList<QueryParameter> queryParameters);
            Assert.AreEqual(expected, sql);
        }
    }
}
