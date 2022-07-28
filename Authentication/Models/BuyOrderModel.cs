using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class BuyOrderModel
    {
        public string userId { get; set; }
        public int productId { get; set; }
        public DateTime currentDate { get; set; }
        public decimal emiAmount { get; set; }
        public int emiPeriod { get; set; }
    }
}
