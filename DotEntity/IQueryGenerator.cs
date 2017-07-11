// #region Author Information
// // IQueryGenerator.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotEntity.Enumerations;

namespace DotEntity
{
    public interface IQueryGenerator
    {
        string GenerateInsert(string tableName, dynamic entity, out IList<QueryInfo> parameters);

        string GenerateInsert<T>(T entity, out IList<QueryInfo> parameters) where T : class;

        string GenerateBatchInsert<T>(T[] entity, out IList<QueryInfo> parameters) where T : class;

        string GenerateUpdate<T>(dynamic item, Expression<Func<T, bool>> where, out IList<QueryInfo> parameters) where T : class;

        string GenerateUpdate<T>(T entity, out IList<QueryInfo> queryParameters) where T : class;

        string GenerateUpdate(string tableName, dynamic item, dynamic where, out IList<QueryInfo> parameters, params string[] exclude);

        string GenerateDelete(string tableName, dynamic where, out IList<QueryInfo> parameters);

        string GenerateDelete<T>(Expression<Func<T, bool>> where, out IList<QueryInfo> parameters) where T : class;

        string GenerateCount<T>(IList<Expression<Func<T, bool>>> where, out IList<QueryInfo> parameters) where T : class;

        string GenerateCount<T>(dynamic where, out IList<QueryInfo> parameters);

        string GenerateCount(string tableName, dynamic where, out IList<QueryInfo> parameters);

        string GenerateSelect<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> where = null,
            Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class;

        string GenerateSelectWithTotalMatchingCount<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> where = null,
            Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class;

        string GenerateJoin<T>(out IList<QueryInfo> parameters, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null,
            int page = 1, int count = int.MaxValue) where T : class;
        
        string Query(string query, dynamic inParameters, out IList<QueryInfo> parameters);
    }
}