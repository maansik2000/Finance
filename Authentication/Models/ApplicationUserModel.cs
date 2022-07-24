using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class ApplicationUserModel
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string fullName { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string phoneNumber { get; set; }
        public string UserAddress { get; set; }

        public string bankname { get; set; }
  
        public string branch { get; set; }

        public string ifscCode { get; set; }
 
        public string accountNumber { get; set; }
    
        public string CardType { get; set; }
        public string roles { get; set; }
    }
}
