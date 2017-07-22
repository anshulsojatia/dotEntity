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
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using DotEntity.Extensions;
using DotEntity.Versioning;

namespace DotEntity
{
    public static class Throw
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

        public static void IfNoDatabaseVersions(ConcurrentQueue<IDatabaseVersion> databaseVersions)
        {
            It<InvalidOperationException>(databaseVersions == null || databaseVersions.IsEmpty,
                $"Version upgrades or downgrades can't be done with no versions. Use {nameof(DotEntityDb)}.{nameof(DotEntityDb.EnqueueVersions)} to add some database versions");

        }

        public static void It<TException>(bool condition, params object[] parameters) where TException : Exception
        {
            if (!condition) return;

            var ex = (TException)Activator.CreateInstance(typeof(TException), parameters);
            throw ex;
        }
    } 
}