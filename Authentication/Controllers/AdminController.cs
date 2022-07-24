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
    public class AdminController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;
        public AdminController(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        [Route("Signup")]
        public async Task<Object> AdminSignUp(AdminModel data)
        {
            data.roles = "Admin";
            var applicationUser = new ApplicationUser()
            {
                UserName = data.username,
                Email = data.email,
                FullName = data.fullName,

            };

            try
            {
                var userFind = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email || a.UserName == data.username);

                if (userFind == null)
                {
                    var result = await _userManager.CreateAsync(applicationUser, data.password);
                    await _userManager.AddToRoleAsync(applicationUser, data.roles);

                    if (result.Succeeded)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest(new { message = "Unable to create user or user exist" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "User already exist, please login" });
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        [Route("getAllUsers")]
        public async Task<ActionResult<IEnumerable<UserDetailsModel>>> GetAllUsers()
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
                       };

            int totalUser = user.Count();
            int deactivatedAccount = 0;
            
            //for filtering admin and user 
            foreach (var item in user)
            {
                if(item.UserName == "admin")
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
                return Ok(new { adminList = adminList, userList = userList , totalUsers = totalUser, adminUser = adminList.Count(), userCount = userList.Count(), deactivatedAccount = deactivatedAccount });
            }
            else
            {
                return Ok(new { data = "unable to get the data" });
            }
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        [Route("GetUserDetails/{id}")]
        public async Task<Object> getUserDetails(string id)
        {
            try
            {
                if(id == null)
                {
                    return BadRequest(new { message="Id is null" });
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
                            role = userdata.role
                        };

                        

                        return Ok(new { data = allData });
                    }
                    else {
                        return BadRequest(new { message = "unable to find the user" });
                    }
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
            
        }

        [HttpGet]
        [Route("userProfile")]
        [Authorize(Roles = "Admin")]
        public async Task<Object> GetAdminProfile()
        {
            string userId = User.Claims.First(c => c.Type == "UserId").Value;
            var user = await _userManager.FindByIdAsync(userId);

            return new
            {
                fullname = user.FullName,
                email = user.Email,
                username = user.UserName
            };
        }
    }
}
