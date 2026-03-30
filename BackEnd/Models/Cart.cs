using BackEnd.Common;

namespace BackEnd.Models
{
    public class Cart : Base
    {
        public User User { get; set; }
        public Guid UserId { get; set; }
    }

}