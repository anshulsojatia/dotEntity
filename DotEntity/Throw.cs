// #region Author Information
// // Throw.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections;
using System.Linq.Expressions;
using DotEntity.Extensions;

namespace DotEntity
{
    internal static class Throw
    {
        public static void IfArgumentNull(object arg, string parameterName)
        {
            It<ArgumentNullException>(arg == null, $"Argument {parameterName} is null", parameterName);
        }

        public static void IfArgumentNullOrEmpty(string arg, string parameterName)
        {
            It<ArgumentNullException>(string.IsNullOrEmpty(arg), $"Argument {parameterName} is null or empty. A non-empty value must be provided", parameterName);
        }

        public static void IfObjectNull(object arg, string parameterName)
        {
            It<NullReferenceException>(arg == null, $"Field/Property/Variable {parameterName} is null and can not be referenced");
        }

        public static void IfKeyNull(string keyColumnName, Type arg)
        {
            It<InvalidOperationException>(string.IsNullOrEmpty(keyColumnName), $"The entity type {arg.FullName} doesn't have a Key specified");
        }

        public static void IfDbNotVersioned(bool versioned)
        {
            It<InvalidOperationException>(versioned, $"The database hasn't been versioned");
        }

        public static void IfKeyTypeNullable(Type arg, string keyColumnName)
        {
            It<InvalidOperationException>(arg.IsNullable(), $"The entity type {arg.FullName} has a Key defined on a Nullable type. A non-Nullable type should be used");
        }

        public static void IfInvalidPagination(ICollection orderBy, int page, int count)
        {
            It<InvalidOperationException>(page < 1 || count < 0 || 
                ((page > 1 || count < int.MaxValue) && (orderBy == null || orderBy.Count == 0)),
                $"The pagination is invalid with Page: {page}, Count: {count} and OrderBy: {orderBy}");
        }

        public static void IfCommitCalledOnNonTransactionalExecution(bool withTransaction)
        {
            It<InvalidOperationException>(!withTransaction, $"Can not call CommitTransaction on a non-transactional execution");
        }

        public static void IfNullOrDisposed(IWrappedDisposable disposable, string parameterName)
        {
            It<NullReferenceException>(disposable.IsNullOrDisposed(), $"Field/Property/Variable {parameterName} is null or disposed and can not be referenced");
        }

        public static void IfMethodNameNotSupported(string methodName)
        {
            It<NotSupportedException>(true, $"The method {methodName} is not supported");
        }

        public static void IfExpressionTypeNotSupported(Expression expression)
        {
            It<NotSupportedException>(true, $"The expression {expression} is not supported");
        }

        public static void IfEmptyBatch(bool isEmpty)
        {
            It<InvalidOperationException>(isEmpty, $"At least one operation must be executed within a batch");
        }

        public static void IfInvalidDataTypeMapping(bool isNotValid, Type type)
        {
            It<InvalidOperationException>(isNotValid, $"Can't find an equivalent database type for type {type.FullName}. Either mark the field as virtual or change the datatype to a more concrete type");
        }

        public static void It<TException>(bool condition, params object[] parameters) where TException : Exception
        {
            if (!condition) return;

            var ex = (TException)Activator.CreateInstance(typeof(TException), parameters);
            throw ex;
        }
    } 
}