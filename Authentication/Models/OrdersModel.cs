using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class OrdersModel
    {
        [Key]
        public Guid orderId { get; set; }
        public string userId { get; set; }
        public Guid transactionId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ProductName { get; set; }
        public decimal totalPrice { get; set; }
        public int productId { get; set; }
        public DateTime createdAt { get; set; }
    }
}
