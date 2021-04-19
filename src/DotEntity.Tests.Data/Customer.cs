using System.ComponentModel.DataAnnotations;

namespace DotEntity.Tests.Data
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        public string Name { get; set; }

        public int Uid { get; set; }
    }
}