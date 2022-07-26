using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class ProductModel
    {
        [Key]
        public int productId { get;set;}
        [Column(TypeName ="varchar(300)")]
        public string ProductName { get; set; }
        [Column(TypeName = "nvarchar(Max)")]
        public string Img { get; set; }
        [Column(TypeName ="nvarchar(Max)")]
        public string ProductDescription { get; set; }
        [Column(TypeName = "decimal(8,2)")]
        public decimal Rating { get; set; }
        [Column(TypeName = "decimal(38,4)")]
        public decimal emiCost { get; set; }
        [Column(TypeName = "decimal(38,4)")]
        public decimal totalCost { get; set; }

    }
}
