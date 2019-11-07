using System.ComponentModel.DataAnnotations;

namespace SIO.Identity.Login.Requests
{
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
