using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class BankDetailsModel
    {
        [Key]
        [Column(TypeName = "int")]
        public int bankId { get; set; }

        [Column(TypeName = "nvarchar(450)")]
        public string userId { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string bankname { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string branch { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string ifscCode { get; set; }
        [Column(TypeName = "nvarchar(20)")]
        public string accountNumber { get; set; }
        [Column(TypeName = "nvarchar(30)")]
        public string CardType { get; set; }
        [Column(TypeName = "decimal(38,4)")]
        public decimal RemainingBalance { get; set; }
       [Column(TypeName = "decimal(38,4)")]
        public decimal amountSpent { get; set; }
        [Column(TypeName = "nvarchar(18)")]
        public string cardNumber { get; set; }
        [Column(TypeName = "nvarchar(10)")]
        public string cardStatus { get; set; }
        [Column(TypeName = "nvarchar(10)")]
        public string Validity { get; set; }
        [Column(TypeName = "decimal(38,4)")]
        public decimal totalCredit { get; set; }
        public decimal InitialCredits { get; set; }
    }
}
