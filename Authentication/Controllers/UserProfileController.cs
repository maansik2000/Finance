using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;
        public UserProfileController(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles ="Customer")]
        [Route("GetUserProfile")]
        public async Task<Object> GetUserProfile()
        {
            try
            {
                var userdata = new UserDetailsModel();

                string userId = User.Claims.First(c => c.Type == "UserId").Value;
                var user = await _userManager.FindByIdAsync(userId);
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();


                return Ok(new
                {
                    username = user.UserName,
                    email = user.Email,
                    fullName = user.FullName,
                    userId = user.Id,
                    joiningFees = userdata.joiningFees
                });
            }
            catch (Exception error)
            {
                return BadRequest(new { message="Unable to get the error", error = error });
            }
       
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        [Route("GetUserData/")]
        public async Task<Object> GetUserData()
        {
            try
            {
                var userdata = new UserDetailsModel();
                var bankData = new BankDetailsModel();
                var orderHistory = new List<OrdersModel>();
                var TransactionHistory = new List<TransactionsModel>();
                var EmiData = new List<EMImodels>();
                var ProductData = new List<ProductModel>();

                string userId = User.Claims.First(c => c.Type == "UserId").Value;
                var user = await _userManager.FindByIdAsync(userId);

                bankData = await _context.BankDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();
                orderHistory = await _context.Orders.Where(item => item.userId == userId).ToListAsync();
                TransactionHistory = await _context.Transactions.Where(item => item.userId == userId).ToListAsync();
                EmiData = await _context.EMI.Where(item => item.userId == userId).ToListAsync();
                ProductData = await _context.Products.ToListAsync();

                var data = from item in orderHistory
                           join item2 in EmiData on item.orderId equals item2.orderId
                           join p2 in ProductData on item.productId equals p2.productId
                           select new
                           {
                               orderId = item.orderId,
                               userid = item.userId,
                               emiInitialDate = item2.emiInitialDate,
                               emiAmount = item2.emiAmount,
                               amountPaid = item2.amountPaid,
                               totalAmount = item2.totalAmoubt,
                               EmiCreatedAt = item2.createdAt,
                               orerCreatedAt = item.createdAt,
                               remainingBalance = item2.remainingBalance,
                               isEmiCompleted = item2.isEmiCompleted,
                               emiPeriod = item2.emiPeriod,
                               emiNextDate = item2.emiNextDate,
                               productId = item2.productId,
                               productName = item.ProductName,
                               transactionId = item.transactionId,
                               PendingEmiInstallment = item2.PendingEmiInstallment,
                               productImg = p2.Img,
                           };

                var cardDetails = new
                {
                    cardNumber = bankData.cardNumber,
                    cardStatus = bankData.cardStatus,
                    isActivated = userdata.isActivated,
                    bankName = bankData.bankname,
                    cardType = bankData.CardType,
                    remainingAmount = bankData.RemainingBalance,
                    totalCredits = bankData.totalCredit,
                    initialCredits = bankData.InitialCredits,
                    amountSpent = bankData.amountSpent,
                    validity = bankData.Validity,
                    savingsAccountNumber = bankData.accountNumber,
                    fullname = user.FullName
                };

                return Ok(new
                {
                    userCardDetails = cardDetails,
                    orderHistory = data,
                    transactionHistory = TransactionHistory,
                });
            }
            catch (Exception error)
            {

                return BadRequest(new { message = error });

            }
            
        }

        [HttpPut]
        [Authorize(Roles = "Customer")]
        [Route("PostJoiningFees/{id}")]
        public async Task<Object> PostJoiningFees(string id, JoiningFeesModel data)
        {
            try
            {
                if (id == null || id == "")
                {
                    return BadRequest(new { message = "Id is null" });
                }
                else
                {
                    var userData = await _context.UserDetails.Where(res => res.userId == id).FirstOrDefaultAsync();
                    if (userData != null)
                    {
                        userData.joiningFees = true;
                        userData.joiningFeesAmount = data.joiningFeesAmount;
                        
                        _context.Update(userData);
                        await _context.SaveChangesAsync();
                       
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(new { message = "Can't find User with the given id" });
                    }

                }
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Something went wrong" });
            }
        }

        
    }
}
