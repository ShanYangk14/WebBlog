using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBlog.Models
{
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        // Define relationships
        public int UserId { get; set; } // Change data type to int to match User.Id

        // Constructor to initialize CreatedAt to current date and time
        public BlogPost()
        {
            CreatedAt = DateTime.Now;
        }
        public ICollection<Comment> Comments { get; set; }
    }
}
