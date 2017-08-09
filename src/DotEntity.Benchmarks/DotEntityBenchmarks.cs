using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using DotEntity.Benchmarks.Entity;
using DotEntity.SqlServer;

namespace DotEntity.Benchmarks
{
  
    public class DotEntityBenchmarks : BenchmarkSetup
    {
        private QueryCache _queryCache;

        [GlobalSetup]
        public void Setup()
        {
            BaseSetup();
            DotEntityDb.Initialize(ConnectionString, new SqlServerDatabaseProvider());
            _queryCache = EntitySet.GetQueryCache();
        }

        [Benchmark(Description = "EntitySet<>.Where without cache")]
        public IList<Product> GetWithoutCacheLinq()
        {
            Iterate();
            return EntitySet<Product>.Where(x => x.Id == IterationCounter).Select().ToList();
        }

        [Benchmark(Description = "EntitySet<>.Where with cache")]
        public IList<Product> GetWithCacheLinq()
        {
            Iterate();
            return EntitySet<Product>.WithQueryCache(_queryCache).Where(x => x.Id == IterationCounter).Select().ToList();
        }

        [Benchmark(Description = "EntitySet<>.Query with cache")]
        public IList<Product> GetWithCacheQuery()
        {
            Iterate();
            return EntitySet<Product>.Query("SELECT * FROM Product WHERE Id=@Id", new {Id = IterationCounter},
                    _queryCache, new object[] {IterationCounter})
                .ToList();
        }

        [Benchmark(Description = "EntitySet<>.Query without cache")]
        public IList<Product> GetWithoutCacheQuery()
        {
            Iterate();
            return EntitySet<Product>.Query("SELECT * FROM Product WHERE Id=@Id", new {Id = IterationCounter}).ToList();
        }

        [GlobalCleanup]
        public void Cleaup()
        {
            _queryCache.Dispose();
        }
    }

   
}