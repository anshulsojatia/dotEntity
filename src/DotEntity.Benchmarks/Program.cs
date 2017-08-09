using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Running;

namespace DotEntity.Benchmarks
{
    public class Program
    {
        static void Main(string[] args)
        {
            WriteLine("=======================================================================================================================", ConsoleColor.Green);
            WriteLine("========================================  dotEntity Benchmarks  =======================================================", ConsoleColor.Green);
            WriteLine("=======================================================================================================================", ConsoleColor.Green);
#if DEBUG
            Write($"You are running benchmarks in debugging mode. This might not give accurate results. Continue (Y/N)?", ConsoleColor.Red);
            var key = Console.ReadKey();
            WriteLine();
            if (key.Key != ConsoleKey.Y)
            {
                Write("Good byee!...");
                Console.ReadKey();
                return;
            }
#endif
            var benchmarks = new List<Benchmark>();
            var benchmarkAssemblyTypes = typeof(Program).Assembly.DefinedTypes.Where(t => t.IsSubclassOf(typeof(BenchmarkSetup)));

            foreach (var type in benchmarkAssemblyTypes)
            {
                var typeBenchmarks = BenchmarkConverter.TypeToBenchmarks(type);
                benchmarks.AddRange(typeBenchmarks);
            }

            BenchmarkRunner.Run(benchmarks.ToArray(), null);
            Write("Press any key to exit...");
            Console.ReadKey();
        }

        static void WriteLine(string message = "", ConsoleColor color = ConsoleColor.White)
        {
            Write(message, color);
            Console.WriteLine();
        }
        static void Write(string message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}