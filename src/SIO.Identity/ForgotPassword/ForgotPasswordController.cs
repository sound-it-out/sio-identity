using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SIO.Identity.ForgotPassword.Requests;
using SIO.Migrations;

namespace SIO.Identity.ForgotPassword
{
    public class ForgotPasswordController : Controller
    {
        private readonly UserManager<SIOUser> _userManager;

        public ForgotPasswordController(UserManager<SIOUser> userManager)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            _userManager = userManager;
        }

        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordRequest());
        }

        [HttpPost("forgot-password")]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return View(request);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                bool hasError = false;
                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "The account associated with this email has not been activated yet");
                    hasError = true;
                }
                
                if(user.IsArchived)
                {
                    ModelState.AddModelError("", "Your account has been deactivated");
                    hasError = true;
                }

                if (hasError)
                    return View(request);

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                return RedirectToAction(nameof(ForgotPasswordSuccess));
            }
            else
            {
                ModelState.AddModelError("", "There is no account associated with the specified email");
            }

            return View(request);
        }

        [HttpGet("forgot-password/success")]
        public IActionResult ForgotPasswordSuccess()
        {
            return View();
        }
    }
}
