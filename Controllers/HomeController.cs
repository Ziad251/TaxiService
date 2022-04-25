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
        private MongoCollection _userDB;
        private IGetClaimsProvider _userClaims;

        public HomeController(
        MongoCollection userDB,
        IGetClaimsProvider userClaims)
        {
            _userDB = userDB;
            _userClaims = userClaims;
        }

        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Signup(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();

        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string firstname, string lastname, string username, string password, string email, string phone)
        {
            var user = await _userDB.GetAsync(username);
            if(user != null)
            {
             TempData["Error"] = "There is an account under that username. Please provide a unique username";
                return View();   
            }
            User newuser = new User();
            Login data = new Login();
            Name names = new Name();
            data.username = username;
            data.password = password;
            names.first = firstname;
            names.last = lastname;
            newuser.login = data;
            newuser.name = names;
            newuser.email = email;
            newuser.phone = phone;
            await _userDB.CreateOneAsync(newuser);
            var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, newuser.name.last),
                        new Claim(ClaimTypes.GivenName, newuser.name.first),
                        new Claim(ClaimTypes.MobilePhone, newuser.phone),
                        new Claim(ClaimTypes.Email, newuser.email),
                        new Claim("username", newuser.login.username)
                    };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            Task.WaitAll();
            return Redirect("/Home/Index");
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
            var user = await _userDB.GetAsync(username);
            if(user == null){
                 return BadRequest();
            }
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
            if (user.login.password != password)
            {
            TempData["Error"] = "Invalid Username or Password!";
            return BadRequest();
            }
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
            ViewBag.destination = search;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }

}
