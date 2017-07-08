// #region Author Information
// // AssemblyLoader.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

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