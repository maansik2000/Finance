using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class ApplicationSettings
    {
        public string JWT_Secret_Code { get; set; }
        public string clientUrl { get; set; }
    }
}
