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
using System.Runtime.InteropServices;
using System.Text;
using SpruceFramework;

namespace SampleApp.Expression
{
    internal static class Markers
    {
        public const string Open = "OPEN";
        public const string Close = "CLOSE";
        public const string Contains = "CONTAINS";
        public const string NotContains = "NOT_CONTAINS";
        public const string StartsWith = "STARTS_WITH";
        public const string NotStartsWith = "NOT_STARTS_WITH";
        public const string EndsWith = "ENDS_WITH";
        public const string NotEndsWith = "NOT_ENDS_WITH";
        public const string NoParam = "NOPARAM";
        public const string Not = "NOT";
    }

    public class ExpressionTreeParser
    {
        public class QueryInfo
        {
            public string LinkingOperator { get; set; }
            public string PropertyName { get; set; }
            public string ParameterName { get; set; }
            public object PropertyValue { get; set; }
            public string QueryOperator { get; set; }
            public bool IsPropertyValueAlsoProperty { get; set; }
            public bool SupportOperator { get; set; }
            /// <summary>
            /// Initializes a new instance of the <see cref="QueryInfo" /> class.
            /// </summary>
            /// <param name="linkingOperator">The linking operator.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="propertyValue">The property value.</param>
            /// <param name="queryOperator">The query operator.</param>
            internal QueryInfo(string linkingOperator, string propertyName, object propertyValue, string queryOperator, string parameterName, bool isPropertyValueAlsoAProperty = false)
            {
                LinkingOperator = linkingOperator;
                PropertyName = propertyName;
                PropertyValue = propertyValue;
                QueryOperator = queryOperator;
                ParameterName = parameterName;
                IsPropertyValueAlsoProperty = isPropertyValueAlsoAProperty;
            }

            internal QueryInfo(bool supportOperator, string linkingOperator)
            {
                SupportOperator = supportOperator;
                LinkingOperator = linkingOperator;
            }

            public override string ToString()
            {
                return $@"Query Info:
                        Linking Operator: {LinkingOperator}
                        Query Operator: {QueryOperator}
                        Property Name: {PropertyName}
                        Property Value: {PropertyValue}
                        Parameter Name: {ParameterName}
                        Is Property: {IsPropertyValueAlsoProperty}
                        Support: {SupportOperator}";
            }
        }


        private int _callerObjectHashCode = 0;
        private int _notCount = 0;
        private bool _isBinaryOperation = false;

        private IList<QueryInfo> _queryInfo;

        public IList<QueryInfo> QueryInfoList => _queryInfo;

        private Dictionary<string, string> _aliases = null;
        private readonly System.Linq.Expressions.Expression _expression;

        public ExpressionTreeParser(System.Linq.Expressions.Expression expression, Dictionary<string, string> aliases = null)
        {
            _expression = expression;
            _aliases = aliases;
            _queryInfo = new List<QueryInfo>();
        }

        #region Evaluators
        private bool IsCallerCalling(object obj)
        {
            var toCheck = obj;
            if (obj is MemberExpression)
                toCheck = ((MemberExpression)obj).Expression;

            return toCheck.GetHashCode() == _callerObjectHashCode;
        }

        private object Visit(System.Linq.Expressions.Expression expression, out bool isProperty)
        {
            isProperty = false;
            Console.WriteLine("\n");
            Console.WriteLine($"Visiting : {expression} {expression.NodeType}");
            if (expression is BinaryExpression)
                VisitBinary((BinaryExpression)expression);
            else if (expression is ConstantExpression)
                return VisitConstant((ConstantExpression)expression);
            else if (expression is MethodCallExpression)
                return VisitMethodCallExpression((MethodCallExpression)expression);
            else if (expression is InvocationExpression)
                return VisitInvocationExpression((InvocationExpression)expression);
            else if (expression is UnaryExpression)
                VisitUnaryExpression((UnaryExpression)expression);
            else if (expression is ParameterExpression)
                VisitParameterExpression((ParameterExpression)expression);
            else if (expression is MemberExpression)
                return VisitMemberExpression((MemberExpression)expression, out isProperty);
            else if (expression is LambdaExpression)
                VisitLambdaExpression((LambdaExpression)expression);
            else
                VisitUnknown(expression);
            return null;
        }

        private void VisitBinary(BinaryExpression expression)
        {
            if (expression.NodeType != ExpressionType.AndAlso && expression.NodeType != ExpressionType.OrElse)
            {
                _isBinaryOperation = true;
                var propertyName = VisitMemberExpression((MemberExpression)expression.Left, out bool isNameProperty);
                var propertyNameStr = propertyName as string ?? Convert.ToString(propertyName);
                var propertyValue = Visit(expression.Right, out bool isValueProperty);
                var opr = GetOperator(expression.NodeType, this);
                AddQueryParameter("", propertyNameStr, propertyValue, opr, propertyNameStr, isValueProperty);
                _isBinaryOperation = false;
            }
            else
            {
                Console.WriteLine($"Binary Left: {expression.Left} {expression.NodeType}");
                _queryInfo.Add(new QueryInfo(true, Markers.Open));
                Visit(expression.Left, out bool isLeftProperty);
                _queryInfo.Add(new QueryInfo(true, Markers.Close));
                _queryInfo.Add(new QueryInfo(true, GetOperator(expression.NodeType, this)));
                Console.WriteLine($"Binary Right: {expression.Right} {expression.NodeType}");
                _queryInfo.Add(new QueryInfo(true, Markers.Open));
                Visit(expression.Right, out bool isRightProperty);
                _queryInfo.Add(new QueryInfo(true, Markers.Close));
                _notCount--;
            }

        }

        private object VisitConstant(ConstantExpression expression)
        {
            if (expression.Value is bool)
            {
                EvaluateAndAdd(expression);
                return null;
            }

            return expression.Value;
        }

        private object VisitMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Object != null)
            {
                if (!IsCallerCalling(expression.Object))
                {
                    if (expression.Arguments.Count == 0)
                    {
                        EvaluateAndAdd(expression);
                    }
                    else
                    {
                        var propertyName = (string)Visit(expression.Arguments[0], out bool isProperty);
                        var propertyValue = Visit(expression.Object, out isProperty);
                        var operatorName = "";
                        if (expression.Method.Name == "Contains")
                        {
                            operatorName = _notCount > 0 ? "NOT IN" : "IN";
                        }

                        AddQueryParameter(Markers.Contains, propertyName, propertyValue, operatorName, propertyName, isProperty);
                        return null;
                    }

                }
                else
                {
                    var propertyName = (string)VisitMemberExpression(expression.Object as MemberExpression, out bool isProperty);
                    if (expression.Arguments[0].Type == typeof(string))
                    {
                        var propertyValue = Visit(expression.Arguments[0], out isProperty);
                        var linkingOperator = "";
                        var opr = _notCount > 0 ? "NOT LIKE" : "LIKE";
                        switch (expression.Method.Name)
                        {
                            case "Contains":
                                linkingOperator = _notCount > 0 ? Markers.NotContains : Markers.Contains;
                                break;
                            case "StartsWith":
                                linkingOperator = _notCount > 0 ? Markers.NotStartsWith : Markers.StartsWith;
                                break;
                            case "EndsWith":
                                linkingOperator = _notCount > 0 ? Markers.NotEndsWith : Markers.EndsWith;
                                break;
                            default:
                                throw new Exception("Unsupported method");
                        }
                        AddQueryParameter(linkingOperator, propertyName, propertyValue, opr, propertyName, isProperty);
                        return null;
                    }
                    throw new Exception("Unsupported expression");
                }
            }
            return Compile(expression);
        }

        private object VisitInvocationExpression(InvocationExpression expression)
        {
            EvaluateAndAdd(expression);
            return null;
        }

        private void VisitUnaryExpression(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                _notCount++;
                Visit(expression.Operand, out bool isProperty);

            }
            Console.WriteLine($"Unary {expression} : {expression.Method}");
        }

        private string VisitParameterExpression(ParameterExpression expression)
        {
            return expression.Name;
            //Console.WriteLine($"Parameter {expression} : {expression.Name}");
        }

        private object VisitMemberExpression(MemberExpression expression, out bool isProperty)
        {
            isProperty = false;
            if (expression.Expression == null)
            {
                return Compile(expression);
            }
            if (IsCallerCalling(expression))
            {
                isProperty = true;
                return expression.Member.Name;
            }
            Console.WriteLine($@"Member {expression} : {expression.Member.Name}" +
                $"\nNode Type: {expression.NodeType} \n" +
                $"Member Type: {expression.Member.MemberType} \n" +
                $"Reflected Type:{expression.Member.ReflectedType} \n" +
                $"Declaring Type:{expression.Member.DeclaringType} \n" +
                $"Expression:{expression.Expression.NodeType} \n" +
                $"Expression Type: {expression.Expression.Type}");


            if (expression.Expression.NodeType == ExpressionType.Parameter)
                return VisitParameterExpression((ParameterExpression)expression.Expression);
            if (expression.Member.MemberType == MemberTypes.Field || expression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                return Compile(expression);
            }
            return null;
        }

        private object Compile(System.Linq.Expressions.Expression expression)
        {
            var objMember = System.Linq.Expressions.Expression.Convert(expression, typeof(object));
            var getterLambda = System.Linq.Expressions.Expression.Lambda<Func<object>>(objMember);
            var getter = getterLambda.Compile();
            return getter();
        }
        private void VisitUnknown(System.Linq.Expressions.Expression expression)
        {
            Console.WriteLine($"Unknown {expression} : {expression.GetType()}");
        }

        private void VisitLambdaExpression(LambdaExpression expression)
        {
            Console.WriteLine($"Lambda {expression}");
            if (expression.Parameters.Count == 1 && _callerObjectHashCode == 0)
                _callerObjectHashCode = expression.Parameters[0].GetHashCode();

            foreach (var parameter in expression.Parameters)
                Console.WriteLine($"->Parameter : {parameter.Name} {parameter.GetHashCode()}");
            Visit(expression.Body, out bool isProperty);
        }

        private void EvaluateAndAdd(System.Linq.Expressions.Expression expression)
        {
            var propertyValue = Compile(expression);
            if (propertyValue is bool)
            {
                if (!_isBinaryOperation)
                {
                    var opr = "";
                    if ((bool)propertyValue)
                    {
                        opr = GetOperator(ExpressionType.Equal, this);
                    }
                    else
                    {
                        opr = GetOperator(ExpressionType.NotEqual, this);
                    }
                    AddQueryParameter("", "1", "1", opr, "1", true);
                }
            }
            else
            {
                throw new Exception("Unsupported expression");
            }
        }

        private QueryInfo AddQueryParameter(string linkingOperator, string propertyName, object propertyValue, string queryOperator, string parameterName, bool isPropertyValueAlsoAProperty = false)
        {
            parameterName = GetSafeParameterName(_queryInfo, propertyName, propertyValue);
            var queryInfo = new QueryInfo(linkingOperator, propertyName, propertyValue, queryOperator, parameterName, isPropertyValueAlsoAProperty);
            _queryInfo.Add(queryInfo);
            return queryInfo;
        }

        private static string GetOperator(ExpressionType type, ExpressionTreeParser parser)
        {
            var typeToCheck = type;
            if (parser._notCount > 0)
            {
                switch (type)
                {
                    case ExpressionType.Equal:
                        typeToCheck = ExpressionType.NotEqual;
                        break;
                    case ExpressionType.NotEqual:
                        typeToCheck = ExpressionType.Equal;
                        break;
                    case ExpressionType.LessThan:
                        typeToCheck = ExpressionType.GreaterThan;
                        break;
                    case ExpressionType.LessThanOrEqual:
                        typeToCheck = ExpressionType.GreaterThanOrEqual;
                        break;
                    case ExpressionType.GreaterThan:
                        typeToCheck = ExpressionType.LessThan;
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        typeToCheck = ExpressionType.LessThanOrEqual;
                        break;
                    case ExpressionType.AndAlso:
                    case ExpressionType.And:
                        typeToCheck = ExpressionType.Or;
                        break;
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        typeToCheck = ExpressionType.And;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            switch (typeToCheck)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
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
        #endregion

        #region query part generators

        public string GetWhereString()
        {
            Visit(_expression, out bool isProperty);
            var builder = new StringBuilder();

            var extraQueryProperties = new List<QueryInfo>();
            for (var i = 0; i < _queryInfo.Count(); i++)
            {
                var item = QueryInfoList[i];
                if (item.SupportOperator)
                {
                    switch (item.LinkingOperator)
                    {
                        case Markers.Open:
                            builder.Append("(");
                            break;
                        case Markers.Close:
                            builder.Append(")");
                            break;
                        default:
                            builder.Append($" {item.LinkingOperator} ");
                            break;
                    }
                }
                else
                {
                    if (item.PropertyValue is ICollection)
                    {
                        builder.Append($"{item.PropertyName} {item.QueryOperator} (");
                        var itemCollection = (IList)item.PropertyValue;
                        for (var itemIndex = 0; itemIndex < itemCollection.Count; itemIndex++)
                        {
                            var paramName = "InParam_" + (itemIndex + 1);
                            var inItemValue = itemCollection[itemIndex];
                            extraQueryProperties.Add(new QueryInfo("", "inner_" + item.PropertyName, inItemValue, "", paramName));
                        }
                        var inParameterString = "@" + string.Join(",@", extraQueryProperties.Select(x => x.ParameterName));
                        builder.Append(inParameterString);
                        builder.Append(") ");
                    }
                    else
                    {
                        builder.Append($"{item.PropertyName} {item.QueryOperator} ");
                        if (item.IsPropertyValueAlsoProperty)
                        {
                            builder.Append(item.PropertyValue);
                        }
                        else
                        {
                            switch (item.LinkingOperator)
                            {
                                case Markers.Contains:
                                case Markers.NotContains:
                                    builder.Append($"'%' + @{item.ParameterName} + '%'");
                                    break;
                                case Markers.StartsWith:
                                case Markers.NotStartsWith:
                                    builder.Append($"@{item.ParameterName} + '%'");
                                    break;
                                case Markers.EndsWith:
                                case Markers.NotEndsWith:
                                    builder.Append($"'%' + @{item.ParameterName}");
                                    break;
                                default:
                                    builder.Append($"@{item.ParameterName}");
                                    break;
                            }
                        }

                    }

                }
            }

            //add any extra parameters to the response
            _queryInfo = _queryInfo.Concat(extraQueryProperties).ToList();
            return builder.ToString();
        }

        internal static bool IncludeParamSymbol(QueryInfo parameter, out string value)
        {
            value = null;
            if (parameter.ParameterName == Markers.NoParam)
                value = parameter.PropertyValue.ToString();
            var propertyValue = parameter.PropertyValue as TablePropertyValue;
            if (propertyValue != null)
                value = propertyValue.Value;
            return value != null;
        }

        public string ParseAsOrderByString(LambdaExpression orderBy)
        {
            if (orderBy == null)
                return string.Empty;

            var ob = ""; // EvaluateExpression(orderBy);
            return ob?.ToString();
        }

        private static string GetSafeParameterName(IList<QueryInfo> queryParameters, string propertyName, object propertyValue)
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

        private static string GetAliasedPropertyName(MemberExpression memberExpression, Dictionary<string, string> aliases)
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

        #endregion;

    }
}