/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (IQueryGenerator.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
 * 
 * dotEntity is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 
 * dotEntity is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU Affero General Public License for more details.
 
 * You should have received a copy of the GNU Affero General Public License
 * along with dotEntity.If not, see<http://www.gnu.org/licenses/>.

 * You can release yourself from the requirements of the AGPL license by purchasing
 * a commercial license (dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DotEntity.Enumerations;

namespace DotEntity
{
    public interface IQueryGenerator
    {
        string GenerateInsert(string tableName, object entity, out IList<QueryInfo> parameters);

        string GenerateInsert<T>(T entity, out IList<QueryInfo> parameters) where T : class;

        string GenerateBatchInsert<T>(T[] entity, out IList<QueryInfo> parameters) where T : class;

        string GenerateUpdate<T>(object item, Expression<Func<T, bool>> where, out IList<QueryInfo> parameters) where T : class;

        string GenerateUpdate<T>(T entity, out IList<QueryInfo> queryParameters) where T : class;

        string GenerateUpdate(string tableName, object item, object where, out IList<QueryInfo> parameters, params string[] exclude);

        string GenerateDelete(string tableName, object where, out IList<QueryInfo> parameters);

        string GenerateDelete<T>(Expression<Func<T, bool>> where, out IList<QueryInfo> parameters) where T : class;

        string GenerateDelete<T>(T entity, out IList<QueryInfo> parameters) where T : class;

        string GenerateCount<T>(IList<Expression<Func<T, bool>>> where, out IList<QueryInfo> parameters) where T : class;

        string GenerateCount<T>(object where, out IList<QueryInfo> parameters);

        string GenerateCount(string tableName, object where, out IList<QueryInfo> parameters);

        string GenerateSelect<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> where = null,
            Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class;

        string GenerateSelectWithTotalMatchingCount<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> where = null,
            Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class;

        string GenerateJoin<T>(out IList<QueryInfo> parameters, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null,
            int page = 1, int count = int.MaxValue) where T : class;
        
        string Query(string query, object inParameters, out IList<QueryInfo> parameters);
    }
}