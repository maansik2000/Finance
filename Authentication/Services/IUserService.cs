using Authentication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace Authentication.Services
{
    public interface IUserService
    {
        Task<ApiResponse> Register(ApplicationUserModel data);
        Task<ApiResponse> Login(LoginModel data);
        Task<ApiResponse> ForgetPassword(string email);
        Task<ApiResponse> ResetPassword(ResetPasswordModel data);
        Task<ApiResponse> ResetPasswordToken(string email);
    }

    public class UserService : IUserService
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly AuthenticationContext _context;
        private readonly ApplicationSettings _applicationSettings;
        private IConfiguration _configuration;
        private readonly IMailService _emailService;

        public UserService(IMailService emailService, IConfiguration configuration,UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager, AuthenticationContext context, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signinManager;
            _context = context;
            _applicationSettings = appSettings.Value;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<ApiResponse> Login(LoginModel data)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email  && a.isActivated);
            //var user = await _userManager.FindByEmailAsync(data.email);
           
            if (user != null && await _userManager.CheckPasswordAsync(user, data.password))
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

                //this is jwt token
                var token = tokenHandler.WriteToken(securityToken);

                var res = new { token = token, email = user.Email, username = user.UserName, fullName = user.FullName, isActivated= user.isActivated };

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
                    message = "Email or Password is incorrect",
                    success = false
                };
            }
        }

        public async Task<ApiResponse> Register( ApplicationUserModel data)
        {
            data.roles = "Customer";
            var applicationUser = new ApplicationUser()
            {
                UserName = data.username,
                Email = data.email,
                FullName = data.fullName,
                isActivated = true
            };

            var userFind = await _context.ApplicationUsers.FirstOrDefaultAsync(a => (a.Email == data.email || a.UserName == data.username) && a.isActivated);

            if (userFind == null)
            {
                var result = await _userManager.CreateAsync(applicationUser, data.password);
                await _userManager.AddToRoleAsync(applicationUser, data.roles);

                if (result.Succeeded)
                {
                    var userData = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email && a.UserName == data.username && a.isActivated);

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
                        return new ApiResponse
                        {
                            message = "Successfull created the user",
                            res = result,
                            success = true
                        };
                    }
                    else
                    {
                        return new ApiResponse
                        {
                            message = "Somthing went wrong",
                            success = false
                        }; 
                    }
                }
                else
                {
                    return new ApiResponse
                    {
                        message = "Unable to create User or username exist",
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

        public async Task<ApiResponse> ForgetPassword(string email)
        {
            //var user = await _userManager.FindByEmailAsync(email);
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(a =>a.Email == email && a.isActivated);

            if (user == null)
            {
                return new ApiResponse
                {
                    success = false,
                    message = "No User found for the given mail",
                };
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);

                string url = $"{_applicationSettings.clientUrl}/user/reset-password?email={email}&token={validToken}";

                await _emailService.SendEmailAsync(email, "Reset Password", "<h1>Click this link to reset this password</h1>" +

                    $"<p>To reset your password <a href='{url}'>Click Here</a></p>"
                    );

                return new ApiResponse { success= true, res=validToken, message = "Reset password url has been sent to the email successfully" };
            }
        }

        public async Task<ApiResponse> ResetPassword(ResetPasswordModel data)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(a =>a.Email == data.email && a.isActivated);

            if (user == null)
            {
                return new ApiResponse
                {
                    success = false,
                    message = "No User found for the given mail",
                };
            }
            else
            {
                var decodedToken = WebEncoders.Base64UrlDecode(data.token);
                string normalToken = Encoding.UTF8.GetString(decodedToken);
                var result = await _userManager.ResetPasswordAsync(user, normalToken, data.password);

                if (result.Succeeded)
                {
                    return new ApiResponse
                    {
                        success = true,
                        message = "Your password has been reset, please login",
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        success = false,
                        message = "Invalid Token please try again",
                    };
                }
            }
        }

        public async Task<ApiResponse> ResetPasswordToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ApiResponse
                {
                    success = false,
                    message = "No User found for the given mail",
                };
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Encoding.UTF8.GetBytes(token);
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);
                var res = new { token = validToken };

                return new ApiResponse
                {
                    success = true,
                    res = res
                };
            }
        }
    }
}
