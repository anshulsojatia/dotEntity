// #region Author Information
// // SpruceQueryManager.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public virtual TType DoScaler<TType>(string query, dynamic parameters)
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

        public virtual T[] Do(string query, dynamic parameters)
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

        public virtual int DoInsert(T entity)
        {
            var keyColumn = DataDeserializer<T>.Instance.GetKeyColumn();
            var query = _queryGenerator.GenerateInsert(entity, out IList<QueryParameter> queryParameters);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters, true, keyColumn))
            {
                con.Open();
                var id = (int) cmd.ExecuteScalar();
                con.Close();
                DataDeserializer<T>.Instance.SetPropertyAs<int>(entity, keyColumn, id);
                return 1;
            }
        }

        public virtual int DoUpdate(T entity)
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

        public virtual int DoDelete(T entity)
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

        public virtual T[] DoSelect(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue)
        {
            ThrowIfInvalidPage(orderBy, page, count);

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

        public virtual T[] DoSelect(List<IJoinMeta> joinMetas, Dictionary<Type, Delegate> relationActions, List<LambdaExpression> where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue)
        {
            ThrowIfInvalidPage(orderBy, page, count);

            var query = _queryGenerator.GenerateJoin<T>(out IList<QueryParameter> queryParameters, joinMetas, where, orderBy, page, count);
            using (var con = Spruce.Provider.Connection)
            using (var cmd = _queryProcessor.GetQueryCommand(con, query, queryParameters))
            {
                con.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    return DataDeserializer<T>.Instance.DeserializeManyNested(reader, joinMetas, relationActions);
                }
            }
        }

        public virtual T DoSelectSingle(List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null)
        {
            var query = _queryGenerator.GenerateSelect(out IList<QueryParameter> queryParameters, where, orderBy);
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

        private void ThrowIfInvalidPage(ICollection orderBy, int page, int count)
        {
            if (page < 1 || count < 0 || ((page > 1 || count < int.MaxValue) && (orderBy == null || orderBy.Count == 0)))
            {
                throw new Exception("Invalid pagination");
            }
        }

        public void Dispose()
        {
            //do nothing
        }
    }
}