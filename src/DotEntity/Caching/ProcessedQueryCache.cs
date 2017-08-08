﻿/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (ProcessedQueryCache.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DotEntity.Caching
{
    internal static class ProcessedQueryCache
    {

        private static readonly ConcurrentDictionary<string, Dictionary<string, int>> Cache;
        static ProcessedQueryCache()
        {
            Cache = new ConcurrentDictionary<string, Dictionary<string, int>>();
        }

        internal static bool TryGet(string commandText, out Dictionary<string, int> columnOrdinals)
        {
            return Cache.TryGetValue(commandText, out columnOrdinals);
        }

        internal static void TrySet(string commandText, Dictionary<string, int> columnOrdinals)
        {
            Cache.TryAdd(commandText, columnOrdinals);
        }
    }
}