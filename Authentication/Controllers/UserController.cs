using Authentication.Models;
using Authentication.Services;
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
using SendGrid;
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
        private readonly IUserService _userService;
   

        public UserController(IUserService userService,UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager, AuthenticationContext context, IOptions<ApplicationSettings> appSettings)
        {
            _userManager = userManager;
            _signInManager = signinManager;
            _context = context;
            _applicationSettings = appSettings.Value;
            _userService = userService;
          
        }

        [HttpPost]
        [Route("Register")]
        public async Task<Object> PostApplicationUser(ApplicationUserModel data)
        {
            try
            {
                ApiResponse dataResult = await _userService.Register(data);

                if(dataResult.success)
                {
                    return Ok(dataResult.res);
                }
                else
                {
                    return BadRequest(dataResult.message);
                }
             
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginModel data)
        {

            try
            {
                ApiResponse dataResult = await _userService.Login(data);

                if (dataResult.success)
                {
                    return Ok(dataResult.res);
                }
                else
                {
                    return BadRequest(dataResult.message);
                }

            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
        }


        [HttpPost]
        [Route("forgetPassword")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new { message="User Not Found" });
                }
                else
                {
                    var result = await _userService.ForgetPassword(email);

                    if (result.success)
                    {
                        return Ok(result);
                    }
                    else
                    {
                        return BadRequest(new { message = "Something went wrong in sending the reset password url" });
                    }
                    
                }
             
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
        }

        [HttpPost]
        [Route("resetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel data)
        {
            try
            {
                var result = await _userService.ResetPassword(data);

                if (result.success)
                {
                    return Ok(new { message = result.message });
                }
                else
                {
                    return BadRequest(new {message = result.message });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
        }

        [HttpPost]
        [Route("resetPasswordToken")]
        public async Task<IActionResult> ResetPasswordToken(string email)
        {
            try
            {
                var result = await _userService.ResetPasswordToken(email);

                if (result.success)
                {
                    return Ok(new { message = result.message });
                }
                else
                {
                    return BadRequest(new { message = result.res });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
        }
    }
}
