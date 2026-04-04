using BackEnd.Common;

namespace BackEnd.Models
{
    public class Product : Base
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; } = 0;
        public decimal OldPrice { get; set; } = 0;

        public int Stock { get; set; } = 0;
        public string ImageUrl { get; set; } = string.Empty;

        public Category Category { get; set; }
        public Guid CategoryId { get; set; }
    }
}
