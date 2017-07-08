// #region Author Information
// // ProductCategory.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.ComponentModel.DataAnnotations;

namespace DotEntity.Tests.Data
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int CategoryId { get; set; }
    }
}