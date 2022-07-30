using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class UserRolesDataModel
    {
        public string userId { get; set; }
        public string emailId { get; set; }
        public string userName { get; set; }
        public string fullName { get; set; }
        public string roleId { get; set; }
    }
}
