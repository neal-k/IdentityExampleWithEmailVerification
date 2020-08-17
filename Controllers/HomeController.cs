using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NETCore.MailKit.Core;

namespace Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            // Required to create the HTTPContext for browser to handle logging in and out
            _signInManager = signInManager;
            _emailService = emailService;
        }
        public IActionResult Index()
        {
            return View();
        }
        
        [Authorize]
        public IActionResult Secret()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Login Functionality
            var user = await _userManager.FindByNameAsync(username);

            if (user != null)
            {
                //sign in user
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // Register functionality
            var user = new IdentityUser
            {
                UserName = username,
                Email = "",
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // generate email token
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var link = Url.Action(nameof(VerifyEmail), "Home", new {userId = user.Id, code}, Request.Scheme, Request.Host.ToString());

                await _emailService.SendAsync("test@test.com", "email verify", $"<a href=\"{link}\">Verify Email</a>", true);

                return RedirectToAction("EmailVerification");
            }

            return RedirectToAction("Index");
        }

        public IActionResult ForgotPassword() => View();
        
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if(user != null)
            {
                // generate password reset code
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                var link = Url.Action(nameof(PasswordReset), "Home", new {code}, Request.Scheme, Request.Host.ToString());

                await _emailService.SendAsync("test@test.com", "password reset", $"<a href=\"{link}\">Reset Password</a>", true);

                return RedirectToAction("EmailVerification");
            }

            return BadRequest();
        }

        public IActionResult PasswordReset(string code) => View();

        [HttpPost]
        public async Task<IActionResult> PasswordReset(string username, string code, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            var result = await _userManager.ResetPasswordAsync(user, code, password);
            if (result.Succeeded) 
            {
                return RedirectToAction("Login");
            }
            return BadRequest();
        }

        public async Task<IActionResult> VerifyEmail(String userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest();
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded) 
            {
                return View();
            }
            return BadRequest();
        } 

        public IActionResult EmailVerification() => View();

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }
    }
}