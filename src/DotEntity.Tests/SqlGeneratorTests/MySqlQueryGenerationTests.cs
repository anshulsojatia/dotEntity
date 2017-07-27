using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using DotEntity.Enumerations;
using DotEntity.MySql;
using DotEntity.Tests.Data;

namespace DotEntity.Tests.SqlGeneratorTests
{
    [TestFixture]
    public class MySqlQueryGenerationTests
    {
        private readonly IQueryGenerator generator;

        public MySqlQueryGenerationTests()
        {
            generator = new MySqlQueryGenerator();
        }

        [Test]
        public void SelectGeneration_WithoutAnything_Valid()
        {
            var sql = generator.GenerateSelect<Product>(out IList<QueryInfo> queryParameters);
            var expected = "SELECT * FROM Product;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(0, queryParameters.Count);
        }

        [Test]
        public void SelectGeneration_WithoutAnything_Paginated_Valid()
        {
            var sql = generator.GenerateSelect<Product>(out IList<QueryInfo> queryParameters,
                orderBy: new Dictionary<Expression<Func<Product, object>>, RowOrder>
                {
                    {product => product.Id, RowOrder.Ascending}
                }, page: 1, count: 30);

            var expected = "SELECT * FROM Product ORDER BY Id LIMIT 0,30;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(0, queryParameters.Count);
        }

        [Test]
        public void SelectGeneration_With_Where_Paginated_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryInfos, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy"
            }, new Dictionary<Expression<Func<Product, object>>, RowOrder>
            {
                {product => product.Id, RowOrder.Ascending}
            }, 1, 30);

            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName ORDER BY Id LIMIT 0,30;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("Ice Candy", queryInfos.First(x => x.PropertyName == "ProductName").PropertyValue);
        }   


        [Test]
        public void SelectGenerator_WithWhere_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy"
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("Ice Candy", queryParameters.First(x => x.PropertyName == "ProductName").PropertyValue);
        }

        [Test]
        public void SelectGenerator_WithWhereAndTotalRecords_Valid()
        {
            var sql = generator.GenerateSelectWithTotalMatchingCount(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy"
            });
            var expected = $"SELECT * FROM Product WHERE ProductName = @ProductName;{Environment.NewLine}SELECT COUNT(*) FROM Product WHERE ProductName = @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("Ice Candy", queryParameters.First(x => x.PropertyName == "ProductName").PropertyValue);
        }

        [Test]
        public void SelectGenerator_WithWhere_WithoutConstant_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == product.ProductDescription
            });
            var expected = "SELECT * FROM Product WHERE ProductName = ProductDescription;";
            Assert.AreEqual(expected, sql);
            var qp = queryParameters.First(x => x.PropertyName == "ProductName");
            Assert.AreEqual("ProductDescription", qp.PropertyValue);
            Assert.AreEqual(true, qp.IsPropertyValueAlsoProperty);
        }

        [Test]
        public void SelectGenerator_WithWhere_With_OtherObject_Proprety_Valid()
        {
            var p = new Product()
            {
                ProductDescription = "This is not going to be used"
            };
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == p.ProductDescription
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(p.ProductDescription, queryParameters.First(x => x.PropertyName == "ProductName").PropertyValue);

        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy" || product.Id > 5
            });
            var expected = "SELECT * FROM Product WHERE (ProductName = @ProductName) OR (Id > @Id);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("Ice Candy", queryParameters.First(x => x.PropertyName == "ProductName").PropertyValue);
            Assert.AreEqual(5, queryParameters.First(x => x.PropertyName == "Id").PropertyValue);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR2_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == "Ice Candy" || product.ProductName == "Random" || product.ProductName == "Crap" || product.Id > 1
            });
            var expected = "SELECT * FROM Product WHERE (((ProductName = @ProductName) OR (ProductName = @ProductName2)) OR (ProductName = @ProductName3)) OR (Id > @Id);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("Ice Candy", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual("Random", queryParameters.First(x => x.ParameterName == "ProductName2").PropertyValue);
            Assert.AreEqual("Crap", queryParameters.First(x => x.ParameterName == "ProductName3").PropertyValue);
            Assert.AreEqual(1, queryParameters.First(x => x.PropertyName == "Id").PropertyValue);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR3_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => (product.ProductName == "Ice Candy" || product.ProductName == "Random") && product.Id > 1
            });
            var expected = "SELECT * FROM Product WHERE ((ProductName = @ProductName) OR (ProductName = @ProductName2)) AND (Id > @Id);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("Ice Candy", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual("Random", queryParameters.First(x => x.ParameterName == "ProductName2").PropertyValue);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR4_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => (product.ProductName == "Ice Candy" || product.ProductName == "Random") && (product.Id < 5 || product.Id > 10)
            });
            var expected = "SELECT * FROM Product WHERE ((ProductName = @ProductName) OR (ProductName = @ProductName2)) AND ((Id < @Id) OR (Id > @Id2));";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("Ice Candy", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual("Random", queryParameters.First(x => x.ParameterName == "ProductName2").PropertyValue);
            Assert.AreEqual(5, queryParameters.First(x => x.ParameterName == "Id").PropertyValue);
            Assert.AreEqual(10, queryParameters.First(x => x.ParameterName == "Id2").PropertyValue);
        }

        [Test]
        public void SelectGenerator_WithWhere_AndOR5_With_Variable_Valid()
        {
            var value1 = "Ice Candy";
            var value2 = "Random";
            var value3 = 5;
            var value4 = 10;
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => (product.ProductName == value1 || product.ProductName == value2) && (product.Id < value3 || product.Id > value4)
            });
            var expected = "SELECT * FROM Product WHERE ((ProductName = @ProductName) OR (ProductName = @ProductName2)) AND ((Id < @Id) OR (Id > @Id2));";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(value1, queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(value2, queryParameters.First(x => x.ParameterName == "ProductName2").PropertyValue);
            Assert.AreEqual(value3, queryParameters.First(x => x.ParameterName == "Id").PropertyValue);
            Assert.AreEqual(value4, queryParameters.First(x => x.ParameterName == "Id2").PropertyValue);
        }

        [Test]
        public void SelectGenerator_WithWhere_As_Bool_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => true
            });
            var expected = "SELECT * FROM Product WHERE 1 = 1;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(1, queryParameters.Count);
            Assert.AreEqual(true, queryParameters.First().IsPropertyValueAlsoProperty);
        }

        [Test]
        public void SelectGenerator_WithMemberExpressionWhere_Valid()
        {
            var str = "Ice Candy";
            var sql = generator.GenerateSelect<Product>(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == str,
                product => product.DateCreated == DateTime.Now
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated = @DateCreated;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(str, queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(DateTime.Now.Date, ((DateTime) queryParameters.First(x => x.ParameterName == "DateCreated").PropertyValue).Date);
        }


        [Test]
        public void SelectGenerator_WithFunctionExpressionWhere_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => product.DateCreated == DateTime.Now
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated = @DateCreated;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName(), queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(DateTime.Now.Date, ((DateTime)queryParameters.First(x => x.ParameterName == "DateCreated").PropertyValue).Date);
        }

        [Test]
        public void SelectGenerator_With_Contains_In_Where_Valid()
        {
            var lst = new List<int> {1, 2, 3, 4};
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => lst.Contains(product.Id)
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND Id IN (@Id_InParam_1,@Id_InParam_2,@Id_InParam_3,@Id_InParam_4);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName(), queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(1, queryParameters.First(x => x.ParameterName == "Id_InParam_1").PropertyValue);
            Assert.AreEqual(2, queryParameters.First(x => x.ParameterName == "Id_InParam_2").PropertyValue);
            Assert.AreEqual(3, queryParameters.First(x => x.ParameterName == "Id_InParam_3").PropertyValue);
            Assert.AreEqual(4, queryParameters.First(x => x.ParameterName == "Id_InParam_4").PropertyValue);

        }

        [Test]
        public void SelectGenerator_With_NotIn_In_Where_Valid()
        {
            var lst = new List<int> { 1, 2, 3, 4 };
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => !lst.Contains(product.Id)
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND Id NOT IN (@Id_InParam_1,@Id_InParam_2,@Id_InParam_3,@Id_InParam_4);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName(), queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(1, queryParameters.First(x => x.ParameterName == "Id_InParam_1").PropertyValue);
            Assert.AreEqual(2, queryParameters.First(x => x.ParameterName == "Id_InParam_2").PropertyValue);
            Assert.AreEqual(3, queryParameters.First(x => x.ParameterName == "Id_InParam_3").PropertyValue);
            Assert.AreEqual(4, queryParameters.First(x => x.ParameterName == "Id_InParam_4").PropertyValue);
        }

        [Test]
        public void Select_Count_Generator_With_NotIn_In_Where_Valid()
        {
            var lst = new List<int> { 1, 2, 3, 4 };
            var sql = generator.GenerateCount(new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => !lst.Contains(product.Id)
            }, out IList<QueryInfo> queryParameters);
            var expected = "SELECT COUNT(*) FROM Product WHERE ProductName = @ProductName AND Id NOT IN (@Id_InParam_1,@Id_InParam_2,@Id_InParam_3,@Id_InParam_4);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName(), queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(1, queryParameters.First(x => x.ParameterName == "Id_InParam_1").PropertyValue);
            Assert.AreEqual(2, queryParameters.First(x => x.ParameterName == "Id_InParam_2").PropertyValue);
            Assert.AreEqual(3, queryParameters.First(x => x.ParameterName == "Id_InParam_3").PropertyValue);
            Assert.AreEqual(4, queryParameters.First(x => x.ParameterName == "Id_InParam_4").PropertyValue);
        }

        [Test]
        public void SelectGenerator_With_NotIn_Explicit_Pass_Int_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName == GetName(),
                product => !(new List<int> { 1, 2, 3, 4 }).Contains(product.Id)
            });
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND Id NOT IN (@Id_InParam_1,@Id_InParam_2,@Id_InParam_3,@Id_InParam_4);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName(), queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(1, queryParameters.First(x => x.ParameterName == "Id_InParam_1").PropertyValue);
            Assert.AreEqual(2, queryParameters.First(x => x.ParameterName == "Id_InParam_2").PropertyValue);
            Assert.AreEqual(3, queryParameters.First(x => x.ParameterName == "Id_InParam_3").PropertyValue);
            Assert.AreEqual(4, queryParameters.First(x => x.ParameterName == "Id_InParam_4").PropertyValue);
        }

        [Test]
        public void SelectGenerator_With_In_Explicit_Pass_String_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => (new List<string> { "a", "b", "c", "d" }).Contains(product.ProductName)
            });
            var expected = "SELECT * FROM Product WHERE ProductName IN (@ProductName_InParam_1,@ProductName_InParam_2,@ProductName_InParam_3,@ProductName_InParam_4);";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(4,
                ((ICollection) queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue).Count);
            Assert.AreEqual("a", queryParameters.First(x => x.ParameterName == "ProductName_InParam_1").PropertyValue);
            Assert.AreEqual("b", queryParameters.First(x => x.ParameterName == "ProductName_InParam_2").PropertyValue);
            Assert.AreEqual("c", queryParameters.First(x => x.ParameterName == "ProductName_InParam_3").PropertyValue);
            Assert.AreEqual("d", queryParameters.First(x => x.ParameterName == "ProductName_InParam_4").PropertyValue);
        }

        [Test]
        public void SelectGenerator_With_Contains2_In_Where_Valid()
        {
            var str = "Spruce Framework";
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters,new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.Contains(GetName())
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual('%' + GetName() + '%', queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
        }

        [Test]
        public void SelectGenerator_With_StartsWith_In_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.StartsWith(GetName())
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName() + '%', queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
        }

        [Test]
        public void SelectGenerator_With_StartsWith2_In_Where_Valid()
        {
            var str = "Spruce Framework";
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.StartsWith(str)
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(str + '%', queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
        }

        [Test]
        public void SelectGenerator_With_StartsWith3_In_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => product.ProductName.StartsWith("a")
            });
            var expected = "SELECT * FROM Product WHERE ProductName LIKE @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual("a%", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
        }



        [Test]
        public void SelectGenerator_With_NotStartsWith_In_Where_Valid()
        {
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, new List<Expression<Func<Product, bool>>>
            {
                product => !product.ProductName.StartsWith(GetName())
            });
            var expected = "SELECT * FROM Product WHERE ProductName NOT LIKE @ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName() + '%', queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
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
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy);
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated > @DateCreated ORDER BY ProductName;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName(), queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(DateTime.Now.Date, ((DateTime)queryParameters.First(x => x.ParameterName == "DateCreated").PropertyValue).Date);
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
            var sql = generator.GenerateSelect(out IList<QueryInfo> queryParameters, where, orderBy);
            var expected = "SELECT * FROM Product WHERE ProductName = @ProductName AND DateCreated != @DateCreated ORDER BY ProductName, Price DESC;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(GetName(), queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(DateTime.Now.Date, ((DateTime)queryParameters.First(x => x.ParameterName == "DateCreated").PropertyValue).Date);
        }

        private string GetName()
        {
            return "Ice Candy";
        }

        [Test]
        public void InsertGenerator_EntityType_Valid()
        {
            var p = new Product();
            var sql = generator.GenerateInsert(p, out IList<QueryInfo> queryParameters);
            var expected = "INSERT INTO Product (ProductName,ProductDescription,DateCreated,Price,IsActive) VALUES (@ProductName,@ProductDescription,@DateCreated,@Price,@IsActive);SELECT last_insert_id() AS Id;";

            Assert.AreEqual(expected, sql);
            Assert.AreEqual(5, queryParameters.Count);
        }

        [Test]
        public void InsertGenerator_DynamicType_Valid()
        {
            var sql = generator.GenerateInsert("User", new { UserName = "JohnSmith", FirstName = "John", DateOfBirth = DateTime.Now}, out IList<QueryInfo> queryParameters);
            var expected = "INSERT INTO User (UserName,FirstName,DateOfBirth) VALUES (@UserName,@FirstName,@DateOfBirth);SELECT last_insert_id() AS Id;";

            Assert.AreEqual(expected, sql);
            Assert.AreEqual(3, queryParameters.Count);
            Assert.AreEqual("JohnSmith", queryParameters.First(x => x.ParameterName == "UserName").PropertyValue);
            Assert.AreEqual("John", queryParameters.First(x => x.ParameterName == "FirstName").PropertyValue);
            Assert.AreEqual(DateTime.Now.Date, ((DateTime) queryParameters.First(x => x.ParameterName == "DateOfBirth").PropertyValue).Date);
        }

        [Test]
        public void UpdateGenerator_Entity_Valid()
        {
            var product = new Product()
            {
                Id = 1,
                IsActive = false,
                Price = 55,
                ProductDescription = "Some description",
                ProductName = "Somr proeud",
                DateCreated = DateTime.Today
            };
            var sql = generator.GenerateUpdate(product, out IList<QueryInfo> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName,ProductDescription = @ProductDescription,DateCreated = @DateCreated,Price = @Price,IsActive = @IsActive WHERE Id = @Id;";

            Assert.AreEqual(expected, sql);
            Assert.AreEqual(6, queryParameters.Count);
            Assert.AreEqual(product.ProductName, queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual(product.ProductDescription, queryParameters.First(x => x.ParameterName == "ProductDescription").PropertyValue);
            Assert.AreEqual(product.Price, queryParameters.First(x => x.ParameterName == "Price").PropertyValue);
            Assert.AreEqual(product.DateCreated, queryParameters.First(x => x.ParameterName == "DateCreated").PropertyValue);
            Assert.AreEqual(product.IsActive, queryParameters.First(x => x.ParameterName == "IsActive").PropertyValue);
            Assert.AreEqual(product.Id, queryParameters.First(x => x.ParameterName == "Id").PropertyValue);

        }

        [Test]
        public void UpdateGenerator_DynamicType_Valid()
        {
            var sql = generator.GenerateUpdate("Product", new { ProductName = "x", ProductDescription="y", DateCreated = DateTime.Now, Price = 1.2d}, new { Id = 5}, out IList<QueryInfo> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName,ProductDescription = @ProductDescription,DateCreated = @DateCreated,Price = @Price WHERE Id = @Id;";

            Assert.AreEqual("x", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual("y", queryParameters.First(x => x.ParameterName == "ProductDescription").PropertyValue);
            Assert.AreEqual(1.2d, queryParameters.First(x => x.ParameterName == "Price").PropertyValue);
            Assert.AreEqual(DateTime.Now.Date,
                ((DateTime) queryParameters.First(x => x.ParameterName == "DateCreated").PropertyValue).Date);
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void UpdateGenerator_DynamicType_With_Multiple_Where_Valid()
        {
            var sql = generator.GenerateUpdate("Product", new { ProductName = "x", ProductDescription = "y", DateCreated = DateTime.Now, Price = 1.2d }, new { Id = 5, DateCreated = DateTime.Now }, out IList<QueryInfo> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName,ProductDescription = @ProductDescription,DateCreated = @DateCreated,Price = @Price WHERE Id = @Id AND DateCreated = @DateCreated2;";

            Assert.AreEqual(expected, sql);
            Assert.AreEqual("x", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual("y", queryParameters.First(x => x.ParameterName == "ProductDescription").PropertyValue);
            Assert.AreEqual(1.2d, queryParameters.First(x => x.ParameterName == "Price").PropertyValue);
            Assert.AreEqual(DateTime.Now.Date,
                ((DateTime)queryParameters.First(x => x.ParameterName == "DateCreated").PropertyValue).Date);
            Assert.AreEqual(5, queryParameters.First(x => x.ParameterName == "Id").PropertyValue);
        }

        [Test]
        public void UpdateGenerator_DynamicType_With_Same_Where_Valid()
        {
            var sql = generator.GenerateUpdate("Product", new { ProductName = "x" }, new { ProductName = "y" }, out IList<QueryInfo> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName WHERE ProductName = @ProductName2;";

            Assert.AreEqual("x", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual("y", queryParameters.First(x => x.ParameterName == "ProductName2").PropertyValue);
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void UpdateGenerator_Expression_With_Same_Where_Valid()
        {
            var sql = generator.GenerateUpdate<Product>(new { ProductName = "x" }, x => x.ProductName == "y", out IList<QueryInfo> queryParameters);
            var expected = "UPDATE Product SET ProductName = @ProductName WHERE ProductName = @ProductName2;";

            Assert.AreEqual("x", queryParameters.First(x => x.ParameterName == "ProductName").PropertyValue);
            Assert.AreEqual("y", queryParameters.First(x => x.ParameterName == "ProductName2").PropertyValue);
            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void DeleteGenerator_EntityType_Valid()
        {
            var sql = generator.GenerateDelete<Product>(x => x.Price > 5, out IList<QueryInfo> queryParameters);
            var expected = "DELETE FROM Product WHERE Price > @Price;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(5, queryParameters.First(x => x.ParameterName == "Price").PropertyValue);
        }

        [Test]
        public void DeleteGenerator_DynamicType_Valid()
        {
            var sql = generator.GenerateDelete("Product", new { Price = 5}, out IList<QueryInfo> queryParameters);
            var expected = "DELETE FROM Product WHERE Price = @Price;";
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(5d, queryParameters.First(x => x.ParameterName == "Price").PropertyValue);
        }

        [Test]
        public void QueryGenerator_ManualSelect_Valid()
        {

            var expected = "SELECT * FROM Product WHERE Id=@Id AND IsActive=@IsActive;";
            var sql = generator.Query(expected, new {Id = 5, IsActive = false}, out IList<QueryInfo> queryParameters);
            Assert.AreEqual(expected, sql);
            Assert.AreEqual(5, queryParameters.First(x => x.ParameterName == "Id").PropertyValue);
            Assert.AreEqual(false, queryParameters.First(x => x.ParameterName == "IsActive").PropertyValue);
        }
      
    }
}
