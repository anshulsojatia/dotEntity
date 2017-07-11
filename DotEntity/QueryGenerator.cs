// #region Author Information
// // QueryGenerator.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotEntity.Enumerations;

namespace DotEntity
{
    internal abstract class QueryGenerator : IQueryGenerator
    {
        private static Dictionary<JoinType, string> _joinMap = new Dictionary<JoinType, string>
        {
            { JoinType.Inner, "INNER JOIN" },
            { JoinType.LeftOuter, "LEFT OUTER JOIN" },
            { JoinType.RightOuter, "RIGHT OUTER JOIN" },
            { JoinType.FullOuter, "FULL OUTER JOIN" }
        };

        public virtual string GenerateInsert(string tableName, dynamic entity, out IList<QueryInfo> parameters)
        {
            Dictionary<string, object> columnValueMap = QueryParserUtilities.ParseObjectKeyValues(entity, exclude: "Id");
            var insertColumns = columnValueMap.Keys.ToArray();
            var joinInsertString = string.Join(",", insertColumns);
            var joinValueString = "@" + string.Join(",@", insertColumns); ;
            parameters = ToQueryInfos(columnValueMap);

            return $"INSERT INTO {tableName} ({joinInsertString}) OUTPUT inserted.Id VALUES ({joinValueString});";
        }

        public virtual string GenerateInsert<T>(T entity, out IList<QueryInfo> parameters) where T : class
        {
            var tableName = DotEntityDb.GetTableNameForType<T>();
            return GenerateInsert(tableName, entity, out parameters);
        }

        public virtual string GenerateUpdate<T>(T entity, out IList<QueryInfo> queryParameters) where T : class
        {
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var deserializer = DataDeserializer<T>.Instance;
            var keyColumn = deserializer.GetKeyColumn();
            var keyColumnValue = deserializer.GetPropertyAs<int>(entity, keyColumn);
            dynamic data = new ExpandoObject();
            var dataDictionary = (IDictionary<string, object>)data;
            dataDictionary.Add(keyColumn, keyColumnValue);
            return GenerateUpdate(tableName, entity, dataDictionary, out queryParameters, keyColumn);
        }

        public virtual string GenerateUpdate(string tableName, dynamic entity, dynamic where, out IList<QueryInfo> queryParameters, params string[] exclude)
        {
            Dictionary<string, object> updateValueMap = QueryParserUtilities.ParseObjectKeyValues(entity, exclude);
            Dictionary<string, object> whereMap = QueryParserUtilities.ParseObjectKeyValues(where);

            queryParameters = ToQueryInfos(updateValueMap, whereMap);

            var updateString = string.Join(",", updateValueMap.Select(x => $"{x.Key} = @{x.Key}"));
            //get the common keys
            var commonKeys = updateValueMap.Keys.Intersect(whereMap.Keys);
            var whereString = string.Join(" AND ", whereMap.Select(x =>
            {
                var prefix = commonKeys.Contains(x.Key) ? "2" : "";
                return $"{x.Key} = @{x.Key}{prefix}";
            }));
            return $"UPDATE {tableName} SET {updateString} WHERE {whereString};";
        }

        public virtual string GenerateUpdate<T>(dynamic item, Expression<Func<T, bool>> where, out IList<QueryInfo> queryParameters) where T : class
        {
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var builder = new StringBuilder();
            // convert the query parms into a SQL string and dynamic property object
            builder.Append("UPDATE ");
            builder.Append(tableName);
            builder.Append(" SET ");

            Dictionary<string, object> updateValueMap = QueryParserUtilities.ParseObjectKeyValues(item);
            var updateString = string.Join(",", updateValueMap.Select(x => $"{x.Key} = @{x.Key}"));

            builder.Append(updateString);
            var parser = new ExpressionTreeParser(where);
            var whereString = parser.GetWhereString();
            queryParameters = parser.QueryInfoList;

            if (!string.IsNullOrEmpty(whereString))
            {
                //update where string to handle common parameters
                var commonKeys = updateValueMap.Keys.Intersect(queryParameters.Select(x => x.PropertyName));
                whereString = commonKeys.Aggregate(whereString, (current, ck) => current.Replace($"@{ck}", $"@{ck}2"));
            }

            queryParameters = MergeParameters(ToQueryInfos(updateValueMap), queryParameters);

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            return builder.ToString().Trim() + ";";
        }

        public virtual string GenerateDelete(string tableName, dynamic where, out IList<QueryInfo> parameters)
        {
            Dictionary<string, object> whereMap = QueryParserUtilities.ParseObjectKeyValues(where);
            var whereString = string.Join(" AND ", whereMap.Select(x => $"{x.Key} = @{x.Key}"));
            parameters = ToQueryInfos(whereMap);
            return $"DELETE FROM {tableName} WHERE {whereString};";
        }

        public virtual string GenerateDelete<T>(Expression<Func<T, bool>> where, out IList<QueryInfo> parameters) where T : class
        {
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var parser = new ExpressionTreeParser(where);
            var whereString = parser.GetWhereString().Trim();
            parameters = parser.QueryInfoList;
            return $"DELETE FROM {tableName} WHERE {whereString};";
        }

        public string GenerateCount<T>(IList<Expression<Func<T, bool>>> @where, out IList<QueryInfo> parameters) where T : class
        {
            parameters = new List<QueryInfo>();
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var whereString = "";

            if (where != null)
            {
                var whereStringBuilder = new List<string>();
                foreach (var wh in where)
                {
                    var parser = new ExpressionTreeParser(wh);
                    whereStringBuilder.Add(parser.GetWhereString());
                    var queryParameters = parser.QueryInfoList;
                    parameters = parameters.Concat(queryParameters).ToList();
                }
                whereString = string.Join(" AND ", whereStringBuilder).Trim();
            }

            return $"SELECT COUNT(*) FROM {tableName} WHERE {whereString};";
        }

      

        public string GenerateCount<T>(dynamic @where, out IList<QueryInfo> parameters)
        {
            var tableName = DotEntityDb.GetTableNameForType<T>();
            return GenerateCount(tableName, where, out parameters);
        }

        public string GenerateCount(string tableName, dynamic @where, out IList<QueryInfo> parameters)
        {
            Dictionary<string, object> whereMap = QueryParserUtilities.ParseObjectKeyValues(where);
            var whereString = string.Join(" AND ", whereMap.Select(x => $"{x.Key} = @{x.Key}"));
            parameters = ToQueryInfos(whereMap);
            return $"SELECT COUNT(*) FROM {tableName} WHERE {whereString};";
        }

        public virtual string GenerateSelect<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue) where T : class
        {
            parameters = new List<QueryInfo>();
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();

            var whereString = "";

            if (where != null)
            {
                var whereStringBuilder = new List<string>();
                foreach (var wh in where)
                {
                    var parser = new ExpressionTreeParser(wh);
                    whereStringBuilder.Add(parser.GetWhereString());
                    var queryParameters = parser.QueryInfoList;
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
                    var parser = new ExpressionTreeParser(ob.Key);
                    orderByStringBuilder.Add(parser.GetOrderByString() + (ob.Value == RowOrder.Descending ? " DESC" : ""));
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
                query = $"SELECT * FROM ({query}) AS __PAGINATEDRESULT__ WHERE {newWhereString};";
            }
            else
                query = query + ";";
            return query;
        }

        public string GenerateSelectWithTotalMatchingCount<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> @where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null,
            int page = 1, int count = Int32.MaxValue) where T : class
        {
            parameters = new List<QueryInfo>();
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();

            var whereString = "";

            if (where != null)
            {
                var whereStringBuilder = new List<string>();
                foreach (var wh in where)
                {
                    var parser = new ExpressionTreeParser(wh);
                    whereStringBuilder.Add(parser.GetWhereString());
                    var queryParameters = parser.QueryInfoList;
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
                    var parser = new ExpressionTreeParser(ob.Key);
                    orderByStringBuilder.Add(parser.GetOrderByString() + (ob.Value == RowOrder.Descending ? " DESC" : ""));
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
                query = $"SELECT * FROM ({query}) AS __PAGINATEDRESULT__ WHERE {newWhereString};";
            }

            //and the count query
            query = query + $"{Environment.NewLine}SELECT COUNT(*) FROM {tableName}" + (string.IsNullOrEmpty(whereString)
                ? ""
                : $" WHERE {whereString}") + ";";
            return query;
        }

        public virtual string GenerateJoin<T>(out IList<QueryInfo> parameters, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null,
            int page = 1, int count = int.MaxValue) where T : class
        {
            parameters = new List<QueryInfo>();
            var typedAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var parentAliasUsed = "t1";
            var lastAliasUsed = parentAliasUsed;
            typedAliases.Add(typeof(T).Name, lastAliasUsed);

            var joinBuilder = new StringBuilder();
            foreach (var joinMeta in joinMetas)
            {
                var joinedTableName = DotEntityDb.GetTableNameForType(joinMeta.OnType);
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
                    $"{_joinMap[joinMeta.JoinType]} {joinedTableName} {newAlias} ON {sourceAlias}.[{joinMeta.SourceColumnName}] = {newAlias}.[{joinMeta.DestinationColumnName}] ");

                lastAliasUsed = newAlias;

            }

            var whereStringBuilder = new List<string>();
            var whereString = "";

            if (where != null)
            {
                foreach (var wh in where)
                {
                    var parser = new ExpressionTreeParser(wh, typedAliases);
                    whereStringBuilder.Add(parser.GetWhereString());
                    var queryParameters = parser.QueryInfoList;
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
                    var parser = new ExpressionTreeParser(ob.Key);
                    orderByStringBuilder.Add(parser.GetOrderByString() + (ob.Value == RowOrder.Descending ? " DESC" : ""));
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
            return query + ";";
        }

        public virtual string Query(string query, dynamic inParameters, out IList<QueryInfo> parameters)
        {
            var columnValueMap = QueryParserUtilities.ParseObjectKeyValues(inParameters);
            parameters = ToQueryInfos(columnValueMap);
            return query;
        }

        private static IList<QueryInfo> ToQueryInfos(params Dictionary<string, object>[] dict)
        {
            if (dict == null)
                return null;
            var queryParameters = new List<QueryInfo>();
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
                    queryParameters.Add(new QueryInfo(string.Empty, propertyName, propertyValue, string.Empty, parameterName));
                }
            }
            return queryParameters;
        }

        private static IList<QueryInfo> MergeParameters(IEnumerable<QueryInfo> baseList, params IList<QueryInfo>[] queryParameterList)
        {
            //update all the parameter names first
            var queryParameters = baseList as IList<QueryInfo> ?? baseList.ToList();
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
