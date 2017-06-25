using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SampleApp.Entity;
using SpruceFramework;
using SpruceFramework.Providers;

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
            var p1 = SpruceTable<Product>.Query("SELECT * FROM Product WHERE Id < @Id", new {Id = 10});
            Console.WriteLine("Time Taken :{0}ms", s.ElapsedMilliseconds);
            s.Reset();


            s.Start();
            var p2 = SpruceTable<Product>.Where(x => x.Id < 10).Select();
            Console.WriteLine("Time Taken :{0}ms", s.ElapsedMilliseconds);
            s.Reset();
            
            s.Start();
            var p = SpruceTable<Product>.Where(x => true)
                .Join<ProductCategory>("Id", "ProductId")
                .Join<Category>("CategoryId", "Id")
                .Relate<ProductCategory>((product, category) =>
                {
                    if (product.ProductCategories == null)
                        product.ProductCategories = new List<ProductCategory>();
                    product.ProductCategories.Add(category);
                })
                .Relate<Category>((product, category) =>
                {
                    var pc = product.ProductCategories.FirstOrDefault(x => x.CategoryId == category.Id);
                    if (pc != null)
                    {
                        pc.Category = category;
                        pc.Product = product;
                    }
                })
                .SelectNested();
            Console.WriteLine("Time Taken :{0}ms", s.ElapsedMilliseconds);
            s.Reset();

            s.Start();
            p = SpruceTable<Product>.Where(x => x.IsActive)
                .Join<ProductCategory>("Id", "ProductId")
                .Join<Category>("CategoryId", "Id")
                .Relate<ProductCategory>((product, category) =>
                {
                    if (product.ProductCategories == null)
                        product.ProductCategories = new List<ProductCategory>();
                    product.ProductCategories.Add(category);
                })
                .Relate<Category>((product, category) =>
                {
                    var pc = product.ProductCategories.FirstOrDefault(x => x.CategoryId == category.Id);
                    if (pc != null)
                    {
                        pc.Category = category;
                        pc.Product = product;
                    }
                })
                .SelectNested();


            Console.WriteLine("Time Taken :{0}ms", s.ElapsedMilliseconds);
            s.Reset();

            Console.ReadKey();


        }


    }
}
