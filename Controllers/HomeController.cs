using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mapsmvcwebapp.Models;
using mapsmvcwebapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using mapsmvcwebapp.Utils;
using System.Collections.Generic;
using MongoDB.Driver.GeoJsonObjectModel;

namespace mapsmvcwebapp.Controllers
{

    public class HomeController : Controller
    {
        private readonly FakeUsers _fetchUsers;
        private mongoServiceUser _userDB;
        private IGetClaimsProvider _userClaims;

        public HomeController(
        FakeUsers users,
        mongoServiceUser userDB,
        IGetClaimsProvider userClaims)
        {
            _fetchUsers = users;
            _userDB = userDB;
            _userClaims = userClaims;
        }

        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }



        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Validate([FromForm] string username, string password, string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var user = _userDB.GetAsync(username).Result;
            if (user.login.password == password)
            {
                var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.name.last),
                        new Claim(ClaimTypes.GivenName, user.name.first),
                        new Claim(ClaimTypes.MobilePhone, user.phone),
                        new Claim(ClaimTypes.Email, user.email),
                        new Claim("username", username)
                    };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                Task.WaitAll();
                return Redirect(returnUrl);
            }
            TempData["Error"] = "Invalid Username or Password!";
            return View("login");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/Home/Index");
        }


        [Authorize]
        [HttpPost("map")]
        public IActionResult Map([FromForm] string search)
        {
            // Do some addrss validation 
            ViewBag.destination = search;
            return View();
        }




       

        public IActionResult Privacy()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }

}
