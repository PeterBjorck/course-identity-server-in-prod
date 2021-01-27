using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{

    /// <summary>
    /// Identity Source code UI
    /// https://github.com/dotnet/aspnetcore/tree/master/src/Identity
    ///
    /// 
    /// </summary>
    public class UserController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task Login()
        {
            await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties()
                {
                    RedirectUri = "/"
                });
        }
  
        /// <summary>
        /// Do the logout
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

            //Important, this method should never return anything.
        }


        /// <summary>
        /// Show the access denied page
        /// </summary>
        /// <returns></returns>
        public IActionResult AccessDenied()
        {
            return View();
        }


        [Authorize]
        public IActionResult Info()
        {
            string idToken = HttpContext.GetTokenAsync("id_token").Result ?? "";
            string accessToken = HttpContext.GetTokenAsync("access_token").Result ?? "";

            //To prevent XSS, make sure the token only contains valid base64 characters or ".-"
            //https://en.wikipedia.org/wiki/Base64#Variants_summary_table
            Regex regex = new Regex(@"^[\w\+\/\=\.-]+$");  //Matches a-z, A-Z, 0-9, including the _ (underscore) character.

            if (regex.Match(idToken).Success)
            {
                ViewData["idToken"] = idToken;
            }
            if (regex.Match(accessToken).Success)
            {
                ViewData["accessToken"] = accessToken;
            }
            return View();
        }      
        
        
        public IActionResult Register()
        {
            return View();
        }
    }
}
