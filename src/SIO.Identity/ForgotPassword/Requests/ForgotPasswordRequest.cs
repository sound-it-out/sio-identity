using System.ComponentModel.DataAnnotations;

namespace SIO.Identity.ForgotPassword.Requests
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
