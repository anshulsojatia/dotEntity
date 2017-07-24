using System;
using System.Reflection;
using NUnitLite;

namespace DotEntity.Tests.Runner
{
    class Program
    {
        static int Main(string[] args)
        {
            var assembly = typeof(DotEntityTest).GetTypeInfo().Assembly;
            return new AutoRun(assembly).Execute(args);
        }
    }
}