using BackEnd.Common;

namespace BackEnd.Models
{
    public class Product : Base
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }

        public int Stock { get; set; }
        public string ImageUrl { get; set; }

        public Category Category { get; set; }
        public Guid CategoryId { get; set; }
    }
}
