using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DotEntity.Enumerations;

namespace DotEntity.MySql
{
    public class MySqlQueryGenerator : DefaultQueryGenerator
    {
        public override string GenerateInsert(string tableName, dynamic entity, out IList<QueryInfo> parameters)
        {
            Dictionary<string, object> columnValueMap = QueryParserUtilities.ParseObjectKeyValues(entity, exclude: "Id");
            var insertColumns = columnValueMap.Keys.ToArray();
            var joinInsertString = string.Join(",", insertColumns);
            var joinValueString = "@" + string.Join(",@", insertColumns); ;
            parameters = ToQueryInfos(columnValueMap);

            return $"INSERT INTO {tableName} ({joinInsertString}) VALUES ({joinValueString});SELECT last_insert_id() AS Id;";
        }

        public override string GenerateSelect<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null, int page = 1, int count = int.MaxValue)
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
         
            // make the query now
            builder.Append($"SELECT * FROM ");
            builder.Append(tableName);

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            var offset = (page - 1) * count;
            builder.Append($" LIMIT {offset},{count}");
            var query = builder.ToString().Trim() + ";";
          
            return query;
        }

        public override string GenerateSelectWithTotalMatchingCount<T>(out IList<QueryInfo> parameters, List<Expression<Func<T, bool>>> @where = null, Dictionary<Expression<Func<T, object>>, RowOrder> orderBy = null,
            int page = 1, int count = Int32.MaxValue)
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
           
            // make the query now
            builder.Append($"SELECT * FROM ");
            builder.Append(tableName);

            if (!string.IsNullOrEmpty(whereString))
            {
                builder.Append(" WHERE " + whereString);
            }

            if (!string.IsNullOrEmpty(orderByString))
            {
                builder.Append(" ORDER BY " + orderByString);
            }
            var offset = (page - 1) * count;
            builder.Append($" LIMIT {offset},{count}");
            var query = builder.ToString().Trim() + ";";

            //and the count query
            query = query + $"{Environment.NewLine}SELECT COUNT(*) FROM {tableName}" + (string.IsNullOrEmpty(whereString)
                        ? ""
                        : $" WHERE {whereString}") + ";";
            return query;
        }
       
    }
}