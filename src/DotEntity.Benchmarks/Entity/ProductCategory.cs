// #region Author Information
// // ProductCategory.cs
// // 
// // (c) Sojatia Infocrafts Private Limited. All Rights Reserved.
// // 
// #endregion

using System.ComponentModel.DataAnnotations;

namespace DotEntity.Benchmarks.Entity
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public virtual Product Product { get; set; }

        public virtual Category Category { get; set; }
    }
}