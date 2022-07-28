using Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Services
{
    public interface IUserProfileService 
    {
        Task<ApiResponse> GetUserProfile(string userId);
        Task<ApiResponse> GetUserData(string userId);
        Task<ApiResponse> PostJoiningFees(string id, JoiningFeesModel data);
        Task<ApiResponse> BuyProduct(BuyOrderModel data);

        Task<ApiResponse> PayEmiInstallment(EmiPaymentModel data);
    }

    public class UserProfileService : IUserProfileService {
        private UserManager<ApplicationUser> _userManager;
        private readonly AuthenticationContext _context;

        public UserProfileService(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<ApiResponse> GetUserProfile(string userId)
        {
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();

            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();
                bankData = await _context.BankDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();

                var data = new
                {
                    username = user.UserName,
                    email = user.Email,
                    fullName = user.FullName,
                    userId = user.Id,
                    joiningFees = userdata.joiningFees,
                    isCardActivated = bankData.cardStatus
                };

                return new ApiResponse
                {
                    res = data,
                    success = true
                };
            }
            else
            {
                return new ApiResponse
                {
                    message = "User is not available for the userid",
                    success = false
                };
            }
            
        }

        public async Task<ApiResponse> GetUserData(string userId)
        {
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();
            var orderHistory = new List<OrdersModel>();
            var TransactionHistory = new List<TransactionsModel>();
            var EmiData = new List<EMImodels>();
            var ProductData = new List<ProductModel>();


            var user = await _userManager.FindByIdAsync(userId);

            if(user != null)
            {

                bankData = await _context.BankDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();

                if(bankData != null && userdata != null)
                {
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
                                   emiId = item2.EmiId
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
                    var res = new
                    {
                        userCardDetails = cardDetails,
                        orderHistory = data,
                        transactionHistory = TransactionHistory,
                    };

                    return new ApiResponse
                    {
                        res = res,
                        success = true
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        message = "Bank details and user details doesn't exist, please contact admin",
                        success = false
                    };
                }
                
            }
            else
            {
                return new ApiResponse
                {
                    message = "User data is not available for the userid",
                    success = false
                };
            }

        }

        public async Task<ApiResponse> PostJoiningFees(string id, JoiningFeesModel data)
        {
            if (id == null || id == "")
            {
                return new ApiResponse
                {
                    message = "Id is null",
                    success = false
                };
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

                    return new ApiResponse
                    {
                        res = "Joining Fees Submitted",
                        success = true
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        message = "Can't find user with the given Id",
                        success = false
                    };
                }

            }
        }

        public async Task<ApiResponse> BuyProduct(BuyOrderModel data)
        {
            var userId = data.userId;
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();
            var productItem = new ProductModel();
            var newEmi = new EMImodels();

            var user = await _userManager.FindByIdAsync(userId);

            if(user != null)
            {
                DateTime now = DateTime.Now;
                bankData = await _context.BankDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();
                productItem = await _context.Products.FirstOrDefaultAsync(item => item.productId == data.productId);

                if (bankData != null && userdata != null && bankData.cardStatus == "Activated")
                {
                    if(bankData.RemainingBalance > productItem.totalCost && data.emiPeriod != 0 )
                    {
                        var newTransaction = new TransactionsModel
                        {
                            userId = userId,
                            TansactionStatus = "Completed",
                            amountPaid = productItem.totalCost,
                            TransactionDate = DateTime.Now,
                            ProductName = productItem.ProductName,
                            productId = productItem.productId
                        };

                        await _context.Transactions.AddAsync(newTransaction);
                        var response = await _context.SaveChangesAsync();

                        if(response > 0)
                        {
                            var transactionId = newTransaction.transactionId;
                           
                            var newOrder = new OrdersModel
                            {
                                userId = userId,
                                transactionId = transactionId,
                                TransactionDate = newTransaction.TransactionDate,
                                ProductName = productItem.ProductName,
                                productId = productItem.productId,
                                totalPrice = productItem.totalCost,
                                createdAt = DateTime.Now
                            };

                            await _context.Orders.AddAsync(newOrder);
                            await _context.SaveChangesAsync();
                            var orderId = newOrder.orderId;

                            newEmi = new EMImodels
                            {
                                userId = userId,
                                orderId = orderId,
                                emiInitialDate = DateTime.Now.AddMonths(1),
                                totalAmoubt = productItem.totalCost,
                                emiAmount = productItem.totalCost / data.emiPeriod,
                                amountPaid = 0.0000m,
                                productId = productItem.productId,
                                createdAt = DateTime.Now,
                                remainingBalance = productItem.totalCost,
                                isEmiCompleted = false,
                                emiPeriod = data.emiPeriod,
                                emiNextDate = DateTime.Now.AddMonths(1),
                                PendingEmiInstallment = data.emiPeriod
                            };

                            await _context.EMI.AddAsync(newEmi);
                            await _context.SaveChangesAsync();


                            bankData.RemainingBalance = bankData.RemainingBalance - productItem.totalCost;
                            bankData.amountSpent = bankData.amountSpent + productItem.totalCost;
                            _context.Update(bankData);
                            await _context.SaveChangesAsync();

                            return new ApiResponse
                            {
                                message = "Order Placed Successfully",
                                success = true
                            };
                        }
                        else
                        {
                            return new ApiResponse
                            {
                                message = "Transaction Failed",
                                success = false
                            };
                        }
                    }
                    else
                    {
                        return new ApiResponse
                        {
                            message = "Your Credits are low, you can't place order",
                            success = false
                        };
                    }
                    
                }
                else
                {
                    return new ApiResponse
                    {
                        message = "Can't place Order",
                        success = false
                    };
                }
            }
            else
            {
                return new ApiResponse
                {
                    message = "Can't find user with the given Id",
                    success = false
                };
            }
        }

        public async Task<ApiResponse> PayEmiInstallment(EmiPaymentModel data)
        {
            var userId = data.userId;
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();
            var productItem = new ProductModel();
            var emiData = new EMImodels();
            var orderData = new OrdersModel();
            var transactionData = new TransactionsModel();

            var user = await _userManager.FindByIdAsync(userId);

            if(user != null)
            {
                transactionData = await _context.Transactions.Where(s => s.transactionId == data.TransactionId).FirstOrDefaultAsync();

                if(transactionData.TansactionStatus == "Completed")
                {
                    orderData = await _context.Orders.Where(s => s.orderId == data.orderId && s.userId == data.userId && s.transactionId == transactionData.transactionId).FirstOrDefaultAsync();
                 
                    if(orderData != null)
                    {
                        emiData = await _context.EMI.Where(s => s.orderId == data.orderId && s.EmiId == data.EmiId).FirstOrDefaultAsync();

                        if (!emiData.isEmiCompleted && emiData.emiNextDate.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
                        {
                            if (emiData.PendingEmiInstallment != 1)
                            {
                                emiData.amountPaid = emiData.amountPaid + emiData.emiAmount;
                                emiData.remainingBalance = emiData.remainingBalance - emiData.emiAmount;
                                emiData.isEmiCompleted = false;
                                emiData.emiNextDate = DateTime.Now.AddMonths(1);
                                emiData.PendingEmiInstallment--;
                            }
                            else
                            {
                                emiData.amountPaid = emiData.totalAmoubt;
                                emiData.remainingBalance = 0;
                                emiData.isEmiCompleted = true;
                                emiData.PendingEmiInstallment = 0;
                            }

                            _context.Update(emiData);
                            await _context.SaveChangesAsync();

                            return new ApiResponse
                            {
                                message = "Emi Installment Paid Successfully",
                                success = true
                            };
                        }
                        else
                        {
                            return new ApiResponse
                            {
                                message = "EMI Already Completed",
                                success = false
                            };
                        }
                    }
                    else
                    {
                        return new ApiResponse
                        {
                            message = "Order doesn't Exist for the given Order Id",
                            success = false
                        };
                    }
                }
                else
                {
                    return new ApiResponse
                    {
                        message = "Can't pay emi transation was not successfull",
                        success = false
                    };
                }
                
            }
            else
            {
                return new ApiResponse {
                    message = "Can't pay emi as user is not present",
                    success = false
                };

            }
        }
    }

}
