// #region Author Information
// // Category.cs
// // 
// // (c) Sojatia Infocrafts Private Limited. All Rights Reserved.
// // 
// #endregion

using System.ComponentModel.DataAnnotations;

namespace DotEntity.Benchmarks.Entity
{
    public class Category
    {
        public string CategoryName { get; set; }

        [Key]
        public int Id { get; set; }
    }
}