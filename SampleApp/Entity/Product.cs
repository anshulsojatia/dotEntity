// #region Author Information
// // Product.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Entity
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

        public virtual IList<ProductCategory> ProductCategories { get; set; }
    }
}