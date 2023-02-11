using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

using SWP_Login.Models;
using SWP_Login.Repository;
using SWP_Login.Service;
using SWP_Login.Service.impl;
using SWP_Login.Utils;

namespace SWP_Login.Controllers
{
    [Route("api/[controller]")]
    public class LoginController : Controller
    {
        [HttpPost]
        public IActionResult Login([FromBody] Account model)
        {
            IAccountService accountService = new AccountServiceImpl();
            Console.WriteLine("controller");
            return Ok(accountService.login(model));
            
        }
        [Route("google")]
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

    }
}

