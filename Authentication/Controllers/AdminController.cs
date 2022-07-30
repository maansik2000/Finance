using Authentication.Models;
using Authentication.Services;
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
        private readonly IAdminServices _adminService;
        public AdminController(UserManager<ApplicationUser> userManager, AuthenticationContext context, IAdminServices adminServices)
        {
            _userManager = userManager;
            _context = context;
            _adminService = adminServices;
        }

        [HttpPost]
        [Route("Signup")]
        public async Task<Object> AdminSignUp(AdminModel data)
        {
            try
            {
                var result = await _adminService.AdminSignUp(data);

                if (result.success)
                {
                    return Ok(result.res);
                }
                else
                {
                    return BadRequest(new { message = result.message });
                }
            }
            catch (Exception ex)
            {
                
                return BadRequest(new { message = ex });
            
            }
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        [Route("getAllUsers")]
        public async Task<ActionResult<IEnumerable<UserDetailsModel>>> GetAllUsers()
        {
            try
            {
                var result = await _adminService.GetAllUsers();
                if (result.success)
                {
                    return Ok(result.res);
                }
                else
                {
                    return BadRequest(new { message = result.message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex });
            }
        }

        [HttpGet]
        [Authorize(Roles ="Admin")]
        [Route("GetUserDetails/{id}")]
        public async Task<Object> getUserDetails(string id)
        {
            try
            {
                var result = await _adminService.GetUserDetails(id);
                if (result.success)
                {
                    return Ok(new { data = result.res });
                }
                else
                {
                    return BadRequest(new { message = result.message });
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
            try
            {
                string userId = User.Claims.First(c => c.Type == "UserId").Value;
                var result = await _adminService.GetAdminProfile(userId);
                if (result.success)
                {
                    return Ok(new { result.res });
                }
                else
                {
                    return BadRequest(new { message = result.message });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
            
        }

        [HttpPut]
        [Route("ActivateAccount/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<Object> ActivateAccount(string id, BankDetailsModel data)
        {
            try
            {
                var result = await _adminService.ActivateAccount( id,  data);
                if (result.success)
                {
                    return Ok(new { message= "Account is activated" });
                }
                else
                {
                    return BadRequest(new { message = result.message });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex });
            }
            
        }
    
        [HttpPost]
        [Route("DeleteUser/{id}")]
        [Authorize(Roles ="Admin")]
        public async Task<Object> DeleteUser(string id)
        {
            try
            {
                var result = await _adminService.DeleteUser(id);
                if (result.success)
                {
                    return Ok(new { message = "Account is Deleted Successfully" });
                }
                else
                {
                    return BadRequest(new { message = result.message });
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex });
            }
        }
    }
}
