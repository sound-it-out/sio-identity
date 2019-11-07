using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIO.Identity.Verify.Requests
{
    public class VerifyRequest : IValidatableObject
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RePassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != RePassword)
                yield return new ValidationResult("Passwords do not match");
        }

        public VerifyRequest()
        {

        }

        public VerifyRequest(string email, string token)
        {
            Email = email;
            Token = token;
        }
    }
}
