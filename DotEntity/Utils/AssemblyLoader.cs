/**
 * Copyright(C) 2017  Apexol Technologies
 * 
 * This file (AssemblyLoader.cs) is part of dotEntity(https://github.com/RoastedBytes/dotentity).
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

 * You can release yourself from the requirements of the license by purchasing
 * a commercial license.Buying such a license is mandatory as soon as you
 * develop commercial software involving the dotEntity software without
 * disclosing the source code of your own applications.
 * To know more about our commercial license email us at support@roastedbytes.com or
 * visit http://dotentity.net/legal/commercial
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

#if NETSTANDARD15
using Microsoft.Extensions.DependencyModel;
#endif

namespace DotEntity.Utils
{
    public class AssemblyLoader
    {
#if !NETSTANDARD15
        public static List<Assembly> GetAppDomainAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().ToList();
        }
#else
        public static List<Assembly> GetAppDomainAssemblies()
        {
            var assemblies = new List<Assembly>();
            return assemblies;
        }
        private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        {
            return compilationLibrary.Name == ("DotEntity")
                   || compilationLibrary.Dependencies.Any(d => d.Name.StartsWith("DotEntity"));
        }
#endif
        public static Assembly LoadAssembly(string assemblyName)
        {
#if NETSTANDARD15
            return Assembly.Load(new AssemblyName(assemblyName));
#else
            return Assembly.Load(assemblyName);
#endif
        }

        public static bool IsSystemAssembly(string assemblyName)
        {
            return Regex.IsMatch(assemblyName, @"^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^DryIoc|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        }
    }
}