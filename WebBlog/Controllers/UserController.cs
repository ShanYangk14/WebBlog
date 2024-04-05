using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBlog.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;

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
            var user = _db.Users
                .Include(u => u.BlogPosts) 
                    .ThenInclude(p => p.Comments)
                     .ThenInclude(c => c.User)
                .FirstOrDefault(u => u.Email == userEmail);
            return View(user);
        }

        [HttpPost]
        public IActionResult AddComment(int postId, string content)
        {
            var post = _db.BlogPosts.Include(p => p.Comments).FirstOrDefault(p => p.Id == postId);
            if (post == null)
            {
                return NotFound();
            }

            var user = _db.Users.FirstOrDefault(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var comment = new Comment
            {
                Content = content,
                CreatedAt = DateTime.Now,
                UserId = user.Id,
                User = user
            };

            post.Comments.Add(comment);
            _db.SaveChanges();

            return RedirectToAction("UserPage", "User");
        }

        [HttpPost]
        public IActionResult DeleteComment(int commentId)
        {
            var comment = _db.Comments
                .Include(c => c.User) 
                .FirstOrDefault(c => c.Id == commentId);

            if (comment == null)
            {
                return NotFound();
            }

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (User.Identity.Name != comment.User.Email)
            {
                return Forbid();
            }

            _db.Comments.Remove(comment);
            _db.SaveChanges();

            return RedirectToAction("UserPage", "User");
        }


        public IActionResult BlogInspect()
        {
            var otherBlog = _db.Users.Include(u => u.BlogPosts).ToList();
            return View(otherBlog);
        }
        public IActionResult ViewBlogPost(int postId)
        {
            var post = _db.BlogPosts
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefault(p => p.Id == postId);

            if (post == null)
            {
                return NotFound();
            }

            var user = _db.Users.FirstOrDefault(u => u.Id == post.UserId);

            if (user == null)
            {
               
                ViewData["UserNotFound"] = true;
            }
            else
            {
                ViewData["UserFirstName"] = user.FirstName;
                ViewData["UserLastName"] = user.LastName;
                ViewData["UserEmail"] = user.Email;
            }

            return View(post);
        }

    }
}
