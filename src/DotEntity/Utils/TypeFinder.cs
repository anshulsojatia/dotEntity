/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (TypeFinder.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotEntity.Utils
{
    internal static class TypeFinder
    {
        private static IList<Assembly> _allAssemblies;

        private static IList<Type> OfType<T>(bool excludeAbstract = true)
        {
            var loadedTypes = new List<Type>();
            foreach (var assembly in _allAssemblies)
            {
                //exclude if it's a system assembly
                if (AssemblyLoader.IsSystemAssembly(assembly.FullName))
                    continue;
                try
                {
                    //if error occurs while loading the assembly, continue or throw error
#if NETSTANDARD15
                    var types = assembly.GetTypes().Where(x => x.GetTypeInfo().IsClass).ToList();
#else
                    var types = assembly.GetTypes().Where(x => x.IsClass).ToList();
#endif
                    foreach (var type in types)
                    {
#if NETSTANDARD15
                        if (excludeAbstract && type.GetTypeInfo().IsClass && type.GetTypeInfo().IsAbstract)
                            continue;

                        if (typeof(T).IsAssignableFrom(type) || typeof(T).GetTypeInfo().IsGenericType)
                        {
                            loadedTypes.Add(type);
                        }
#else
                        if (excludeAbstract && type.IsClass && type.IsAbstract)
                            continue;

                        if (typeof(T).IsAssignableFrom(type) || typeof(T).IsGenericType)
                        {
                            loadedTypes.Add(type);
                        }
#endif

                    }

                }
                catch
                {
                    // ignored
                }
            }
            return loadedTypes;
        }

        public static IList<Type> ClassesOfType<T>(bool excludeAbstract = true)
        {
            _allAssemblies = AssemblyLoader.GetAppDomainAssemblies();
            return OfType<T>(excludeAbstract);
        }
    }
}