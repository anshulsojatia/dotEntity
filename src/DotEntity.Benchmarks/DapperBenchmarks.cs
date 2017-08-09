using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Dapper;
using DotEntity.Benchmarks.Entity;

namespace DotEntity.Benchmarks
{
    public class DapperBenchmarks : BenchmarkSetup
    {

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
        }

        [Benchmark(Description = "Dapper Query<> with cache")]
        public IList<Product> GetWithCacheQuery()
        {
            Iterate();
            using (IDbConnection con = new SqlConnection(ConnectionString))
            {
                return con.Query<Product>("SELECT * FROM Product WHERE Id=@Id", new {Id = IterationCounter}).ToList();
            }
         
        }

        [Benchmark(Description = "Dapper Query<> without cache")]
        public IList<Product> GetWithoutCacheQuery()
        {
            Iterate();
            using (IDbConnection con = new SqlConnection(ConnectionString))
            {
                return con.Query<Product>("SELECT * FROM Product WHERE Id=@Id", new { Id = IterationCounter }, buffered: false).ToList();
            }
        }
    }
}