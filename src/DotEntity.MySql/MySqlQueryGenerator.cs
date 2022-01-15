using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotEntity.Enumerations;
using DotEntity.Extensions;

namespace DotEntity.MySql
{
    public class MySqlQueryGenerator : DefaultQueryGenerator
    {
        const string TempTableName = "__DOTENTITY_FILTERED_IDS";

        public override string GenerateInsert(string tableName, object entity, out IList<QueryInfo> parameters)
        {
            GetColumns(entity, out var keyColumn, out var excludeColumns);
            Dictionary<string, object> columnValueMap = QueryParserUtilities.ParseObjectKeyValues(entity, exclude: excludeColumns);
            var insertColumns = columnValueMap.Keys.ToArray();
            var joinInsertString = string.Join(",", insertColumns.Select(x => x.ToEnclosed()));
            var joinValueString = "@" + string.Join(",@", insertColumns); ;
            parameters = ToQueryInfos(columnValueMap);
            var insertBuilder = new StringBuilder($"INSERT INTO {tableName.TableEnclosed()} ({joinInsertString}) VALUES ({joinValueString});");
            if (keyColumn != null)
                insertBuilder.Append($"SELECT last_insert_id() AS {keyColumn.ToEnclosed()};");
            return insertBuilder.ToString();
        }

        public override string GenerateSelect<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue, Dictionary<Type, IList<string>> excludeColumns = null)
        {
            parameters = new List<QueryInfo>();
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();

            var whereString = "";

            if (where != null)
            {
                var parser = new ExpressionTreeParser();
                var whereStringBuilder = new List<string>();
                foreach (var wh in where)
                {
                    whereStringBuilder.Add(parser.GetWhereString(wh));
                }
                parameters = parser.QueryInfoList;
                whereString = string.Join(" AND ", WrapWithBraces(whereStringBuilder)).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                var parser = new ExpressionTreeParser();
                foreach (var ob in orderBy)
                {
                    orderByStringBuilder.Add(parser.GetOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            // make the query now
            builder.Append($"SELECT {QueryParserUtilities.GetSelectColumnString(new List<Type>() { typeof(T) }, null, excludeColumns)} FROM ");
            builder.Append(tableName.ToEnclosed());

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            if (page > 1 || count != int.MaxValue)
            {
                var offset = (page - 1) * count;
                builder.Append($" LIMIT {offset},{count}");
            }
            var query = builder.ToString().Trim() + ";";
            return query;
        }

        public override string GenerateSelectWithTotalMatchingCount<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> @where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null,
            int page = 1, int count = Int32.MaxValue, Dictionary<Type, IList<string>> excludeColumns = null)
        {
            parameters = new List<QueryInfo>();
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();

            var whereString = "";

            if (where != null)
            {
                var parser = new ExpressionTreeParser();
                var whereStringBuilder = new List<string>();
                foreach (var wh in where)
                {
                    whereStringBuilder.Add(parser.GetWhereString(wh));
                }
                parameters = parser.QueryInfoList;
                whereString = string.Join(" AND ", WrapWithBraces(whereStringBuilder)).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                var parser = new ExpressionTreeParser();
                foreach (var ob in orderBy)
                {
                    orderByStringBuilder.Add(parser.GetOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            // make the query now
            builder.Append($"SELECT {QueryParserUtilities.GetSelectColumnString(new List<Type>() { typeof(T) }, null, excludeColumns)} FROM ");
            builder.Append(tableName.ToEnclosed());

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            if (page > 1 || count != int.MaxValue)
            {
                var offset = (page - 1) * count;
                builder.Append($" LIMIT {offset},{count}");
            }
            var query = builder.ToString().Trim() + ";";

            //and the count query
            query = query + $"{Environment.NewLine}SELECT COUNT(*) FROM {tableName.ToEnclosed()}" + (string.IsNullOrEmpty(whereString)
                        ? ""
                        : $" WHERE {whereString}") + ";";
            return query;
        }

        public override string GenerateSelectWithCustomSelection<T>(out IList<QueryInfo> parameters, string rawSelection, List<Expression<Func<T, bool>>> @where = null,
            Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = Int32.MaxValue)
        {
            parameters = new List<QueryInfo>();
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();

            var whereString = "";

            if (where != null)
            {
                var parser = new ExpressionTreeParser();
                var whereStringBuilder = new List<string>();
                foreach (var wh in where)
                {
                    whereStringBuilder.Add(parser.GetWhereString(wh));
                }
                parameters = parser.QueryInfoList;
                whereString = string.Join(" AND ", WrapWithBraces(whereStringBuilder)).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                var parser = new ExpressionTreeParser();
                foreach (var ob in orderBy)
                {
                    orderByStringBuilder.Add(parser.GetOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            // make the query now
            builder.Append($"SELECT {rawSelection} FROM ");
            builder.Append(tableName.ToEnclosed());

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            if (page > 1 || count != int.MaxValue)
            {
                var offset = (page - 1) * count;
                builder.Append($" LIMIT {offset},{count}");
            }
            var query = builder.ToString().Trim() + ";";
            return query;
        }

        public override string GenerateJoin<T>(out IList<QueryInfo> parameters, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null, Dictionary<LambdaExpression, RowOrder> orderBy = null,
            int page = 1, int count = int.MaxValue, Dictionary<Type, IList<string>> excludeColumns = null)
        {
            parameters = new List<QueryInfo>();
            var typedAliases = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var parentAliasUsed = "t1";
            var lastAliasUsed = parentAliasUsed;
            typedAliases.Add(typeof(T).Name, new List<string>() { lastAliasUsed });

            var joinBuilder = new StringBuilder();
            foreach (var joinMeta in joinMetas)
            {
                var joinedTableName = DotEntityDb.GetTableNameForType(joinMeta.OnType);
                var newAlias = $"t{typedAliases.SelectMany(x => x.Value).Count() + 1}";
                if (typedAliases.ContainsKey(joinedTableName))
                    typedAliases[joinedTableName].Add(newAlias);
                else
                {
                    typedAliases.Add($"{joinedTableName}", new List<string>() { newAlias });
                }

                var sourceAlias = lastAliasUsed;
                if (joinMeta.SourceColumn == SourceColumn.Parent)
                {
                    sourceAlias = parentAliasUsed;
                }
                else if (joinMeta.SourceColumn == SourceColumn.Implicit)
                {
                    var sourceTableName = DotEntityDb.GetTableNameForType(joinMeta.SourceColumnType);
                    if (!typedAliases.TryGetValue(sourceTableName, out List<string> availableAliases))
                    {
                        sourceAlias = lastAliasUsed;
                    }
                    else
                        sourceAlias = availableAliases?[joinMeta.SourceColumnAppearanceOrder] ?? lastAliasUsed;
                }
                joinBuilder.Append(
                    $"{JoinMap[joinMeta.JoinType]} {joinedTableName.ToEnclosed()} {newAlias} ON {sourceAlias}.{joinMeta.SourceColumnName.ToEnclosed()} = {newAlias}.{joinMeta.DestinationColumnName.ToEnclosed()} ");
                if (joinMeta.AdditionalJoinExpression != null)
                {
                    var parser = new ExpressionTreeParser(typedAliases);
                    var joinExpression = parser.GetWhereString(joinMeta.AdditionalJoinExpression);
                    joinBuilder.Append($"AND {joinExpression} ");
                    parameters = parameters.Concat(parser.QueryInfoList).ToList();
                }
                lastAliasUsed = newAlias;

            }

            var whereStringBuilder = new List<string>();
            var whereString = "";

            if (where != null)
            {
                var parser = new ExpressionTreeParser(typedAliases);
                foreach (var wh in where)
                {
                    whereStringBuilder.Add(parser.GetWhereString(wh));
                }
                parameters = parameters?.Concat(parser.QueryInfoList).ToList() ?? parser.QueryInfoList;
                whereString = string.Join(" AND ", WrapWithBraces(whereStringBuilder)).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                var parser = new ExpressionTreeParser(typedAliases);
                foreach (var ob in orderBy)
                {
                    orderByStringBuilder.Add(parser.GetOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            var allTypes = joinMetas.Select(x => x.OnType).Distinct().ToList();
            allTypes.Add(typeof(T));
            var tablePartBuilder = new StringBuilder();
            var commonPartBuilder = new StringBuilder();
            var wherePartBuilder = new StringBuilder();

            tablePartBuilder.Append(tableName.ToEnclosed() + $" {parentAliasUsed} ");

            //join
            tablePartBuilder.Append(joinBuilder);

            if (!string.IsNullOrEmpty(whereString))
            {
                wherePartBuilder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                commonPartBuilder.Append(" ORDER BY " + orderByString);
            }
            if (page > 1 || count != int.MaxValue)
            {
                var offset = (page - 1) * count;
                commonPartBuilder.Append($" LIMIT {offset},{count}");
            }

            var rootIdColumnName = $"{parentAliasUsed}.{typeof(T).GetKeyColumnName().ToEnclosed()}";

            // make the query now
            //to properly fetch relevant rows with pagination, we need to create a temp table
            builder.Append($"CREATE TEMPORARY TABLE {TempTableName} AS (SELECT DISTINCT({rootIdColumnName}) FROM {tablePartBuilder}{wherePartBuilder}{commonPartBuilder});{Environment.NewLine}");
            builder.Append($"SELECT {QueryParserUtilities.GetSelectColumnString(allTypes, typedAliases, excludeColumns)} FROM ");
            builder.Append(tablePartBuilder.ToString());

            builder.Append($" WHERE {rootIdColumnName} IN (SELECT * FROM {TempTableName})");
            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }

            var query = builder.ToString().Trim();
            return query + ";";           
        }

        public override string GenerateJoinWithTotalMatchingCount<T>(out IList<QueryInfo> parameters, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null,
            Dictionary<LambdaExpression, RowOrder> orderBy = null, int page = 1, int count = Int32.MaxValue, Dictionary<Type, IList<string>> excludeColumns = null)
        {
            parameters = new List<QueryInfo>();
            var typedAliases = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var parentAliasUsed = "t1";
            var lastAliasUsed = parentAliasUsed;
            typedAliases.Add(typeof(T).Name, new List<string>() { lastAliasUsed });

            var joinBuilder = new StringBuilder();
            foreach (var joinMeta in joinMetas)
            {
                var joinedTableName = DotEntityDb.GetTableNameForType(joinMeta.OnType);
                var newAlias = $"t{typedAliases.SelectMany(x => x.Value).Count() + 1}";
                if (typedAliases.ContainsKey(joinedTableName))
                    typedAliases[joinedTableName].Add(newAlias);
                else
                {
                    typedAliases.Add($"{joinedTableName}", new List<string>() { newAlias });
                }

                var sourceAlias = lastAliasUsed;
                if (joinMeta.SourceColumn == SourceColumn.Parent)
                {
                    sourceAlias = parentAliasUsed;
                }
                else if (joinMeta.SourceColumn == SourceColumn.Implicit)
                {
                    var sourceTableName = DotEntityDb.GetTableNameForType(joinMeta.SourceColumnType);
                    if (!typedAliases.TryGetValue(sourceTableName, out List<string> availableAliases))
                    {
                        sourceAlias = lastAliasUsed;
                    }
                    else
                        sourceAlias = availableAliases?[joinMeta.SourceColumnAppearanceOrder] ?? lastAliasUsed;
                }

                joinBuilder.Append(
                    $"{JoinMap[joinMeta.JoinType]} {joinedTableName.ToEnclosed()} {newAlias} ON {sourceAlias}.{joinMeta.SourceColumnName.ToEnclosed()} = {newAlias}.{joinMeta.DestinationColumnName.ToEnclosed()} ");
                if (joinMeta.AdditionalJoinExpression != null)
                {
                    var parser = new ExpressionTreeParser(typedAliases);
                    var joinExpression = parser.GetWhereString(joinMeta.AdditionalJoinExpression);
                    joinBuilder.Append($"AND {joinExpression} ");
                    parameters = parameters.Concat(parser.QueryInfoList).ToList();
                }
                lastAliasUsed = newAlias;

            }

            var whereStringBuilder = new List<string>();
            var whereString = "";
            if (where != null)
            {
                var parser = new ExpressionTreeParser(typedAliases);
                foreach (var wh in where)
                {
                    var wStr = parser.GetWhereString(wh);
                    whereStringBuilder.Add(wStr);
                }
                parameters = parameters?.Concat(parser.QueryInfoList).ToList() ?? parser.QueryInfoList;
                whereString = string.Join(" AND ", WrapWithBraces(whereStringBuilder)).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                var parser = new ExpressionTreeParser(typedAliases);
                foreach (var ob in orderBy)
                {
                    orderByStringBuilder.Add(parser.GetOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            var allTypes = joinMetas.Select(x => x.OnType).Distinct().ToList();
            allTypes.Add(typeof(T));
            var tablePartBuilder = new StringBuilder();
            var commonPartBuilder = new StringBuilder();
            var wherePartBuilder = new StringBuilder();

            tablePartBuilder.Append(tableName.ToEnclosed() + $" {parentAliasUsed} ");

            //join
            tablePartBuilder.Append(joinBuilder);

            if (!string.IsNullOrEmpty(whereString))
            {
                wherePartBuilder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                commonPartBuilder.Append(" ORDER BY " + orderByString);
            }
            if (page > 1 || count != int.MaxValue)
            {
                var offset = (page - 1) * count;
                commonPartBuilder.Append($" LIMIT {offset},{count}");
            }

            var rootIdColumnName = $"{parentAliasUsed}.{typeof(T).GetKeyColumnName().ToEnclosed()}";

            // make the query now
            //to properly fetch relevant rows with pagination, we need to create a temp table
            builder.Append($"CREATE TEMPORARY TABLE {TempTableName} AS (SELECT DISTINCT({rootIdColumnName}) FROM {tablePartBuilder}{wherePartBuilder}{commonPartBuilder});{Environment.NewLine}");
            builder.Append($"SELECT {QueryParserUtilities.GetSelectColumnString(allTypes, typedAliases, excludeColumns)} FROM ");
            builder.Append(tablePartBuilder.ToString());

            builder.Append($" WHERE {rootIdColumnName} IN (SELECT * FROM {TempTableName})");
            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }

            //now thecount query
            builder.Append(";" + Environment.NewLine);

            builder.Append(
                $"SELECT COUNT(DISTINCT({rootIdColumnName})) FROM {tablePartBuilder}{wherePartBuilder}");          

            var query = builder.ToString().Trim();
            return query + ";";
        }

        public override string GenerateJoinWithCustomSelection<T>(out IList<QueryInfo> parameters, string rawSelection, List<IJoinMeta> joinMetas, List<LambdaExpression> @where = null,
            Dictionary<LambdaExpression, RowOrder> orderBy = null, int page = 1, int count = Int32.MaxValue)
        {
            parameters = new List<QueryInfo>();
            var typedAliases = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var builder = new StringBuilder();
            var tableName = DotEntityDb.GetTableNameForType<T>();
            var parentAliasUsed = "t1";
            var lastAliasUsed = parentAliasUsed;
            typedAliases.Add(typeof(T).Name, new List<string>() { lastAliasUsed });

            var joinBuilder = new StringBuilder();
            foreach (var joinMeta in joinMetas)
            {
                var joinedTableName = DotEntityDb.GetTableNameForType(joinMeta.OnType);
                var newAlias = $"t{typedAliases.SelectMany(x => x.Value).Count() + 1}";
                if (typedAliases.ContainsKey(joinedTableName))
                    typedAliases[joinedTableName].Add(newAlias);
                else
                {
                    typedAliases.Add($"{joinedTableName}", new List<string>() { newAlias });
                }

                var sourceAlias = lastAliasUsed;
                if (joinMeta.SourceColumn == SourceColumn.Parent)
                {
                    sourceAlias = parentAliasUsed;
                }
                else if (joinMeta.SourceColumn == SourceColumn.Implicit)
                {
                    var sourceTableName = DotEntityDb.GetTableNameForType(joinMeta.SourceColumnType);
                    if (!typedAliases.TryGetValue(sourceTableName, out List<string> availableAliases))
                    {
                        sourceAlias = lastAliasUsed;
                    }
                    else
                        sourceAlias = availableAliases?[joinMeta.SourceColumnAppearanceOrder] ?? lastAliasUsed;
                }
                joinBuilder.Append(
                    $"{JoinMap[joinMeta.JoinType]} {joinedTableName.ToEnclosed()} {newAlias} ON {sourceAlias}.{joinMeta.SourceColumnName.ToEnclosed()} = {newAlias}.{joinMeta.DestinationColumnName.ToEnclosed()} ");
                if (joinMeta.AdditionalJoinExpression != null)
                {
                    var parser = new ExpressionTreeParser(typedAliases);
                    var joinExpression = parser.GetWhereString(joinMeta.AdditionalJoinExpression);
                    joinBuilder.Append($"AND {joinExpression} ");
                    parameters = parameters.Concat(parser.QueryInfoList).ToList();
                }
                lastAliasUsed = newAlias;

            }

            var whereStringBuilder = new List<string>();
            var whereString = "";

            if (where != null)
            {
                var parser = new ExpressionTreeParser(typedAliases);
                foreach (var wh in where)
                {
                    whereStringBuilder.Add(parser.GetWhereString(wh));
                }
                parameters = parameters?.Concat(parser.QueryInfoList).ToList() ?? parser.QueryInfoList;
                whereString = string.Join(" AND ", WrapWithBraces(whereStringBuilder)).Trim();
            }

            var orderByStringBuilder = new List<string>();
            var orderByString = "";
            if (orderBy != null)
            {
                var parser = new ExpressionTreeParser(typedAliases);
                foreach (var ob in orderBy)
                {
                    orderByStringBuilder.Add(parser.GetOrderByString(ob.Key) + (ob.Value == RowOrder.Descending ? " DESC" : ""));
                }

                orderByString = string.Join(", ", orderByStringBuilder).Trim(',');
            }

            var allTypes = joinMetas.Select(x => x.OnType).Distinct().ToList();
            allTypes.Add(typeof(T));

            var tablePartBuilder = new StringBuilder();
            var commonPartBuilder = new StringBuilder();
            var wherePartBuilder = new StringBuilder();

            tablePartBuilder.Append(tableName.ToEnclosed() + $" {parentAliasUsed} ");

            //join
            tablePartBuilder.Append(joinBuilder);

            if (!string.IsNullOrEmpty(whereString))
            {
                wherePartBuilder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                commonPartBuilder.Append(" ORDER BY " + orderByString);
            }
            if (page > 1 || count != int.MaxValue)
            {
                var offset = (page - 1) * count;
                commonPartBuilder.Append($" LIMIT {offset},{count}");
            }

            var rootIdColumnName = $"{parentAliasUsed}.{typeof(T).GetKeyColumnName().ToEnclosed()}";

            // make the query now
            //to properly fetch relevant rows with pagination, we need to create a temp table
            builder.Append($"CREATE TEMPORARY TABLE {TempTableName} AS (SELECT DISTINCT({rootIdColumnName}) FROM {tablePartBuilder}{wherePartBuilder}{commonPartBuilder});{Environment.NewLine}");
            builder.Append($"SELECT {rawSelection} FROM ");
            builder.Append(tablePartBuilder.ToString());

            builder.Append($" WHERE {rootIdColumnName} IN (SELECT * FROM {TempTableName})");
            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            return builder + ";";
        }
    }
}