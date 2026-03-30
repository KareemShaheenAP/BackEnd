using BackEnd.Common;

namespace BackEnd.Models
{
    public class Order : Base
    {
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public decimal TotalFee { get; set; } = 0;
        public OrderStatus OrderStatus { get; set; }
        public Guid OrderStatusId { get; set; }

    }
}