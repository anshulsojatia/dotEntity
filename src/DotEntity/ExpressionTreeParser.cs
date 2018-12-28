/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (ExpressionTreeParser.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotEntity.Extensions;

namespace DotEntity
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

        private int[] _callerObjectHashCodes;
        private int _notCount = 0;
        private bool _isBinaryOperation = false;

        private IList<QueryInfo> _queryInfo;

        public IList<QueryInfo> QueryInfoList => _queryInfo;

        private Dictionary<string, string> _aliases = null;

        private IList<QueryInfo> _previousQueryInfo;
        public ExpressionTreeParser(Dictionary<string, string> aliases = null, IList<QueryInfo> queryInfo = null)
        {
            _aliases = aliases;
            _queryInfo = new List<QueryInfo>();
            _previousQueryInfo = queryInfo;
        }

        #region Evaluators
        private bool IsCallerCalling(object obj)
        {
            var toCheck = obj;
            if (obj is MemberExpression)
                toCheck = ((MemberExpression)obj).Expression;

            return _callerObjectHashCodes.Contains(toCheck.GetHashCode());
        }

        private object Visit(Expression expression, out bool isProperty)
        {
            isProperty = false;
            
            
            if (expression is BinaryExpression)
                VisitBinary((BinaryExpression)expression);
            else if (expression is ConstantExpression)
                return VisitConstant((ConstantExpression)expression);
            else if (expression is MethodCallExpression)
                return VisitMethodCallExpression((MethodCallExpression)expression);
            else if (expression is InvocationExpression)
                return VisitInvocationExpression((InvocationExpression)expression);
            else if (expression is UnaryExpression)
                return VisitUnaryExpression((UnaryExpression)expression, out isProperty);
            else if (expression is ParameterExpression)
                VisitParameterExpression((ParameterExpression)expression);
            else if (expression is MemberExpression)
                return VisitMemberExpression((MemberExpression)expression, out isProperty);
            else if (expression is LambdaExpression)
                return VisitLambdaExpression((LambdaExpression)expression);
            else if (expression is ListInitExpression)
                return Compile(expression);
            else 
                VisitUnknown(expression);
            return null;
        }

        private void VisitBinary(BinaryExpression expression)
        {
            if (expression.NodeType != ExpressionType.AndAlso && expression.NodeType != ExpressionType.OrElse)
            {
                _isBinaryOperation = true;
                var left = expression.Left as MemberExpression;
                var propertyName = left != null ? GetAliasedPropertyName(left, _aliases) : Visit(expression.Left, out bool isProperty) as string;
                var propertyValue = Visit(expression.Right, out bool isValueProperty);
                var opr = GetOperator(expression.NodeType, this);
                AddQueryParameter("", propertyName, propertyValue, opr, propertyName, isValueProperty);
                _isBinaryOperation = false;
            }
            else
            {
                
                _queryInfo.Add(new QueryInfo(true, Markers.Open));
                var p = Visit(expression.Left, out bool isProperty);
                if (isProperty)
                {
                    var value = _notCount <= 0;
                    AddQueryParameter("", p.ToString(), value, "=", p.ToString());
                    DecrementNotCount();
                }
                _queryInfo.Add(new QueryInfo(true, Markers.Close));
                _queryInfo.Add(new QueryInfo(true, GetOperator(expression.NodeType, this)));
                
                _queryInfo.Add(new QueryInfo(true, Markers.Open));
                p = Visit(expression.Right, out isProperty);
                if (isProperty)
                {
                    var value = _notCount <= 0;
                    AddQueryParameter("", p.ToString(), value, "=", p.ToString());
                    DecrementNotCount();
                }
                _queryInfo.Add(new QueryInfo(true, Markers.Close));
                DecrementNotCount();
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
                        return EvaluateAndAdd(expression);
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
                                Throw.IfMethodNameNotSupported(expression.Method.Name);
                                break;
                        }
                        AddQueryParameter(linkingOperator, propertyName, propertyValue, opr, propertyName, isProperty);
                        return null;
                    }
                    Throw.IfExpressionTypeNotSupported(expression);
                }
            }
            return Compile(expression);
        }

        private object VisitInvocationExpression(InvocationExpression expression)
        {
            EvaluateAndAdd(expression);
            return null;
        }

        private object VisitUnaryExpression(UnaryExpression expression, out bool isProperty)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                _notCount++;
            }
            return Visit(expression.Operand, out isProperty);
        }

        private void DecrementNotCount()
        {
            if (_notCount > 0)
                _notCount--;
        }
        private string VisitParameterExpression(ParameterExpression expression)
        {
            return expression.Name;
            //
        }

        private object VisitMemberExpression(MemberExpression expression, out bool isProperty)
        {
            isProperty = false;
            if (expression.Expression == null || expression.Expression.NodeType == ExpressionType.Constant)
            {
                return Compile(expression);
            }
            if (IsCallerCalling(expression))
            {
                isProperty = true;
                var propertyName = GetAliasedPropertyName(expression, _aliases);
                if (expression.Type == typeof(bool))
                {
                    //we have a unary type of expression where the column itself is boolean
                    var qOperator = GetOperator(ExpressionType.Equal, this);
                    AddQueryParameter(qOperator, propertyName, true, qOperator, propertyName);
                    isProperty = false;
                    return null;
                }
                return propertyName;
            }
            if (expression.Expression.NodeType == ExpressionType.Parameter)
                return VisitParameterExpression((ParameterExpression)expression.Expression);


            if (expression.Member.MemberType == MemberTypes.Field || expression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                return Compile(expression);
            }
            return null;
        }

        private object Compile(Expression expression)
        {
            var objMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objMember);
            var getter = getterLambda.Compile();
            return getter();
        }
        private void VisitUnknown(Expression expression)
        {
            
        }

        private object VisitLambdaExpression(LambdaExpression expression)
        {

            if (expression.Parameters.Count > 0 && _callerObjectHashCodes == null)
            {
                _callerObjectHashCodes = new int[expression.Parameters.Count];
                for (var i = 0; i < expression.Parameters.Count; i++)
                {
                    _callerObjectHashCodes[i] = expression.Parameters[i].GetHashCode();
                }
            }

            //foreach (var parameter in expression.Parameters)
                
            return Visit(expression.Body, out bool isProperty);
        }

        private object EvaluateAndAdd(Expression expression)
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
                return null;
            }
            else
            {
                return propertyValue;
            }
        }

        private QueryInfo AddQueryParameter(string linkingOperator, string propertyName, object propertyValue, string queryOperator, string parameterName, bool isPropertyValueAlsoAProperty = false)
        {
            parameterName = GetSafeParameterName(_queryInfo, propertyName, propertyValue, _previousQueryInfo);
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

        private void ResetHashCodes()
        {
            _callerObjectHashCodes = null;
        }

        public string GetWhereString(Expression expression)
        {
            ResetHashCodes();
            Visit(expression, out bool isProperty);
            var builder = new StringBuilder();

            var extraQueryProperties = new List<QueryInfo>();
            foreach (var item in  _queryInfo.Where(x => !x.Processed))
            {
                item.Processed = true;
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
                        builder.Append($"{item.PropertyName.ToEnclosed()} {item.QueryOperator} (");
                        var itemCollection = (IList)item.PropertyValue;
                        
                        for (var itemIndex = 0; itemIndex < itemCollection.Count; itemIndex++)
                        {
                            var iterator = 1;
                            var paramName = $"{item.PropertyName}_InParam_{itemIndex + iterator}".Replace(".", "_");
                            while (_queryInfo.Any(x => x.ParameterName == paramName) || extraQueryProperties.Any(x => x.ParameterName == paramName))
                            {
                                iterator++;
                                paramName = $"{item.PropertyName}_InParam_{itemIndex + iterator}".Replace(".", "_");
                            }
                            var inItemValue = itemCollection[itemIndex];
                            var innerParam = new QueryInfo("", "inner_" + item.PropertyName, inItemValue, "",
                                paramName) {Processed = true};
                            extraQueryProperties.Add(innerParam);
                        }
                        var inParameterString = "@" + string.Join(",@", extraQueryProperties.Select(x => x.ParameterName));
                        builder.Append(inParameterString);
                        builder.Append(") ");
                    }
                    else
                    {
                        builder.Append($"{item.PropertyName.ToEnclosed()} {item.QueryOperator} ");
                        if (item.IsPropertyValueAlsoProperty)
                        {
                            builder.Append(item.PropertyValue.ToString().ToEnclosed());
                        }
                        else
                        {
                            switch (item.LinkingOperator)
                            {
                                case Markers.Contains:
                                case Markers.NotContains:
                                    builder.Append($"@{item.ParameterName}");
                                    item.PropertyValue = $"%{item.PropertyValue}%";
                                    break;
                                case Markers.StartsWith:
                                case Markers.NotStartsWith:
                                    builder.Append($"@{item.ParameterName}");
                                    item.PropertyValue = $"{item.PropertyValue}%";
                                    break;
                                case Markers.EndsWith:
                                case Markers.NotEndsWith:
                                    builder.Append($"@{item.ParameterName}");
                                    item.PropertyValue = $"%{item.PropertyValue}";
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

        public string GetOrderByString(Expression expression)
        {
            ResetHashCodes();
            var ob = Visit(expression, out bool _);
            return ob.ToString().ToEnclosed();
        }

        private static string GetSafeParameterName(IList<QueryInfo> queryParameters, string propertyName, object propertyValue, IList<QueryInfo> previousQueryParameters = null)
        {
            var parameterName = propertyName.Replace("[", "").Replace("]", "").Replace(".", "_");
            //do we have any existing parameter with same name, we'll have to then rename the parameter
            var qp = queryParameters.FirstOrDefault(x => x.PropertyName == propertyName);
            qp = qp ?? previousQueryParameters?.FirstOrDefault(x => x.PropertyName == propertyName);
            if (qp != null)
            {
                if (!qp.PropertyValue.Equals(propertyValue))
                {
                    //change propertyName to something else
                    //but first check if propertyname is aliased?
                    propertyName = propertyName.Replace(".", "_");
                    parameterName = propertyName +
                                   (queryParameters.Count(x => x.PropertyName == propertyName) + 1);
                }
            }
            return parameterName;
        }

        private static string GetAliasedPropertyName(MemberExpression memberExpression, Dictionary<string, string> aliases)
        {
            var propertyName = memberExpression.Member.Name;
            if (aliases == null)
                return propertyName;

            var tableName = DotEntityDb.GetTableNameForType(memberExpression.Expression.Type);
            propertyName = memberExpression.Member.Name;
            var prefix = tableName;
            aliases?.TryGetValue(tableName, out prefix);

            return $"{prefix}.{propertyName}";
        }

        #endregion;

    }
}