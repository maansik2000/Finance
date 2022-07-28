using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class EmiPaymentModel
    {
        public Guid orderId { get; set; }
        public string userId { get; set; }
        public Guid EmiId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
