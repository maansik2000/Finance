using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class UserDetailsModel
    {
        [Key]
        public int id { get; set; }
        [Column(TypeName = "nvarchar(450)")]
        public string userId { get; set; }
        public DateTime dateOfBirth { get; set; }
        public string phoneNumber { get; set; }
        public string UserAddress { get; set; }
   
        public bool isVerified { get; set; }
      
        public bool isActivated { get; set; }
    
        public bool joiningFees { get; set; }
        public DateTime createdAt { get; set; }
        public decimal joiningFeesAmount { get; set; }

    }
}
