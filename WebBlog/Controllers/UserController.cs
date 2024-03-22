using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBlog.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace WebBlog.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Policy = "AuthenticatedUser")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult UserPage()
        {
            var userEmail = User.Identity.Name;
            var user = _db.Users.Include(u => u.BlogPosts).FirstOrDefault(u => u.Email == userEmail);
            return View(user);
        }

        // Implement other actions for user profile, blog post management, etc.
    }
}
