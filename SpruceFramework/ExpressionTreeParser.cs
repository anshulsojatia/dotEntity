// #region Author Information
// // ExpressionTreeParser.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SpruceFramework
{
    internal class ExpressionTreeParser
    {
        static class Markers
        {
            public const string Open = "OPEN";
            public const string Close = "CLOSE";
            public const string ContainsStart = "CONTAINS_START";
            public const string StartsWithStart = "STARTS_WITH_START";
            public const string EndsWithStart = "ENDS_WITH_START";
            public const string NoParam = "NOPARAM";
            public const string Not = "NOT";
        }

        /// <summary>
        /// Walks the tree.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <param name="linkingType">Type of the linking.</param>
        /// <param name="queryProperties">The query properties.</param>
        /// <param name="aliases"></param>
        public static void WalkTree(Expression body, ExpressionType linkingType, ref List<QueryParameter> queryProperties, Dictionary<string, string> aliases = null)
        {
            if (body is BinaryExpression)
            {
                var binaryBody = (BinaryExpression) body;
                if (binaryBody.NodeType != ExpressionType.AndAlso && binaryBody.NodeType != ExpressionType.OrElse)
                {
                    if ((binaryBody.Left as MemberExpression)?.Expression is ConstantExpression)
                    {
                        throw new Exception("Can't use constant expression for field names");
                    }
                    var propertyName = GetPropertyName(binaryBody);
                    var opr = GetOperator(binaryBody.NodeType);
                    var link = GetOperator(linkingType);

                    var br = binaryBody.Right;
                    var propertyValue = br is ConstantExpression || br is MethodCallExpression ? EvaluateExpression(br) : EvaluateCompiled((MemberExpression)br, aliases);
                    var parameterName = GetSafeParameterName(queryProperties, propertyName, propertyValue);

                    //alias if necessary
                    propertyName = GetAliasedPropertyName((MemberExpression) binaryBody.Left, aliases);

                    queryProperties.Add(new QueryParameter(link, propertyName, propertyValue, opr, parameterName));
                }
                else
                {
                    queryProperties.Add(new QueryParameter(true, Markers.Open));
                    WalkTree((BinaryExpression)binaryBody.Left, binaryBody.NodeType, ref queryProperties);
                    queryProperties.Add(new QueryParameter(true, Markers.Close));
                    queryProperties.Add(new QueryParameter(true, GetOperator(binaryBody.NodeType)));
                    queryProperties.Add(new QueryParameter(true, Markers.Open));
                    WalkTree((BinaryExpression)binaryBody.Right, binaryBody.NodeType, ref queryProperties);
                    queryProperties.Add(new QueryParameter(true, Markers.Close));
                }
            }
            else if (body is MethodCallExpression)
            {
                var mExpBody = (MethodCallExpression)body;
                if (mExpBody.Object == null)
                {
                    //the call must be by a reference type e.g. int
                    var arguments = mExpBody.Arguments;
                    var propertyName = EvaluateExpression(arguments[0])?.ToString();
                    object propertyValue = null;
                    for (var i = 1; i < arguments.Count; i++)
                    {
                        var argument = arguments[i];
                        if (argument is MethodCallExpression || argument is ListInitExpression)
                        {
                            propertyValue = EvaluateExpression(argument);
                        }
                        else if (argument is MemberExpression)
                        {
                            propertyValue = EvaluateCompiled((MemberExpression) argument);
                        }
                    }

                    var opr = "IN";
                    var lastParameter = queryProperties.LastOrDefault();
                    if (lastParameter?.LinkingOperator == Markers.Not)
                    {
                        opr = "NOT IN";
                        queryProperties.RemoveAt(queryProperties.Count - 1); //remove this element
                    }
                    queryProperties.Add(new QueryParameter(true, Markers.ContainsStart));
                    queryProperties.Add(new QueryParameter("", propertyName, propertyValue, opr, propertyName));

                }
                else
                {
                    if (mExpBody.Object.Type != typeof(string))
                    {
                        throw new Exception();
                    }
                    var propertyName = EvaluateExpression(mExpBody.Object)?.ToString();
                    var argument = mExpBody.Arguments[0];
                    var propertyValue = argument is ConstantExpression || argument is MethodCallExpression ? EvaluateExpression(argument) : EvaluateCompiled((MemberExpression) argument);
                    var opr = "LIKE";
                    var lastParameter = queryProperties.LastOrDefault();
                    if (lastParameter?.LinkingOperator == Markers.Not)
                    {
                        opr = "NOT LIKE";
                        queryProperties.RemoveAt(queryProperties.Count - 1); //remove this element
                    }

                    if (mExpBody.Method.Name == "Contains")
                    {
                        queryProperties.Add(new QueryParameter(true, Markers.ContainsStart));
                    }
                    else if (mExpBody.Method.Name == "StartsWith")
                    {
                        queryProperties.Add(new QueryParameter(true, Markers.StartsWithStart));
                    }
                    else if (mExpBody.Method.Name == "EndsWith")
                    {
                        queryProperties.Add(new QueryParameter(true, Markers.EndsWithStart));
                    }
                    queryProperties.Add(new QueryParameter("", propertyName, propertyValue, opr, propertyName));
                }
               
            }
            else if (body is ConstantExpression)
            {
                var constBody = (ConstantExpression) body;
                if (constBody.Value is bool)
                {
                    var value = (bool) constBody.Value;
                    queryProperties.Add(new QueryParameter("", "1", "1", value ? "=" : "!=", Markers.NoParam));
                }
            }
            else if (body is UnaryExpression)
            {
                var unaryBody = (UnaryExpression)body;
                if (unaryBody.NodeType == ExpressionType.Not)
                {
                    queryProperties.Add(new QueryParameter(true, Markers.Not));
                    WalkTree(unaryBody.Operand, linkingType, ref queryProperties);
                }
            }
        }

        internal static List<QueryParameter> GetQueryParameters(LambdaExpression where, Dictionary<string, string> aliases = null)
        {
            var queryProperties = new List<QueryParameter>();
            // walk the tree and build up a list of query parameter objects
            // from the left and right branches of the expression tree
            WalkTree(where.Body, ExpressionType.Default, ref queryProperties, aliases);
            return queryProperties;
        }

        internal static string ParseAsWhereString(LambdaExpression where, out IList<QueryParameter> queryProperties, Dictionary<string, string> aliases = null)
        {
            queryProperties = null;
            if (where == null)
                return string.Empty;

            var builder = new StringBuilder();

            queryProperties = GetQueryParameters(where, aliases);

            var pendingClose = 0;
            var bracketOpened = false;
            bool containsOpened = false, startsWith = false, endsWith = false;

            var extraQueryProperties = new List<QueryParameter>();
            for (var i = 0; i < queryProperties.Count(); i++)
            {
                var item = queryProperties[i];
                if (item.SupportOperator)
                {
                    if (item.LinkingOperator == Markers.Open)
                    {
                        builder.Append("(");
                        bracketOpened = true;
                        pendingClose++;
                    }
                    else if (item.LinkingOperator == Markers.Close)
                    {
                        builder.Append(")");
                        bracketOpened = false;
                        pendingClose--;
                    }
                    else if (item.LinkingOperator == Markers.ContainsStart)
                    {
                        containsOpened = true;
                    }
                    else if (item.LinkingOperator == Markers.StartsWithStart)
                    {
                        startsWith = true;
                    }
                    else if (item.LinkingOperator == Markers.EndsWithStart)
                    {
                        endsWith = true;
                    }
                    else if (item.LinkingOperator == Markers.Not)
                    {
                        builder.Append(" NOT ");
                    }
                    else
                    {
                        builder.Append(" " + item.LinkingOperator + " ");
                    }
                }
                else if (containsOpened)
                {
                    if (item.PropertyValue is IList)
                    {
                        var inItemCounter = 0;
                        //add parameters for each of the property value
                        builder.Append($"{item.PropertyName} {item.QueryOperator} (");
                        var itemCollection = (IList) item.PropertyValue;
                        for (var itemIndex = 0; itemIndex < itemCollection.Count; itemIndex++)
                        {
                            var paramName = "InParam_" + (itemIndex + 1);
                            var inItemValue = itemCollection[itemIndex];
                            extraQueryProperties.Add(new QueryParameter("", "inner_" + item.PropertyName, inItemValue, "", paramName));
                        }
                        var inParameterString = "@" + string.Join(",@", extraQueryProperties.Select(x => x.ParameterName));
                        builder.Append(inParameterString);
                        builder.Append(") ");

                        //mark the current query parameter as support one now
                        item.SupportOperator = true;
                    }
                    else if (item.PropertyValue is string)
                    {
                        builder.Append($"{item.PropertyName} {item.QueryOperator} '%' + @{item.ParameterName} + '%'");
                    }
                    containsOpened = false;
                }
                else if (startsWith)
                {
                    builder.Append($"{item.PropertyName} {item.QueryOperator} @{item.ParameterName} + '%'");
                    startsWith = false;
                }
                else if (endsWith)
                {
                    builder.Append($"{item.PropertyName} {item.QueryOperator} '%' + @{item.ParameterName}");
                    endsWith = false;
                }
                else if (!string.IsNullOrEmpty(item.LinkingOperator) && i > 0 && !bracketOpened)
                {
                    builder.Append(IncludeParamSymbol(item, out string propertyValue)
                        ? $" {item.LinkingOperator} {item.PropertyName} {item.QueryOperator} {propertyValue}"
                        : $" {item.LinkingOperator} {item.PropertyName} {item.QueryOperator} @{item.ParameterName}");
                }
                else
                {
                    builder.Append(IncludeParamSymbol(item, out string propertyValue)
                        ? $"{item.PropertyName} {item.QueryOperator} {propertyValue}"
                        : $"{item.PropertyName} {item.QueryOperator} @{item.ParameterName}");
                    bracketOpened = false;
                }

            }

            //add any extra parameters to the response
            queryProperties = queryProperties.Concat(extraQueryProperties).ToList();
            return builder.ToString();
        }

        internal static bool IncludeParamSymbol(QueryParameter parameter, out string value)
        {
            value = null;
            if (parameter.ParameterName == Markers.NoParam)
                value = parameter.PropertyValue.ToString();
            var propertyValue = parameter.PropertyValue as TablePropertyValue;
            if (propertyValue != null)
                value = propertyValue.Value;
            return value != null;
        }
        internal static string ParseAsOrderByString(LambdaExpression orderBy)
        {
            if (orderBy == null)
                return string.Empty;

            var ob = EvaluateExpression(orderBy);
            return ob?.ToString();
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>The property name for the property expression.</returns>
        private static string GetPropertyName(BinaryExpression body)
        {
            var propertyName = body.Left.ToString().Split('.')[1];

            if (body.Left.NodeType == ExpressionType.Convert)
            {
                // hack to remove the trailing ) when convering.
                propertyName = propertyName.Replace(")", string.Empty);
            }

            return propertyName;
        }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The expression types SQL server equivalent operator.
        /// </returns>
        private static string GetOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    return "AND";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Default:
                    return string.Empty;
                default:
                    throw new NotImplementedException();
            }
        }

        internal static object EvaluateExpression(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return ((ConstantExpression) expression).Value;
                case ExpressionType.Call:
                    return Expression.Lambda(expression).Compile().DynamicInvoke();
                case ExpressionType.ListInit:
                    var listInit = (ListInitExpression) expression;
                    return listInit.Initializers.Select(x => EvaluateExpression(x.Arguments.First())).ToList();
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression) expression;
                    if (memberExpression.Member.ReflectedType.FullName.StartsWith("System"))
                    {
                        var objMember = Expression.Convert(expression, typeof(object));
                        var getterLambda = Expression.Lambda<Func<object>>(objMember);
                        var getter = getterLambda.Compile();
                        return getter();
                    }
                    return memberExpression.Member.Name;
                case ExpressionType.Lambda:
                    return EvaluateExpression(((LambdaExpression) expression).Body);
                case ExpressionType.Convert:
                    return EvaluateExpression(((UnaryExpression) expression).Operand);
            }
            return null;
        }

        internal static object EvaluateCompiled(MemberExpression expression, Dictionary<string, string> aliases = null)
        {
            if (expression.Expression?.Type.MemberType == MemberTypes.TypeInfo)
            {
                var propertyName = GetAliasedPropertyName(expression, aliases);
                return new TablePropertyValue($"{propertyName}");
            }
            var converted = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(converted);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static string GetSafeParameterName(IList<QueryParameter> queryParameters, string propertyName, object propertyValue)
        {
            //do we have any existing parameter with same name, we'll have to then rename the parameter
            var qp = queryParameters.FirstOrDefault(x => x.PropertyName == propertyName);
            if (qp != null)
            {
                if (qp.PropertyValue != propertyValue)
                {
                    //change propertyName to something else
                    propertyName = propertyName +
                                    (queryParameters.Count(x => x.PropertyName == propertyName) + 1);
                }
            }
            return propertyName;
        }

        public static string GetAliasedPropertyName(MemberExpression memberExpression, Dictionary<string, string> aliases)
        {
            var propertyName = memberExpression.Member.Name;
            if (aliases == null)
                return propertyName;

            var tableName = Spruce.GetTableNameForType(memberExpression.Expression.Type);
            propertyName = memberExpression.Member.Name;
            var prefix = tableName;
            aliases?.TryGetValue(tableName, out prefix);

            return $"[{prefix}].[{propertyName}]";
        }
    }

    /// <summary>
    /// Class that models the data structure in coverting the expression tree into SQL and Params.
    /// </summary>
    internal class QueryParameter
    {
        public string LinkingOperator { get; set; }
        public string PropertyName { get; set; }
        public string ParameterName { get; set; }
        public object PropertyValue { get; set; }
        public string QueryOperator { get; set; }

        public bool SupportOperator { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParameter" /> class.
        /// </summary>
        /// <param name="linkingOperator">The linking operator.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="queryOperator">The query operator.</param>
        internal QueryParameter(string linkingOperator, string propertyName, object propertyValue, string queryOperator, string parameterName)
        {
            LinkingOperator = linkingOperator;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            QueryOperator = queryOperator;
            ParameterName = parameterName;
        }

        internal QueryParameter(bool supportOperator, string linkingOperator)
        {
            SupportOperator = supportOperator;
            LinkingOperator = linkingOperator;
        }
    }
}