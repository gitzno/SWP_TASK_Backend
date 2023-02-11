using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SWP_Login.Utils;

namespace SWP_Login.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : Controller
    {

        [JwtFilter]
        [HttpGet , Authorize(Roles = "Basic")]
        public IActionResult Index()
        {  
            return Ok(new ResponseObject
            {
                Status = "500",
                Message = "JWT",
                Data = null
            });
        }
    }
}
