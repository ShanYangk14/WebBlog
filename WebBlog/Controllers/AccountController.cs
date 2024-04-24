using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBlog.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace WebBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly EmailService _mailService;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger, ApplicationDbContext db, RoleManager<IdentityRole<int>> roleManager, EmailService mailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _db = db;
            _roleManager = roleManager;
            _mailService = mailService;
        }

        public IActionResult SendEmail()
        {
            var response = _mailService.SendSimpleMessage("duongvietmy2002@gmail.com", "Hello", "Testing Mailgun awesomeness!");

            // Handle the response as needed
            if (response.IsSuccessful)
            {
                // Email sent successfully
                return Content("Email sent successfully!");
            }
            else
            {
                // Email sending failed
                return Content("Failed to send email. Error: " + response.ErrorMessage);
            }
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterAsync(User _user, bool isAdmin)
        {
            try
            { 
                var existingUser = await _userManager.FindByEmailAsync(_user.Email);

                if (existingUser != null)
                {
                    ViewBag.error = "Email already exists";
                    return View();
                }

                var user = new User
                {
                    FirstName = _user.FirstName,
                    LastName = _user.LastName,
                    UserName = _user.Email, 
                    Email = _user.Email,
                    Password = _user.Password,
                    PasswordHash = _userManager.PasswordHasher.HashPassword(null, _user.Password),
                    ResetToken = string.Empty,
                    ResetTokenExpiration = DateTime.UtcNow,
                    EmailConfirmationToken = Guid.NewGuid().ToString(),
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    if (isAdmin)
                    {
                        await _userManager.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(user, "AuthenticatedUser"); 
                    }

                    if (isAdmin)
                    {
                        return RedirectToAction("Admin", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("UserPage", "User");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration.");
                ViewBag.error = "An error occurred during registration.";
                return View();
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
             
                var user = await _db.Users.FirstOrDefaultAsync(s => s.Email == email && s.Password == password);

                if (user != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
            };

                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "AuthenticatedUser"));
                    }

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    if (claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"))
                    {
                        return RedirectToAction("Admin", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("UserPage", "User");
                    }
                }
                else
                {
                    ViewBag.error = "Login failed";
                    return RedirectToAction(nameof(Login));
                }
            }

            return View();
        }


        public async Task<ActionResult> Logout()
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.Session.Clear();
                    return RedirectToAction("Index", "Home");
                }
                public IActionResult ResetPassword(string token)
                {
                    var user = _db.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiration > DateTime.UtcNow);

                    if (user != null)
                    {
                        return View(new ResetPasswordViewModel { Token = token });
                    }

                    return RedirectToAction("InvalidToken");
                }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null)
                {
                    user.ResetToken = Guid.NewGuid().ToString();
                    user.ResetTokenExpiration = DateTime.UtcNow.AddHours(1);

                    _db.SaveChanges();

                    return RedirectToAction("ResetPassword", new { token = user.ResetToken });
                }
                else
                {
                    ViewBag.Error = "Email address not found.";
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            var user = _db.Users.FirstOrDefault(u => u.ResetToken == model.Token && u.ResetTokenExpiration > DateTime.UtcNow);

            if (user != null)
            {

                user.Password = model.NewPassword;
                user.ConfirmPassword = model.NewPassword;
                user.ResetToken = string.Empty;
                user.ResetTokenExpiration = DateTime.UtcNow;

                _db.SaveChanges();

                return RedirectToAction("Login");
            }

            return RedirectToAction("InvalidToken");
        }

        public IActionResult InvalidToken()
        {

            return View();
        }
    }
}
