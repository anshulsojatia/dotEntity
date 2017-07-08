// #region Author Information
// // Category.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System.ComponentModel.DataAnnotations;

namespace SampleApp.Core.Entity
{
    public class Category
    {
        public string CategoryName { get; set; }

        [Key]
        public int Id { get; set; }
    }
}