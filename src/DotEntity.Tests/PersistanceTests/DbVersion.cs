using DotEntity.Tests.Data;
using DotEntity.Versioning;

namespace DotEntity.Tests.PersistanceTests
{
    public class DbVersion : IDatabaseVersion
    {
        public string VersionKey => "TestKey";
        public void Upgrade(IDotEntityTransaction transaction)
        {
            DotEntity.Database.CreateTables(new[]
            {
                typeof(Product),
                typeof(Category),
                typeof(ProductCategory)
            }, transaction);

            DotEntity.Database.CreateConstraint(Relation.Create<Product, ProductCategory>("Id", "ProductId"), transaction);
            DotEntity.Database.CreateConstraint(Relation.Create<Category, ProductCategory>("Id", "CategoryId"), transaction);
        }

        public void Downgrade(IDotEntityTransaction transaction)
        {
            DotEntity.Database.DropConstraint(Relation.Create<Product, ProductCategory>("Id", "ProductId"), transaction);
            DotEntity.Database.DropConstraint(Relation.Create<Category, ProductCategory>("Id", "CategoryId"), transaction);
            DotEntity.Database.DropTable<ProductCategory>(transaction);
            DotEntity.Database.DropTable<Product>(transaction);
            DotEntity.Database.DropTable<Category>(transaction);
        }
    }
}