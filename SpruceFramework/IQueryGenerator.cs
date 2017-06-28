// #region Author Information
// // IQueryGenerator.cs
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
    internal interface IQueryGenerator
    {
        string GenerateInsert(string tableName, dynamic entity, out IList<QueryParameter> parameters);

        string GenerateInsert<T>(T entity, out IList<QueryParameter> parameters) where T : class;

        string GenerateUpdate<T>(Expression<Func<T, bool>> where, out IList<QueryParameter> parameters) where T : class;

        string GenerateUpdate<T>(dynamic item, Expression<Func<T, bool>> where, out IList<QueryParameter> parameters) where T : class;

        string GenerateUpdate<T>(T entity, out IList<QueryParameter> queryParameters) where T : class;

        string GenerateUpdate(string tableName, dynamic item, dynamic where, out IList<QueryParameter> parameters, params string[] exclude);

        string GenerateDelete(string tableName, dynamic where, out IList<QueryParameter> parameters);

        string GenerateDelete<T>(Expression<Func<T, bool>> where, out IList<QueryParameter> parameters) where T : class;

        string GenerateSelect<T>(out IList<QueryParameter> parameters, List<Expression<Func<T, bool>>> where = null,
            Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class;

        string GenerateJoin<T>(out IList<QueryParameter> parameters, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null,
            int page = 1, int count = int.MaxValue) where T : class;
        
        string Query(string query, dynamic inParameters, out IList<QueryParameter> parameters);
    }
}