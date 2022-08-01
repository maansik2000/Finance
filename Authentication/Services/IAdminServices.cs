using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.Services
{
    public interface IAdminServices
    {
        //iadminservice interface is implemented by adminService class in that all the methods are define.
        //using asynchronous programming, the application can work on other task without waiting for the task to be completed
        //async keyword is used to make the method asynchronous, If any Second Method, as method2 has a dependency on method1,
        //then it will wait for the completion of Method1 with the help of await keyword.
        //The async keyword marks the method as asynchronous.
        //The await keyword waits for the async method to complete until it returns a value.
        Task<ApiResponse> AdminSignUp(AdminModel data);
        Task<ApiResponse> GetAllUsers();
        Task<ApiResponse> GetUserDetails(string id);
        Task<ApiResponse> GetAdminProfile(string userId);
        Task<ApiResponse> ActivateAccount(string id, BankDetailsModel data);
        Task<ApiResponse> DeleteUser(string id);
    }

    public class AdminService : IAdminServices
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;
        public AdminService(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        //method for activating the users account
        public async Task<ApiResponse> ActivateAccount(string id, BankDetailsModel data)
        {
            if (id == null || id == "") //if the id is null or empty then api response will be false and message is sent to the client
            {
                return new ApiResponse
                {
                    success = false,
                    message = "Id is null"
                };
            }
            else
            {
                var user = await _context.ApplicationUsers.Where(item => item.isActivated && item.Id == id).FirstOrDefaultAsync();
                var userData = await _context.UserDetails.Where(res => res.userId == id).FirstOrDefaultAsync(); //wait until we get the return value
                if (userData != null && user != null) //if the user we find is null then, we send the api response as false and error message otherwise we will activate the data of the user in the database
                {
                    userData.isActivated = true; //updating only isActivated and isVerified in the database
                    userData.isVerified = true;
                    _context.Update(userData);
                    await _context.SaveChangesAsync();
                    _context.Entry(data).State = EntityState.Modified;  //this is updating the whole record of the bank details in the database if it is successfull then we will send the api response as true
                    await _context.SaveChangesAsync();
                    return new ApiResponse
                    {
                        success = true,
                        message = "User Account is activated"
                    };
                }
                else
                {
                    //if the user is null then error response is send
                    return new ApiResponse
                    {
                        success = false,
                        message = "Can't find user with given id"
                    };
                }

            }
        }

        //method for admin Signup
        public async Task<ApiResponse> AdminSignUp(AdminModel data)
        {
            data.roles = "Admin"; //assigning roles to the admin
            var applicationUser = new ApplicationUser()
            {
                UserName = data.username,
                Email = data.email,
                FullName = data.fullName,
                Role = "Admin",
                isActivated = true
            };

            var userFind = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email || a.UserName == data.username); 

            if (userFind == null)
            {
                var result = await _userManager.CreateAsync(applicationUser, data.password); //if we didn't find the user in the database then we will create a database using CreateAsync Function
                await _userManager.AddToRoleAsync(applicationUser, data.roles); //here we are adding the roles in the database using AddToRoleAsync function from identity

                if (result.Succeeded)
                { 
                    //if the result is succeeded then we will send the api response as true
                    return new ApiResponse
                    {
                        success = true,
                        res = result
                    };
                }
                else
                {
                    //if the result is false then we will send the api response as false
                    return new ApiResponse
                    {
                        message = "Unable to create user or user exist",
                        success = false
                    };
                }
            }
            else
            {
                //if the user already present in the database we will send the message and api response as false
                return new ApiResponse
                {
                    message = "User already exist, please login",
                    success = false
                };
           
            }
        }

        //method for deleting the user account
        public async Task<ApiResponse> DeleteUser(string id)
        {
            var userFind = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Id == id && a.isActivated); //we will find is the user is present in the database and the user must be activated

            if(userFind != null)
            {
                //if the user is present in the database then we will set the isActivated flag to false and update the database and then send the apiResponse to true
                userFind.isActivated = false;
                _context.Update(userFind);
                await _context.SaveChangesAsync();

                return new ApiResponse
                {
                    message = "User is Deleted Successfully",
                    success = true
                };
            }
            else
            {
                //if the user already present in the database then we will send the api response as false and message
                return new ApiResponse
                {
                    message = "Can't delete as User doesn't exist",
                    success = false
                };
            }
        }

        //method for getting admin profile
        public async Task<ApiResponse> GetAdminProfile(string userId)
        {
            
            var user = await _userManager.FindByIdAsync(userId); //finding admin using _usermanager from identity through id

            if(user != null)
            {
                //if the user if present then we will send the api response to true and send the admin details
                var res = new {
                    fullname = user.FullName,
                    email = user.Email,
                    username = user.UserName
                };

                return new ApiResponse
                {
                    success = true,
                    res = res
                };
            }
            else
            {
                //if the user doesn't exist then we will send the error response
                return new ApiResponse
                {
                    success = false,
                    message = "Unable to get the admin Profile"
                };
            }
        }

        //method for getting all users profile
        public async Task<ApiResponse> GetAllUsers()
        {
            var user = new List<ApplicationUser>();
            var adminList = new List<ApplicationUser>();

            var userData = new List<UserDetailsModel>();

            user = await _context.ApplicationUsers.ToListAsync();
            if (user != null)
            {
                userData = await _context.UserDetails.ToListAsync();

                //first getting all the user and their details from the database using ToListAsync then joining both table on the basis of ID and then extracting whatever data we need in userList
                var userList = from u in user
                               join ud in userData on u.Id equals ud.userId
                               select new
                               {
                                   userid = u.Id,
                                   username = u.UserName,
                                   email = u.Email,
                                   phoneNumber = ud.phoneNumber,
                                   isActivated = ud.isActivated,
                                   fullName = u.FullName,
                                   isVerified = ud.isVerified,
                                   createdAt = ud.createdAt,
                                   isActivatedUser = u.isActivated
                               };

                int totalUser = user.Count();
                int deactivatedAccount = 0;

                //for filtering admin and user
                foreach (var item in user)
                {
                    if (item.Role == "Admin")
                    {
                        adminList.Add(item);
                    }
                }

                //for filtering deactivated accounts if the isActivated flag is false
                foreach (var item in userData)
                {
                    if (item.isActivated == false)
                    {
                        deactivatedAccount++;
                    }
                }

                //in the final result we will send adminData, userData, total user, adminUsers, userCount
                var res = new
                {
                    adminList = adminList,
                    userList = userList,
                    totalUsers = totalUser,
                    adminUser = adminList.Count(),
                    userCount = userList.Count(),
                    deactivatedAccount = deactivatedAccount
                };
                return new ApiResponse
                {
                    success = true,
                    res= res
                };
            }
            else
            {
                return new ApiResponse
                {
                    message = "Unabel to get all users",
                    success = false
                };
            }
        }

        //method for getting user details for the given user Id
        public async Task<ApiResponse> GetUserDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) //if the user is null or empty then we will send the error response and api respose to false
            {
                return new ApiResponse
                {
                    message = "Id is Null",
                    success = false
                };
            }
            else
            {
                var userdata = new UserDetailsModel();
                var bankdata = new BankDetailsModel();
                var user = new ApplicationUser();

                userdata = await _context.UserDetails.Where(s => s.userId == id).FirstOrDefaultAsync();
                bankdata = await _context.BankDetails.Where(s => s.userId == id).FirstOrDefaultAsync();
                user = await _context.ApplicationUsers.Where(s => s.Id == id).FirstOrDefaultAsync();

                //getting all the userData, bankData and user info by the Id from the database using FirstorDefaultAsync

                if (userdata != null && bankdata != null && user != null)
                {
                    var allData = new AdminAllUsersModel()   //if we find the data in the database then we will send the result as true and response
                    {
                        userid = id,
                        bankId = bankdata.bankId,
                        username = user.UserName,
                        email = user.Email,
                        phoneNumber = userdata.phoneNumber,
                        joiningFees = userdata.joiningFees,
                        UserAddress = userdata.UserAddress,
                        bankname = bankdata.bankname,
                        branch = bankdata.branch,
                        ifscCode = bankdata.ifscCode,
                        isActivated = userdata.isActivated,
                        accountNumber = bankdata.accountNumber,
                        CardType = bankdata.CardType,
                        dateOfBirth = userdata.dateOfBirth,
                        fullName = user.FullName,
                        totalCredit = bankdata.totalCredit,
                        Validity = bankdata.Validity,
                        cardStatus = bankdata.cardStatus,
                        cardNumber = bankdata.cardNumber,
                        amountSpent = bankdata.amountSpent,
                        isVerified = userdata.isVerified,
                        createdAt = userdata.createdAt,
                        RemainingBalance = bankdata.RemainingBalance,
                        role = user.Role,
                        InitialCredits = bankdata.InitialCredits,
                        isActivateUser =user.isActivated
                    };

                    return new ApiResponse
                    {
                        res = allData,
                        success = true
                    };
                }
                else
                {
                    //if the user with the given id doesn;t present in the database then we will send error response
                    return new ApiResponse
                    {
                        message = "User Doesn't exist for the given id",
                        success = false
                    };
                }
            }
        }

    }
}
