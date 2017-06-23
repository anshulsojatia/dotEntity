// #region Author Information
// // SpruceQueryManager.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    internal class SpruceQueryManager<T> : IDisposable where T: class
    {
        private readonly IQueryGenerator _queryGenerator;
        private readonly IQueryProcessor _queryProcessor;
        internal SpruceQueryManager()
        {
            _queryGenerator = Spruce.QueryGenerator;
            _queryProcessor = Spruce.QueryProcessor;
        }

        public TType DoScaler<TType>(string query, dynamic parameters)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryParameter> queryParameters);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                if (typeof(TType).IsPrimitive)
                    return (TType) cmd.ExecuteScalar();

                return default(TType);
            }
        }

        public T[] Do(string query, dynamic parameters)
        {
            query = _queryGenerator.Query(query, parameters, out IList<QueryParameter> queryParameters);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    return DataDeserializer<T>.Instance.DeserializeMany(reader);
                }
            }
        }

        public int DoInsert(T entity)
        {
            var query = _queryGenerator.GenerateInsert(entity, out IList<QueryParameter> queryParameters);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                var rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected;
            }
        }

        public int DoUpdate(T entity)
        {
            var query = _queryGenerator.GenerateUpdate(entity, out IList<QueryParameter> queryParameters);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                var rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected;
            }
        }

        public int DoDelete(T entity)
        {
            var query = _queryGenerator.GenerateDelete<T>(x => x == entity, out IList<QueryParameter> queryParameters);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                var rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected;
            }
        }

        public T[] DoSelect(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = 30)
        {
            var query = _queryGenerator.GenerateSelect(out IList<QueryParameter> queryParameters, where, orderBy, page, count);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    return DataDeserializer<T>.Instance.DeserializeMany(reader);
                }
            }
        }

        public T DoSelectSingle(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = 30)
        {
            var query = _queryGenerator.GenerateSelect(out IList<QueryParameter> queryParameters, where, orderBy, page, count);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    return DataDeserializer<T>.Instance.DeserializeSingle(reader);
                    
                }
            }
        }

      
        public void Dispose()
        {
            //do nothing
        }
    }
}