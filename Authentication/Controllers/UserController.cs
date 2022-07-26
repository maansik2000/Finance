using Authentication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly AuthenticationContext _context;
        private readonly ApplicationSettings _applicationSettings;

        public UserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager, AuthenticationContext context, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signinManager;
            _context = context;
            _applicationSettings = appSettings.Value;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<Object> PostApplicationUser(ApplicationUserModel data)
        {
            data.roles = "Customer";
            var applicationUser = new ApplicationUser()
            {
                UserName = data.username,
                Email = data.email,
                FullName = data.fullName,
            };

            try
            {
                var userFind = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email || a.UserName == data.username);

                if(userFind == null)
                {
                    var result = await _userManager.CreateAsync(applicationUser, data.password);
                    await _userManager.AddToRoleAsync(applicationUser, data.roles);

                    if (result.Succeeded)
                    {
                        var userData = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email);

                        if (userData != null)
                        {
                            DateTime localDate = DateTime.Now;
                            var newUser = new UserDetailsModel()
                            {
                                userId = userData.Id,
                                isActivated = false,
                                isVerified = false,
                                joiningFees = false,
                                dateOfBirth = data.dateOfBirth,
                                UserAddress = data.UserAddress,
                                phoneNumber = data.phoneNumber,
                                role = "Customer",
                                createdAt = localDate,
                                joiningFeesAmount = 0.0m

                            };

                            var bankDetails = new BankDetailsModel()
                            {
                                userId = userData.Id,
                                accountNumber = data.accountNumber,
                                bankname = data.bankname,
                                branch = data.branch,
                                CardType = data.CardType,
                                ifscCode = data.ifscCode,
                                cardNumber = "00000000",
                                cardStatus = "Deactivate",
                                Validity = "00/00",
                                RemainingBalance = 0.0m,
                                amountSpent = 0.0m,
                                totalCredit = 0.0m,
                                InitialCredits = 0.0m

                            };

                            await _context.UserDetails.AddAsync(newUser);
                            await _context.BankDetails.AddAsync(bankDetails);
                            await _context.SaveChangesAsync();
                            return Ok(result);
                        }
                        else
                        {
                            return BadRequest(new { message = "Something went wrong!!" });
                        }
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

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginModel data)
        {
            var user = await _userManager.FindByEmailAsync(data.email);
            if(user != null && await _userManager.CheckPasswordAsync(user, data.password))
            {
                //assign role
                var roles = await _userManager.GetRolesAsync(user);
                IdentityOptions _options = new IdentityOptions();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                        new Claim("UserId", user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles.FirstOrDefault())
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_applicationSettings.JWT_Secret_Code)), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new {token  = token, email = user.Email, username = user.UserName, fullName = user.FullName});
            }
            else
            {
                return BadRequest(new { message= "Username or password is incorrect"});
            }
        }
    }
}
