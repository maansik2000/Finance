using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class ResetPasswordModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string token { get; set; }
    }
}
