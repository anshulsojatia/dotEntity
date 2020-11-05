using System.ComponentModel.DataAnnotations;

namespace DotEntity.Tests.Data
{
    public class Customer
    {
        [Key]
        public string CustomerId { get; set; }

        public string Name { get; set; }
    }
}