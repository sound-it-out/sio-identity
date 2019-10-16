using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SIO.Identity.Login.Requests;
using SIO.Migrations;

namespace SIO.Identity.Login
{
    public class LoginController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;

        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                if (_interaction.IsValidReturnUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return Redirect("");

            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request, string button)
        {
            var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);

            if (button != "login")
            {
                if (context == null)
                    return Redirect("");

                await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                return Redirect(request.ReturnUrl);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    // throw error
                }

                var validLogin = await _userManager.CheckPasswordAsync(user, request.Password);


                if (!validLogin || user.IsArchived)
                {
                    // throw error
                }

                await _signInManager.SignInAsync(user, false);

                if (_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl))
                    return Redirect(request.ReturnUrl);
                 
                return Redirect("");
            }

            return Redirect("");
        }
    }
}
