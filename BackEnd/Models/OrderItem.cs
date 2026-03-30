using BackEnd.Common;

namespace BackEnd.Models
{
    public class OrderItem : Base
    {
        public Product Product { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus OrderItemStatus { get; set; }
        public Guid OrderItemStatusId { get; set; }
    }
}