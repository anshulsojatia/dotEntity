using System;
using System.Data.SqlClient;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace DotEntity.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public abstract class BenchmarkSetup
    {
        public static string ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=ms;Integrated Security=False;Persist Security Info=False;User ID=iis_user;Password=iis_user";
        public static int IterationCounter = 0;
        private const int TotalProducts = 5000;

        public void BaseSetup()
        {
            SetupDb();
        }

        private void SetupDb()
        {
            //create table
            var createProduct = @"
IF OBJECT_ID('Product', 'U') IS NOT NULL
    DROP TABLE Product;
CREATE TABLE Product
(     Id INT NOT NULL IDENTITY(1,1),
     ProductName NVARCHAR(MAX) NOT NULL,
     ProductDescription NVARCHAR(MAX) NOT NULL,
     DateCreated DATETIME NOT NULL,
     Price NUMERIC(18,0) NOT NULL,
     IsActive BIT NOT NULL,
PRIMARY KEY CLUSTERED (Id ASC));";

            var builder = new StringBuilder();
            const string insert = "INSERT INTO Product (ProductName, ProductDescription, DateCreated, Price, IsActive) VALUES ('Product Item {0}','Product Description {1}','{2}','{3}','{4}');";
            for (var i = 0; i < TotalProducts; i++)
            {
                builder.Append(string.Format(insert, i, i, DateTime.Now, "500", true));
                builder.Append(Environment.NewLine);
            }
            var insertQuery = builder.ToString();
            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = createProduct;
                cmd.ExecuteNonQuery();

                cmd.CommandText = insertQuery;
                cmd.ExecuteNonQuery();
            }
        }

        public void Iterate()
        {
            IterationCounter++;
            if (IterationCounter > 5000) IterationCounter = 0;
        }
    }

    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            Add(new MemoryDiagnoser());
            Add(Job.Default
                .WithLaunchCount(1)
                .WithWarmupCount(0)
                .WithUnrollFactor(50)
                .WithTargetCount(1)
                .WithRemoveOutliers(true)
            );
        }
    }
}