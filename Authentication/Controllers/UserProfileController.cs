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
    public class UserProfileController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;
        private readonly IUserProfileService _userProfile;
        public UserProfileController(UserManager<ApplicationUser> userManager, AuthenticationContext context, IUserProfileService userProfile)
        {
            _userManager = userManager;
            _context = context;
            _userProfile = userProfile;
        }

        [HttpGet]
        [Authorize(Roles ="Customer")]
        [Route("GetUserProfile")]
        public async Task<Object> GetUserProfile()
        {
            try
            {
                string userId = User.Claims.First(c => c.Type == "UserId").Value;

                var result = await _userProfile.GetUserProfile(userId);

                if (result.success)
                {
                    return Ok(new
                    {
                        data = result.res
                    }) ;
                }
                else
                {
                    return BadRequest(new { message = "Unable to get the user data" });
                }

               
            }
            catch (Exception error)
            {
                return BadRequest(new { message=error });
            }
       
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        [Route("GetUserData/")]
        public async Task<Object> GetUserData()
        {
            try
            {
                string userId = User.Claims.First(c => c.Type == "UserId").Value;

                var result = await _userProfile.GetUserData(userId);

                if (result.success)
                {
                    return Ok(result.res);
                }
                else
                {
                    return BadRequest(new { message = result.message });
                }
            }
            catch (Exception error)
            {

                return BadRequest(new { message = error });

            }
            
        }

        [HttpPut]
        [Authorize(Roles = "Customer")]
        [Route("PostJoiningFees/{id}")]
        public async Task<Object> PostJoiningFees(string id, JoiningFeesModel data)
        {
            try
            {
                var result = await _userProfile.PostJoiningFees(id, data);

                if (result.success)
                {
                    return Ok(new { message = "Joining Fees Submitted" });
                }
                else
                {
                    return BadRequest(new { message = result.message });
                }
            }
            catch (Exception err)
            {
                return BadRequest(new { message = err });
            }
        }

        [HttpPost]
        [Authorize(Roles ="Customer")]
        [Route("BuyProduct")]
        public async Task<Object> BuyProduct(BuyOrderModel data)
        {
            try
            {
                var res = await _userProfile.BuyProduct(data);

                if (res.success)
                {
                    return Ok(new { message = res.message });
                }
                else
                {
                    return BadRequest(new { message = res.message });
                }
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = ex });
            }
            
        }
    
        [HttpPost]
        [Authorize(Roles="Customer")]
        [Route("PayEmiInstallment")]
        public async Task<Object> PayEmiInstallment(EmiPaymentModel data)
        {
            try
            {
                var result = await _userProfile.PayEmiInstallment(data);

                if (result.success)
                {
                    return Ok(new { message = result.message });
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
