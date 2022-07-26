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
    public class ProductsController : ControllerBase
    {
        private readonly AuthenticationContext _context;
        private UserManager<ApplicationUser> _userManager;
        public ProductsController(UserManager<ApplicationUser> userManager, AuthenticationContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        [Route("GetAllProducts")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetPaymentDetails()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        [Route("GetProductDetails/{id}")]
        public async Task<Object> GetProductDetails(int? id)
        {
            try
            {
                if(id == null)
                {
                     return BadRequest(new { message = "Id is null unable to get product details" });
                }
                else
                {
                    var productDetails = await _context.Products.FirstOrDefaultAsync(item => item.productId == id);

                    if(productDetails != null)
                    {
                        return Ok(new { data = productDetails });
                    }
                    else
                    {
                        return BadRequest(new { message = "Product details is not available" });
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
