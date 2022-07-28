using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class TransactionsModel
    {
        [Key]
        public Guid transactionId { get; set; }
        public string userId { get; set; }
        public string TansactionStatus { get; set; }
        public decimal amountPaid { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ProductName { get; set; }
        public int productId { get; set; }
    }
}
