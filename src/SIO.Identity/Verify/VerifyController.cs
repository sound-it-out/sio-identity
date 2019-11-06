using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenEventSourcing.Events;
using SIO.Identity.Verify.Requests;
using SIO.Identity.Verify.Response;
using SIO.Migrations;

namespace SIO.Identity.Verify
{
    public class VerifyController : Controller
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IConfiguration _configuration;

        public VerifyController(UserManager<SIOUser> userManager, SignInManager<SIOUser> signInManager, IConfiguration configuration)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpGet("verify")]
        public IActionResult Verify(string email, string token)
        {
            if (string.IsNullOrEmpty(email))
                ModelState.AddModelError("", "You must suply a valid email address");

            if (string.IsNullOrEmpty(token))
                ModelState.AddModelError("", "You must supply a valid verification token");
            else
                token = Encoding.UTF8.GetString(Convert.FromBase64String(token));

            return View(new VerifyRequest(email, token));
        }

        [HttpPost("verify")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(VerifyRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            if (User.Identity.IsAuthenticated)
                return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || user.IsArchived)
                return Redirect("Error");

            var confirmationResult = await _userManager.ConfirmEmailAsync(user, request.Token);

            if (!confirmationResult.Succeeded)
            {
                foreach (var error in confirmationResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return Redirect("Error");
            }            

            var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Password);

            if(!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(request);
            }

            await _signInManager.SignInAsync(user, false);

            return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));
        }
    }
}
