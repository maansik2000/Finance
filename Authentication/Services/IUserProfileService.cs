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
        //IUserProfileService interface is implemented by UserProfileService class in that all the methods are define.
        //using asynchronous programming, the application can work on other task without waiting for the task to be completed
        //async keyword is used to make the method asynchronous, If any Second Method, as method2 has a dependency on method1,
        //then it will wait for the completion of Method1 with the help of await keyword.
        //The async keyword marks the method as asynchronous.
        //The await keyword waits for the async method to complete until it returns a value.

        private UserManager<ApplicationUser> _userManager;
        private readonly AuthenticationContext _context;

        public UserProfileService(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        //method for getting user profile like username, email, cardStatus and fullName for the given userid
        public async Task<ApiResponse> GetUserProfile(string userId)
        {
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();

            var user = await _userManager.FindByIdAsync(userId);        //finding user using _usermanager from identity by the given id in the database
            if (user != null && userdata != null && bankData != null)
            {
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();     //finding userData with the userId from the frontend
                bankData = await _context.BankDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();     //finding bankData with the userId from the frontend

                if(userdata != null && bankData != null)                //if the userData and bank data exist then sending api response as true and sending data
                {
                    var data = new
                    {
                        username = user.UserName,
                        email = user.Email,
                        fullName = user.FullName,
                        userId = user.Id,
                        joiningFees = userdata.joiningFees,
                        isCardActivated = bankData.cardStatus,
                        cardType = bankData.CardType
                    };

                    return new ApiResponse
                    {
                        res = data,
                        success = true
                    };
                }
                else
                {
                    //if the userData and bank data doesn't exist then sending error response
                    return new ApiResponse
                    {
                        message = "Bank Details doesn't exist for the user, please contact admin",
                        success = false
                    };
                }
            }
            else
            {
                //if the user dones't present in the database then sending error response
                return new ApiResponse
                {
                    message = "User doesn't exist for the given userId",
                    success = false
                };
            }
            
        }

        //method for getting all the data like transacton data, order history, emi pending etc for the given userId
        public async Task<ApiResponse> GetUserData(string userId)
        {
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();
            var orderHistory = new List<OrdersModel>();
            var TransactionHistory = new List<TransactionsModel>();
            var EmiData = new List<EMImodels>();
            var ProductData = new List<ProductModel>();

            var user = await _userManager.FindByIdAsync(userId);                            //finding the user using _userManager and FindByidAsync method for the given userId in the database

            if (user != null)
            {
                bankData = await _context.BankDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();         //if the user is present in the database then finding bank data and userData for the given userId
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();         //if the user is present in the database then finding user data and userData for the given userId

                if (bankData != null && userdata != null)
                {
                    orderHistory = await _context.Orders.Where(item => item.userId == userId).ToListAsync();        //if the user Data and bank data is present in the database then finding orderhistory, transaction History and emi pending list for the given userId
                    TransactionHistory = await _context.Transactions.Where(item => item.userId == userId).ToListAsync();
                    EmiData = await _context.EMI.Where(item => item.userId == userId).ToListAsync();
                    ProductData = await _context.Products.ToListAsync();

                    var data = from item in orderHistory
                               join item2 in EmiData on item.orderId equals item2.orderId
                               join p2 in ProductData on item.productId equals p2.productId
                               select new                                                                          //joining order history, emi data and product to get the complete info and storing in data
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

                    //getting card details and bank detaiils from bankData
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

                    //storing orderData, cardDetails and transactionHistory in the final res and then sending api response as true 
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
                    //if the bank details and userDetails doesn't exist then sending error message
                    return new ApiResponse
                    {
                        message = "Bank details and user details doesn't exist, please contact admin",
                        success = false
                    };
                }
                
            }
            else
            {
                //if the user doesn't present in the database
                return new ApiResponse
                {
                    message = "User data is not available for the userid",
                    success = false
                };
            }

        }

        //method for submitting joining fees
        public async Task<ApiResponse> PostJoiningFees(string id, JoiningFeesModel data)
        {
            if (id == null || id == "")         //if the id is null then sending error response
            {
                return new ApiResponse
                {
                    message = "Id is null",
                    success = false
                };
            }
            else
            {
                var userData = await _context.UserDetails.Where(res => res.userId == id).FirstOrDefaultAsync();   //finding userData for the given userId
                if (userData != null)
                {
                    userData.joiningFees = true;                            //setting joining fees to true
                    userData.joiningFeesAmount = data.joiningFeesAmount;    //setting joiningFeesAmount to the given amount from the frontend

                    _context.Update(userData);                              //updating the database
                    await _context.SaveChangesAsync();

                    return new ApiResponse                                  //sending true response
                    {
                        res = "Joining Fees Submitted",
                        success = true
                    };
                }
                else
                {
                    return new ApiResponse                                  //sending false response if the user isn't availabe in the database
                    {
                        message = "Can't find user with the given Id",
                        success = false
                    };
                }

            }
        }

        //method for placing an order
        public async Task<ApiResponse> BuyProduct(BuyOrderModel data)
        {
            var userId = data.userId;                                   //extracting user Id from the data
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();
            var productItem = new ProductModel();
            var newEmi = new EMImodels();

            var user = await _userManager.FindByIdAsync(userId);        //finding user from given user Id using _userManager

            if(user != null)
            {
                bankData = await _context.BankDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();         //finding bankDetails from given user Id if the user is not null
                userdata = await _context.UserDetails.Where(s => s.userId == userId).FirstOrDefaultAsync();         //finding userData from given user Id if the user is not null
                productItem = await _context.Products.FirstOrDefaultAsync(item => item.productId == data.productId);    //finding product from given user Id if the user is not null

                if (bankData != null && userdata != null && bankData.cardStatus == "Activated")                     //checking if the bank details and user details are null and card status must be active
                {
                    //checking if the remaining balance is greater than total cost of the product and emi period from the frontend must be not equals to 0
                    if (bankData.RemainingBalance > productItem.totalCost && data.emiPeriod != 0 )                  
                    {
                        var newTransaction = new TransactionsModel                  //making new transaction using userId
                        {
                            userId = userId,
                            TansactionStatus = "Completed",
                            amountPaid = productItem.totalCost,
                            TransactionDate = DateTime.Now,
                            ProductName = productItem.ProductName,
                            productId = productItem.productId
                        };

                        await _context.Transactions.AddAsync(newTransaction);       //adding new transaction
                        var response = await _context.SaveChangesAsync();           //saving the database

                        if(response > 0)                                            //check if the transaction insertion is true
                        {
                            var transactionId = newTransaction.transactionId;       //taking the transactionId from the database
                           
                            var newOrder = new OrdersModel                          //making new order using the new transaction Id and userId
                            {
                                userId = userId,
                                transactionId = transactionId,
                                TransactionDate = newTransaction.TransactionDate,
                                ProductName = productItem.ProductName,
                                productId = productItem.productId,
                                totalPrice = productItem.totalCost,
                                createdAt = DateTime.Now
                            };

                            await _context.Orders.AddAsync(newOrder);               //adding new order in database
                            await _context.SaveChangesAsync();                          
                            var orderId = newOrder.orderId;                         //taking the orderId from the newest created record

                            //creating new EMI using orderid and userId and setting emi next and initital date to next month,
                            //emi pending installment to full emi, 
                            //amount paid to 0 as initially,
                            //isEmi completed to false
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

                            await _context.EMI.AddAsync(newEmi);                    //adding new EMI
                            await _context.SaveChangesAsync();


                            bankData.RemainingBalance = bankData.RemainingBalance - productItem.totalCost;  //deducting the total amount of the product from the remaining balace
                            bankData.amountSpent = bankData.amountSpent + productItem.totalCost;            //addming the total amount of the product to the amount spent balace
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
                            //if the transaction is failed then sending false response
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
                            //if the credits are low then sending error message
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
                //if the user is not present in the database sending error message
                return new ApiResponse
                {
                    message = "Can't find user with the given Id",
                    success = false
                };
            }
        }

        //method for paying emi installments
        public async Task<ApiResponse> PayEmiInstallment(EmiPaymentModel data)
        {
            var userId = data.userId;
            var userdata = new UserDetailsModel();
            var bankData = new BankDetailsModel();
            var productItem = new ProductModel();
            var emiData = new EMImodels();
            var orderData = new OrdersModel();
            var transactionData = new TransactionsModel();

            var user = await _userManager.FindByIdAsync(userId);            //finding user from given user Id using _userManager

            if (user != null)
            {
                transactionData = await _context.Transactions.Where(s => s.transactionId == data.TransactionId).FirstOrDefaultAsync();          //finding the transaction using given transaction id from the frontend 

                if(transactionData.TansactionStatus == "Completed")        //checking if the transaction status was completed or not
                {
                    orderData = await _context.Orders.Where(s => s.orderId == data.orderId && s.userId == data.userId && s.transactionId == transactionData.transactionId).FirstOrDefaultAsync();       //finding order data from the database using given userId, transactionId, and orderId

                    if (orderData != null)                                  //if the order is not null then paying the emi installment
                    {
                        emiData = await _context.EMI.Where(s => s.orderId == data.orderId && s.EmiId == data.EmiId).FirstOrDefaultAsync();      //now finding the emi for the current order and current emi Id from the frontend

                        if (!emiData.isEmiCompleted && emiData.emiNextDate.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))       //checking if the emi is completed and current date must be equals to emi next date
                        {
                            if (emiData.PendingEmiInstallment >= 1)                                
                            {
                                //if the emi pending installment is greater than 1 
                                //adding emi amount to the amount paid,
                                //deducting emi amount to the remaining balance
                                emiData.amountPaid = emiData.amountPaid + emiData.emiAmount;
                                emiData.remainingBalance = emiData.remainingBalance - emiData.emiAmount;
                                emiData.isEmiCompleted = false;
                                emiData.emiNextDate = DateTime.Now.AddMonths(1);
                                emiData.PendingEmiInstallment--;
                            }
                            else
                            {
                                //if the emi pending installment is equals to 1
                                //setting amount paid to total amount
                                //setting remaining balance to 0
                                //setting isEmiCompleted to true
                                //setting PendingEmiInstallment to 0
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
                            //if the emi is already completed then sending emi response error message
                            return new ApiResponse
                            {
                                message = "EMI Already Completed",
                                success = false
                            };
                        }
                    }
                    else
                    {
                        //if the order doesn't exist then sending error response
                        return new ApiResponse
                        {
                            message = "Order doesn't Exist for the given Order Id",
                            success = false
                        };
                    }
                }
                else
                {
                    //if the transaction was not successfull then sending error response
                    return new ApiResponse
                    {
                        message = "Can't pay emi transation was not successfull",
                        success = false
                    };
                }
                
            }
            else
            {
                //if the user is not present in database then sending error response
                return new ApiResponse {
                    message = "Can't pay emi as user is not present",
                    success = false
                };

            }
        }
    }

}
