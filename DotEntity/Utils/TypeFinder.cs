// #region Author Information
// // TypeFinder.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

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