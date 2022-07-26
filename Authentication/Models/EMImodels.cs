using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class EMImodels
    {
        [Key]
        public Guid EmiId { get; set; }
        public string userId { get; set; }
        public Guid orderId { get; set; }
        public DateTime emiInitialDate { get; set; }
        public decimal totalAmoubt { get; set; }
        public decimal emiAmount { get; set; }
        public decimal amountPaid { get; set; }
        public int productId { get; set; }
        public DateTime createdAt { get; set; }
        public decimal remainingBalance { get; set; }
        public bool isEmiCompleted { get; set; }
        public int emiPeriod { get; set; }
        public DateTime emiNextDate { get; set; }
    }
}
