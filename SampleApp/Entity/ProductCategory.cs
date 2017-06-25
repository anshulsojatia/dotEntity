// #region Author Information
// // ProductCategory.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion
namespace SampleApp.Entity
{
    public class ProductCategory
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int CategoryId { get; set; }

        public virtual Product Product { get; set; }

        public virtual Category Category { get; set; }
    }
}