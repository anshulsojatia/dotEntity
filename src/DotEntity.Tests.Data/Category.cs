// #region Author Information
// // Category.cs
// // 
// // (c) Sojatia Infocrafts Private Limited. All Rights Reserved.
// // 
// #endregion

using System.ComponentModel.DataAnnotations;

namespace DotEntity.Tests.Data
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string CategoryName { get; set; }

        public CategoryType CategoryType { get; set; }
    }
}