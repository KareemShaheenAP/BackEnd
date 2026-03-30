using BackEnd.Common;

namespace BackEnd.Models
{
    public class Category : Base
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
