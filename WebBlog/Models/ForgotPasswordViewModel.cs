using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
