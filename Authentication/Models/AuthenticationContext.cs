
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Models
{
    public class AuthenticationContext : IdentityDbContext
    {
        public AuthenticationContext(DbContextOptions<AuthenticationContext> options):base(options)
        {

        }

        public DbSet<BankDetailsModel> BankDetails { get; set; }
        public DbSet<PaymentDetail> PaymentDetails { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserDetailsModel> UserDetails { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<EMImodels> EMI { get; set; }
        public DbSet<TransactionsModel> Transactions { get; set; }
        public DbSet<OrdersModel> Orders { get; set; }
    }
}
