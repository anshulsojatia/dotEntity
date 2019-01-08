/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (QueryParserUtilities.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Runtime.CompilerServices;
using System.Text;
using DotEntity.Caching;
using DotEntity.Enumerations;
using DotEntity.Extensions;

namespace DotEntity
{
    public static class QueryParserUtilities
    {        
       
        internal static string[] ParseTypeKeyValues(Type type, params string[] exclude)
        {
            var props = type.GetDatabaseUsableProperties();
            var columns = props.Select(p => p.Name).Where(s => !exclude.Contains(s)).ToArray();
            var parameters = columns.Select(name => name + " = @" + name).ToArray();
            return parameters;
        }

        public static Dictionary<string, object> ParseObjectKeyValues(object obj, params string[] exclude)
        {
            if (obj == null)
                return null;
            Type typeOfObj = obj.GetType();
            var props = typeOfObj.GetDatabaseUsableProperties().ToArray();
            if(exclude.Any())
                props = props.Where(x => !exclude.Contains(x.Name)).ToArray();
            var getterMap = PropertyCallerCache.GetterOfType(typeOfObj);
            var dict = new Dictionary<string, object>();
            
            foreach(var p in props)
            {
                var propertyName = p.Name;

                var propertyValue = getterMap.Get<object>(obj, propertyName); // p.GetValue(obj);
                dict.Add(propertyName, propertyValue);
            }

            if (!props.Any())
            {
                dict = ((IDictionary<string, object>) obj).ToDictionary(x => x.Key, x => x.Value);
            }
            return dict;
        }

        public static string GetSelectColumnString(IList<Type> types, Dictionary<string, List<string>> typedAliases = null, params string[] exclude)
        {
            Throw.IfArgumentNull(types, nameof(types));
            
            if (DotEntityDb.SelectQueryMode == SelectQueryMode.Wildcard)
                return "*";

            return string.Join(",", types.Select(type =>
            {
                var props = type.GetDatabaseUsableProperties();
                var columns = props.Select(p => p.Name).Where(s => !exclude.Contains(s)).ToList();
                if (typedAliases == null)
                    return string.Join(",", columns);
                var tableName = type.Name;
                typedAliases.TryGetValue(tableName, out List<string> prefixes);
                if(prefixes == null)
                    return string.Join(",", columns);
                var builder = new List<string>();
                foreach (var p in prefixes)
                {
                    if (!string.IsNullOrEmpty(p))
                    {
                        var prefix = p + ".";
                        builder.Add(string.Join(",",
                            columns.Select(x => prefix + x.ToEnclosed() + " AS " + GetColumnAliasName(tableName, x))));
                    }
                }
                return string.Join(",", builder);
            }));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetColumnAliasName(string tableName, string columnName)
        {
            return tableName + "_" + columnName;
        }
    }
}