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

        public int UserId { get; set; } // Change data type to int to match User.Id
        public BlogPost()
        {
            CreatedAt = DateTime.Now;
        }
        public string ImagePath { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
}
