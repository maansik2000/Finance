using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class ApiResponse
    {
        public object res { get; set; }
        public bool success { get; set; }
        public string message { get; set; }
    }
}
