using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class AdminModel
    {
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string fullName { get; set; }
        public string roles { get; set; }
    }
}
