// #region Author Information
// // QueryGenerator.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SpruceFramework.Enumerations;

namespace SpruceFramework
{
    internal abstract class QueryGenerator : IQueryGenerator
    {
        //https://github.com/bbraithwaite/RepoWrapper
        public virtual string GenerateInsert(string tableName, dynamic entity, out IList<QueryParameter> parameters)
        {
            Dictionary<string, object> columnValueMap = QueryParserUtilities.ParseObjectKeyValues(entity, exclude: "Id");
            var insertColumns = columnValueMap.Keys.ToArray();
            var joinInsertString = string.Join(",", insertColumns);
            var joinValueString = "@" + string.Join(",@", insertColumns); ;
            parameters = ToQueryParameters(columnValueMap);

            return $"INSERT INTO {tableName} ({joinInsertString}) OUTPUT inserted.Id VALUES ({joinValueString})";
        }

        public virtual string GenerateInsert<T>(T entity, out IList<QueryParameter> parameters) where T : class
        {
            var tableName = Spruce.GetTableNameForType<T>();
            return GenerateInsert(tableName, entity, out parameters);
        }

        public virtual string GenerateUpdate<T>(T entity, out IList<QueryParameter> queryParameters) where T : class
        {
            var tableName = Spruce.GetTableNameForType<T>();
            return GenerateUpdate(tableName, entity, entity, out queryParameters);
        }

        public virtual string GenerateUpdate(string tableName, dynamic entity, dynamic where, out IList<QueryParameter> queryParameters)
        {
            Dictionary<string, object> updateValueMap = QueryParserUtilities.ParseObjectKeyValues(entity);
            Dictionary<string, object> whereMap = QueryParserUtilities.ParseObjectKeyValues(where);


            var updateString = string.Join(",", updateValueMap.Select(x => $"{x.Key} = @{x.Key}"));
            var whereString = string.Join(" AND ", whereMap.Select(x => $"{x.Key} = @{x.Key}"));
            queryParameters = ToQueryParameters(updateValueMap, whereMap);

            return $"UPDATE {tableName} SET {updateString} WHERE {whereString}";
        }

        public virtual string GenerateUpdate<T>(Expression<Func<T, bool>> where, out IList<QueryParameter> queryParameters) where T : class
        {
            var tableName = Spruce.GetTableNameForType<T>();
            var parameters = QueryParserUtilities.ParseTypeKeyValues(typeof(T), DataDeserializer<T>.Instance.GetKeyColumn());
            var updateString = string.Join(",", parameters);

            var whereString = ExpressionTreeParser.ParseAsWhereString(where, out queryParameters);
            if (!string.IsNullOrEmpty(whereString))
            {
                whereString = "WHERE " + whereString;
                whereString = whereString.Trim();
            }
            return $"UPDATE {tableName} SET {updateString} {whereString}";
        }



        public virtual string GenerateUpdate<T>(dynamic item, Expression<Func<T, bool>> where, out IList<QueryParameter> queryParameters) where T : class
        {
            var tableName = Spruce.GetTableNameForType<T>();
            var builder = new StringBuilder();
            // convert the query parms into a SQL string and dynamic property object
            builder.Append("UPDATE ");
            builder.Append(tableName);
            builder.Append(" SET ");

            Dictionary<string, object> updateValueMap = QueryParserUtilities.ParseObjectKeyValues(item);
            var updateString = string.Join(",", updateValueMap.Select(x => $"{x.Key} = @{x.Key}"));

            builder.Append(updateString);

            var whereString = ExpressionTreeParser.ParseAsWhereString(where, out queryParameters);

            queryParameters = MergeParameters(queryParameters, ToQueryParameters(updateValueMap));

            if (string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            return builder.ToString().Trim();
        }

        public virtual string GenerateDelete(string tableName, dynamic where, out IList<QueryParameter> parameters)
        {
            Dictionary<string, object> whereMap = QueryParserUtilities.ParseObjectKeyValues(where);
            var whereString = string.Join(" AND ", whereMap.Select(x => $"{x.Key} = @{x.Key}"));
            parameters = ToQueryParameters(whereMap);
            return $"DELETE FROM {tableName} WHERE {whereString}";
        }

        public virtual string GenerateDelete<T>(Expression<Func<T, bool>> where, out IList<QueryParameter> parameters) where T : class
        {
            var tableName = Spruce.GetTableNameForType<T>();
            var whereString = ExpressionTreeParser.ParseAsWhereString(where, out parameters).Trim();
            return $"DELETE FROM {tableName} WHERE {whereString}";
        }
        public virtual string GenerateSelect<T>(out IList<QueryParameter> parameters, List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            parameters = new List<QueryParameter>();
            var builder = new StringBuilder();
            var tableName = Spruce.GetTableNameForType<T>();
            
            var whereStringBuilder = new List<string>();
            var whereString = "";

            if (where != null)
            {
                foreach (var wh in where)
                {
                    whereStringBuilder.Add(
                        ExpressionTreeParser.ParseAsWhereString(wh, out IList<QueryParameter> queryParameters));

                    parameters = parameters.Concat(queryParameters).ToList();
                }
                whereString = string.Join(" AND ", whereStringBuilder).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                foreach (var ob in orderBy)
                {

                    orderByStringBuilder.Add(ExpressionTreeParser.ParseAsOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            var paginatedSelect = PaginateOrderByString(orderByString, page, count, out string newWhereString);
            if (paginatedSelect != string.Empty)
            {
                paginatedSelect = "," + paginatedSelect;
                orderByString = string.Empty;
            }
            // make the query now
            builder.Append($"SELECT *{paginatedSelect} FROM ");
            builder.Append(tableName);

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            var query = builder.ToString().Trim();
            if (paginatedSelect != string.Empty)
            {
                //wrap everything
                query = $"SELECT * FROM ({query}) AS __PAGINATEDRESULT__ WHERE {newWhereString}";
            }
            return query;
        }

        public virtual string GenerateJoin<T>(out IList<QueryParameter> parameters, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null,
            int page = 1, int count = int.MaxValue) where T : class
        {
            parameters = new List<QueryParameter>();
            var typedAliases = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var builder = new StringBuilder();
            var tableName = Spruce.GetTableNameForType<T>();
            var parentAliasUsed = "t1";
            var lastAliasUsed = parentAliasUsed;
            typedAliases.Add(typeof(T).Name, lastAliasUsed);

            var joinBuilder = new StringBuilder();
            foreach (var joinMeta in joinMetas)
            {
                var joinedTableName = Spruce.GetTableNameForType(joinMeta.OnType);
                if (!typedAliases.TryGetValue(joinedTableName, out string newAlias))
                {
                    newAlias = $"t{typedAliases.Count + 1}";
                }
                typedAliases.Add($"{joinedTableName}", newAlias);
                var sourceAlias = lastAliasUsed;
                if (joinMeta.SourceColumn == SourceColumn.Parent)
                {
                    sourceAlias = parentAliasUsed;
                }
                joinBuilder.Append(
                    $"INNER JOIN {joinedTableName} {newAlias} ON {sourceAlias}.[{joinMeta.SourceColumnName}] = {newAlias}.[{joinMeta.DestinationColumnName}] ");

                lastAliasUsed = newAlias;

            }

            var whereStringBuilder = new List<string>();
            var whereString = "";

            if (where != null)
            {
                foreach (var wh in where)
                {
                    whereStringBuilder.Add(
                        ExpressionTreeParser.ParseAsWhereString(wh, out IList<QueryParameter> queryParameters, typedAliases));

                    parameters = parameters.Concat(queryParameters).ToList();
                }
                whereString = string.Join(" AND ", whereStringBuilder).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                foreach (var ob in orderBy)
                {

                    orderByStringBuilder.Add(ExpressionTreeParser.ParseAsOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            var paginatedSelect = PaginateOrderByString(orderByString, page, count, out string newWhereString);
            if (paginatedSelect != string.Empty)
            {
                paginatedSelect = "," + paginatedSelect;
                orderByString = string.Empty;
            }
            // make the query now
            builder.Append($"SELECT *{paginatedSelect} FROM ");

            builder.Append(tableName + $" {parentAliasUsed} ");

            //join
            builder.Append(joinBuilder);

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            var query = builder.ToString().Trim();
            if (paginatedSelect != string.Empty)
            {
                //wrap everything
                query = $"SELECT * FROM ({query}) AS __PAGINATEDRESULT__ WHERE {newWhereString}";
            }
            return query;
        }

        public virtual string Query(string query, dynamic inParameters, out IList<QueryParameter> parameters)
        {
            var columnValueMap = QueryParserUtilities.ParseObjectKeyValues(inParameters);
            parameters = ToQueryParameters(columnValueMap);
            return query;
        }

        private static IList<QueryParameter> ToQueryParameters(params Dictionary<string, object>[] dict)
        {
            var queryParameters = new List<QueryParameter>();
            foreach (var dictionary in dict)
            {
                foreach (var strObj in dictionary)
                {
                    var propertyName = strObj.Key;
                    var propertyValue = strObj.Value;
                    var parameterName = propertyName;
                    //do we have any property of same name, we'll have to rename parameters if that's the case
                    if (queryParameters.Any(x => x.PropertyName == propertyName))
                    {
                        parameterName = propertyName + (queryParameters.Count(x => x.PropertyName == propertyName) + 1);
                    }
                    queryParameters.Add(new QueryParameter(string.Empty, propertyName, propertyValue, string.Empty, parameterName));
                }
            }
            return queryParameters;
        }

        private static IList<QueryParameter> MergeParameters(IEnumerable<QueryParameter> baseList, params IList<QueryParameter>[] queryParameterList)
        {
            //update all the parameter names first
            var queryParameters = baseList as IList<QueryParameter> ?? baseList.ToList();
            foreach (var list in queryParameterList)
            {
                foreach (var qp in list)
                {
                    if (queryParameters.Any(x => x.PropertyName == qp.PropertyName))
                    {
                        qp.ParameterName = qp.PropertyName + (queryParameters.Count(x => x.PropertyName == qp.PropertyName) + 1);
                    }
                }
                //concat this
                queryParameters = queryParameters.Concat(list).ToList();
            }

            return queryParameters;
        }

        private static string PaginateOrderByString(string orderByString, int page, int count, out string newWhereString)
        {
            if (page > 1 || count < int.MaxValue)
            {
                //pagination is required
                const string rowNumVariable = "__ROW_NUM__";
                var start = (page - 1) * count; //0
                var end = start + count + 1; //16
                newWhereString = $"{rowNumVariable} > {start} AND {rowNumVariable} < {end}"; //1-15
                return $"ROW_NUMBER() OVER (ORDER BY {orderByString}) AS {rowNumVariable}";
            }
            return newWhereString = string.Empty;
        }
    }
}
