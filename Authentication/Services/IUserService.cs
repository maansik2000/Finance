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
        //IUserService interface is implemented by UserService class in that all the methods are define.
        //using asynchronous programming, the application can work on other task without waiting for the task to be completed
        //async keyword is used to make the method asynchronous, If any Second Method, as method2 has a dependency on method1,
        //then it will wait for the completion of Method1 with the help of await keyword.
        //The async keyword marks the method as asynchronous.
        //The await keyword waits for the async method to complete until it returns a value.
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

        //method of user login
        public async Task<ApiResponse> Login(LoginModel data)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email  && a.isActivated);   //finding the activated user using the email we get from the frontend
            //var user = await _userManager.FindByEmailAsync(data.email);
           
            if (user != null && await _userManager.CheckPasswordAsync(user, data.password)) //checking if the user is not null and password are matching
            {
                var roles = await _userManager.GetRolesAsync(user);     //setting roles

                IdentityOptions _options = new IdentityOptions();       //new identity options

                var tokenDescriptor = new SecurityTokenDescriptor       //setting userId, role, expiration time, encoding the token and generating the token descriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                        new Claim("UserId", user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType, roles.FirstOrDefault())
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_applicationSettings.JWT_Secret_Code)), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);      //creating the security token from the token descriptor and sending the token in the response

                var token = tokenHandler.WriteToken(securityToken);                 //this is jwt token

                var res = new { token = token, email = user.Email, username = user.UserName, fullName = user.FullName, isActivated= user.isActivated };

                return new ApiResponse
                {
                    res = res,
                    success = true
                };
              
            }
            else
            {
                //if the user doesn't exist or the password is incorrect then sending error response
                return new ApiResponse
                {
                    message = "Email or Password is incorrect",
                    success = false
                };
            }
        }

        //method of registering the user
        public async Task<ApiResponse> Register( ApplicationUserModel data)
        {
            data.roles = "Customer";                            //assign roles to the user as Customer
            var applicationUser = new ApplicationUser()
            {
                UserName = data.username,
                Email = data.email,
                FullName = data.fullName,
                isActivated = true,
                Role = "Customer"
            };

            var userFind = await _context.ApplicationUsers.FirstOrDefaultAsync(a => (a.Email == data.email || a.UserName == data.username) && a.isActivated); //finding the user in the database using emails and username and isActivated 

            if (userFind == null)               //if the user we find is null then we create a account
            {
                var result = await _userManager.CreateAsync(applicationUser, data.password);        //if we didn't find the user in the database then we will create a database using CreateAsync Function
                await _userManager.AddToRoleAsync(applicationUser, data.roles);                     //here we are adding the roles in the database using AddToRoleAsync function from identity

                if (result.Succeeded)
                {
                    var userData = await _context.ApplicationUsers.FirstOrDefaultAsync(a => a.Email == data.email && a.UserName == data.username && a.isActivated);     //finding the newest user with emaild id and username and it should be activated for inserting bank data and user data

                    if (userData != null)                   //if the user is not null then we will insert bank data and user data
                    {
                        DateTime localDate = DateTime.Now;

                        var newUser = new UserDetailsModel()    //creating new userDetails using the userId from the user that we find in the database
                        {
                            userId = userData.Id,
                            isActivated = false,
                            isVerified = false,
                            joiningFees = false,
                            dateOfBirth = data.dateOfBirth,
                            UserAddress = data.UserAddress,
                            phoneNumber = data.phoneNumber,
                            createdAt = localDate,
                            joiningFeesAmount = 0.0m

                        };

                        var bankDetails = new BankDetailsModel()        //creating new bankDetails using the userId from the user that we find in the database
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
                        await _context.SaveChangesAsync();      //saving changes in the database
                        return new ApiResponse
                        {
                            message = "Successfully created the user",
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
                        //if the result was not succeed then sending error response or username exist error message
                        message = "Unable to create User or username exist",
                        success = false
                    }; 
                }
            }
            else
            {
                return new ApiResponse
                {
                    //if the user already exist then we will send the error response
                    message = "User already exist, please login",
                    success = false
                }; 
            }
        }

        //method for forget password
        public async Task<ApiResponse> ForgetPassword(string email)
        {
            //var user = await _userManager.FindByEmailAsync(email);
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(a =>a.Email == email && a.isActivated);      //finding if the user exist in the database

            if (user == null)               //if the user is null then send error response
            {
                return new ApiResponse
                {
                    success = false,
                    message = "No User found for the given mail",
                };
            }
            else
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);       //if the user is present then generate Reset password token and pass user in that method
                var encodedToken = Encoding.UTF8.GetBytes(token);                           //encode the token
                var validToken = WebEncoders.Base64UrlEncode(encodedToken);                 // encode the token into base64 url

                string url = $"{_applicationSettings.clientUrl}/user/reset-password?email={email}&token={validToken}";      //url for reseting the password

                await _emailService.SendEmailAsync(email, "Reset Password", "<h1>Click this link to reset this password</h1>" +

                    $"<p>To reset your password <a href='{url}'>Click Here</a></p>"
                    );                                                                                                      //sending email

                return new ApiResponse { success= true, res=validToken, message = "Reset password url has been sent to the email successfully" };       
            }
        }

        //methods for resetting password
        public async Task<ApiResponse> ResetPassword(ResetPasswordModel data)
        {
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(a =>a.Email == data.email && a.isActivated);   //finding if the user exist in the database and should be activated

            if (user == null)   //if the user is null then send error response
            {
                return new ApiResponse
                {
                    success = false,
                    message = "No User found for the given mail",
                };
            }
            else
            {
                var decodedToken = WebEncoders.Base64UrlDecode(data.token);     //decoding the token from base64 url
                string normalToken = Encoding.UTF8.GetString(decodedToken);     //decoding the token to string
                var result = await _userManager.ResetPasswordAsync(user, normalToken, data.password);   //resetting the password to the new password

                if (result.Succeeded)   //if succedd then return true response
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
