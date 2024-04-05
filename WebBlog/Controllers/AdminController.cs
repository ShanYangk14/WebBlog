using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebBlog.Models;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace WebBlog.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme, Policy = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Admin()
        {
            var usersWithPosts = _db.Users.Include(u => u.BlogPosts).ToList();
            return View(usersWithPosts);
        }

        public IActionResult AdminBlog()
        {
            var userEmail = User.Identity.Name;
            var user = _db.Users
                .Include(u => u.BlogPosts)
                    .ThenInclude(p => p.Comments)
                        .ThenInclude(c => c.User) 
                .FirstOrDefault(u => u.Email == userEmail);
            return View(user);
        }

        public IActionResult AddPost(int userId)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                ViewData["UserId"] = userId;
                return View();
            }
            else
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [HttpPost]
        public IActionResult AddPost(BlogPost post, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Path.GetFileName(image.FileName);
                    var imagePath = "/images/" + fileName;

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    post.ImagePath = imagePath;
                }

                _db.BlogPosts.Add(post);
                _db.SaveChanges();
                return RedirectToAction("Admin");
            }
            return View(post);
        }

        public IActionResult EditPost(int userId, int id)
        {
            var post = _db.BlogPosts.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        public IActionResult EditPost(BlogPost post, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = Path.GetFileName(image.FileName);
                    var imagePath = "/images/" + fileName; 

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    post.ImagePath = imagePath;
                }

                _db.BlogPosts.Update(post);
                _db.SaveChanges();
                return RedirectToAction("Admin");
            }
            return View(post);
        }

        public IActionResult DeletePost(int id)
        {
            var post = _db.BlogPosts.Find(id);
            _db.BlogPosts.Remove(post);
            _db.SaveChanges();
            return RedirectToAction("Admin");
        }

        public IActionResult EditUser(int id)
        {
            var user = _db.Users.Find(id);
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                _db.Users.Update(user);
                _db.SaveChanges();
                return RedirectToAction("Admin");
            }
            return View(user);
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _db.Users.Find(id);
            _db.Users.Remove(user);
            _db.SaveChanges();
            return RedirectToAction("Admin");
        }
        public IActionResult UserComments(int userId)
        {
            var userComments = _db.Comments
                                  .Include(c => c.User) 
                                  .Where(c => c.UserId == userId)
                                  .ToList();
            return View(userComments);
        }

        public IActionResult DeleteComment(int commentId)
        {
            var comment = _db.Comments.Find(commentId);
            _db.Comments.Remove(comment);
            _db.SaveChanges();
            return RedirectToAction("Admin");
        }
    }
}
    

