using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SampleApp.Entity;
using SpruceFramework;
using SpruceFramework.Extensions;
using SpruceFramework.Providers;
using SpruceFramework.Reflection;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Spruce.Initialize(
                @"Data Source=.\sqlexpress;Initial Catalog=spruce_framework;Integrated Security=False;Persist Security Info=False;User ID=iis_user;Password=iis_user",
                new SqlServerDatabaseProvider());


            var s = new Stopwatch();

            s.Start();
            SpruceTable<Product>.Query("SELECT * FROM Product WHERE Id < @Id", new {Id = 10});
            Console.WriteLine("Time Taken :{0}ms", s.ElapsedMilliseconds);
            s.Reset();


            s.Start();
            SpruceTable<Product>.Where(x => x.Id < 10).Select();
            Console.WriteLine("Time Taken :{0}ms", s.ElapsedMilliseconds);
            s.Reset();

           


            Console.ReadKey();


        }


    }
}
