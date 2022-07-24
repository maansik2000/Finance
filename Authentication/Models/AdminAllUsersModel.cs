using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class AdminAllUsersModel
    {
        public string userid { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string fullName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string phoneNumber { get; set; }
        public string UserAddress { get; set; }

        public string bankname { get; set; }

        public string branch { get; set; }

        public string ifscCode { get; set; }

        public string accountNumber { get; set; }

        public string CardType { get; set; }

        public decimal RemainingBalance { get; set; }

        public decimal amountSpent { get; set; }

        public string cardNumber { get; set; }
   
        public string cardStatus { get; set; }
  
        public string Validity { get; set; }
        public bool isActivated { get; set; }
        public bool isVerified { get; set; }
        public bool joiningFees { get; set; }
        public DateTime createdAt { get; set; }
        public decimal totalCredit { get; set; }
        public string role { get; set; }
    }
}
