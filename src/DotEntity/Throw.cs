/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (Throw.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using DotEntity.Extensions;
using DotEntity.Versioning;

namespace DotEntity
{
    public static class Throw
    {
        public class ThrowInfo
        {         
            public object[] Parameters { get; set; }

            public ThrowInfo(string message, params object[] parameters)
            {
                Parameters = new object[parameters.Length + 1];
                Parameters[0] = message;
                for (var i = 1; i < parameters.Length; i++)
                    Parameters[i] = parameters[i];
            }
        }

        public static void IfArgumentNull(object arg, string parameterName)
        {
            It<ArgumentNullException>(arg == null,
                () => new ThrowInfo($"Argument {parameterName} is null", parameterName));
        }

        public static void IfArgumentNullOrEmpty(string arg, string parameterName)
        {
            It<ArgumentNullException>(string.IsNullOrEmpty(arg),
                () => new ThrowInfo($"Argument {parameterName} is null or empty. A non-empty value must be provided",
                    parameterName));
        }

        public static void IfObjectNull(object arg, string parameterName)
        {
            It<NullReferenceException>(arg == null,
                () => new ThrowInfo($"Field/Property/Variable {parameterName} is null and can not be referenced"));
        }

        public static void IfKeyNull(string keyColumnName, Type arg)
        {
            It<InvalidOperationException>(string.IsNullOrEmpty(keyColumnName),
                () => new ThrowInfo($"The entity type {arg.FullName} doesn't have a Key specified"));
        }

        public static void IfDbNotVersioned(bool versioned)
        {
            It<InvalidOperationException>(versioned, () => new ThrowInfo("The database hasn't been versioned"));
        }

        public static void IfKeyTypeNullable(Type arg, string keyColumnName)
        {
            It<InvalidOperationException>(arg.IsNullable(),
                () => new ThrowInfo(
                    $"The entity type {arg.FullName} has a Key defined on a Nullable type. A non-Nullable type should be used"));
        }

        public static void IfInvalidPagination(ICollection orderBy, int page, int count)
        {
            It<InvalidOperationException>(page < 1 || count < 0 ||
                                          ((page > 1 || count < int.MaxValue) &&
                                           (orderBy == null || orderBy.Count == 0)),
                () => new ThrowInfo(
                    $"The pagination is invalid with Page: {page}, Count: {count} and OrderBy: {orderBy}"));
        }

        public static void IfCommitCalledOnNonTransactionalExecution(bool withTransaction)
        {
            It<InvalidOperationException>(!withTransaction,
                () => new ThrowInfo($"Can not call CommitTransaction on a non-transactional execution"));
        }

        public static void IfNullOrDisposed(IWrappedDisposable disposable, string parameterName)
        {
            It<NullReferenceException>(disposable.IsNullOrDisposed(),
                () => new ThrowInfo(
                    $"Field/Property/Variable {parameterName} is null or disposed and can not be referenced"));
        }

        public static void IfMethodNameNotSupported(string methodName)
        {
            It<NotSupportedException>(true, () => new ThrowInfo($"The method {methodName} is not supported"));
        }

        public static void IfExpressionTypeNotSupported(Expression expression)
        {
            It<NotSupportedException>(true, () => new ThrowInfo($"The expression {expression} is not supported"));
        }

        public static void IfEmptyBatch(bool isEmpty)
        {
            It<InvalidOperationException>(isEmpty,
                () => new ThrowInfo($"At least one operation must be executed within a batch"));
        }

        public static void IfInvalidDataTypeMapping(bool isNotValid, Type type)
        {
            It<InvalidOperationException>(isNotValid,
                () => new ThrowInfo(
                    $"Can't find an equivalent database type for type {type.FullName}. Either mark the field as virtual or change the datatype to a more concrete type"));
        }

        public static void IfNoDatabaseVersions(Queue<IDatabaseVersion> databaseVersions)
        {
            It<InvalidOperationException>(databaseVersions == null || !databaseVersions.Any(),
                () => new ThrowInfo(
                    $"Version upgrades or downgrades can't be done with no versions. Use {nameof(DotEntityDb)}.{nameof(DotEntityDb.EnqueueVersions)} to add some database versions"));

        }

        public static void IfTableCreated(bool created, string parameterName)
        {
            It<InvalidOperationException>(created,
                () => new ThrowInfo($"Table {parameterName} has been already created earlier.", parameterName));
        }

        public static void IfTransactionIsNullOrDisposed(bool isNullOrDisposed, string parameterName)
        {
            It<ArgumentNullException>(isNullOrDisposed,
                () => new ThrowInfo($"Transaction is null or has been disposed.", parameterName));
        }

        public static void It<TException>(bool condition, Func<ThrowInfo> getParameters) where TException : Exception
        {
            if (!condition) return;

            var messageInfo = getParameters();
            var ex = (TException)Activator.CreateInstance(typeof(TException), messageInfo.Parameters);
            throw ex;
        }

        public static void IfInvalidValue(bool invalidValue, string parameterName)
        {
            It<NotSupportedException>(invalidValue, () => new ThrowInfo($"The value for parameter {parameterName} is not valid"));
        }
    } 
}