using BackEnd.Common;

namespace BackEnd.Models
{
    public class CartItem : Base
    {
        public Cart Cart { get; set; }
        public Guid CartId { get; set; }

        public Product Product { get; set; }
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }

}

