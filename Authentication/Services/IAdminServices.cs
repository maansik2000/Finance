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

        public async Task<ApiResponse> ActivateAccount(string id, BankDetailsModel data)
        {
            if (id == null || id == "")
            {
                return new ApiResponse
                {
                    success = false,
                    message = "Id is null"
                };
            }
            else
            {
                var userData = await _context.UserDetails.Where(res => res.userId == id).FirstOrDefaultAsync();
                if (userData != null)
                {
                    userData.isActivated = true;
                    userData.isVerified = true;
                    _context.Update(userData);
                    await _context.SaveChangesAsync();
                    _context.Entry(data).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                    return new ApiResponse
                    {
                        success = true,
                        message = "User Account is activated"
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        success = false,
                        message = "Can't find user with given id"
                    };
                }

            }
        }

        public async Task<ApiResponse> AdminSignUp(AdminModel data)
        {
            data.roles = "Admin";
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
                var result = await _userManager.CreateAsync(applicationUser, data.password);
                await _userManager.AddToRoleAsync(applicationUser, data.roles);

                if (result.Succeeded)
                {
                    return new ApiResponse
                    {
                        success = true,
                        res = result
                    };
                }
                else
                {
    
                    return new ApiResponse
                    {
                        message = "Unable to create user or user exist",
                        success = false
                    };
                }
            }
            else
            {
                return new ApiResponse
                {
                    message = "User already exist, please login",
                    success = false
                };
           
            }
        }

        public async Task<ApiResponse> DeleteUser(string id)
        {
            var userFind = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Id == id && a.isActivated);

            if(userFind != null)
            {
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
                return new ApiResponse
                {
                    message = "Can't delete as User doesn't exist",
                    success = false
                };
            }
        }

        public async Task<ApiResponse> GetAdminProfile(string userId)
        {
            
            var user = await _userManager.FindByIdAsync(userId);

            if(user != null)
            {
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
                return new ApiResponse
                {
                    success = false,
                    message = "Unable to get the admin Profile"
                };
            }
        }

        public async Task<ApiResponse> GetAllUsers()
        {
            var user = new List<ApplicationUser>();
            var adminList = new List<ApplicationUser>();

            var userData = new List<UserDetailsModel>();

            user = await _context.ApplicationUsers.ToListAsync();
            userData = await _context.UserDetails.ToListAsync();

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

            //for filtering admin and user , need to change this
            foreach (var item in user)
            {
                if (item.Role == "Admin")
                {
                    adminList.Add(item);
                }
            }

            //for filtering deactivated accounts
            foreach (var item in userData)
            {
                if (item.isActivated == false)
                {
                    deactivatedAccount++;
                }
            }


            if (user != null)
            {
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

        public async Task<ApiResponse> GetUserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
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

                if (userdata != null && bankdata != null && user != null)
                {
                    var allData = new AdminAllUsersModel()
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
