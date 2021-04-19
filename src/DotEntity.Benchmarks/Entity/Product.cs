// #region Author Information
// // Product.cs
// // 
// // (c) Sojatia Infocrafts Private Limited. All Rights Reserved.
// // 
// #endregion

using System;
using System.ComponentModel.DataAnnotations;

namespace DotEntity.Benchmarks.Entity
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public DateTime DateCreated { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }
}