/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (TypeExtensions.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
 * a commercial license (dotEntity or dotEntity Pro). Buying such a license is mandatory as soon as you
 * develop commercial activities involving the dotEntity software without
 * disclosing the source code of your own applications. The activites include:
 * shipping dotEntity with a closed source product, offering paid services to customers
 * as an Application Service Provider.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/licensing
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotEntity.Extensions
{
    public static class TypeExtensions
    {
        internal static DotEntityQueryManager AsDotEntityQueryManager(this IDotEntityQueryManager manager)
        {
            var asObj = manager as DotEntityQueryManager;
            Throw.IfArgumentNull(asObj, nameof(manager));
            return asObj;
        }

        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static IEnumerable<PropertyInfo> GetDatabaseUsableProperties(this Type type)
        {
            var excludedColumns = DotEntityDb.GetIgnoredColumns(type);
            var allProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => !x.GetAccessors()[0].IsVirtual);
            if (excludedColumns != null)
                allProps = allProps.Where(prop => !excludedColumns.Contains(prop.Name));

            return type.IsAnonymousType() ? allProps : allProps.Where(x => x.CanWrite);
        }

        public static bool IsAnonymousType(this Type type)
        {
            var hasCompilerGeneratedAttribute = type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            var isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
            return isAnonymousType;
        }
    }
}