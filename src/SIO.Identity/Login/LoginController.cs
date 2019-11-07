using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SIO.Identity.Login.Requests;
using SIO.Identity.Login.Responses;
using SIO.Migrations;

namespace SIO.Identity.Login
{
    public class LoginController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserManager<SIOUser> _userManager;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IClientStore _clientStore;

        public LoginController(IIdentityServerInteractionService interaction,
            UserManager<SIOUser> userManager, 
            SignInManager<SIOUser> signInManager, 
            IConfiguration configuration,
            IAuthenticationSchemeProvider schemeProvider,
            IClientStore clientStore)
        {
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (schemeProvider == null)
                throw new ArgumentNullException(nameof(schemeProvider));
            if (clientStore == null)
                throw new ArgumentNullException(nameof(clientStore));

            _interaction = interaction;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _schemeProvider = schemeProvider;
            _clientStore = clientStore;
        }


        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                else
                    return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            return View(await BuildResponseAsync(returnUrl));
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string button)
        {   
            if (button != "login")
            {
                var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);

                if (context == null)
                    return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

                await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);

                if (_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl))
#pragma warning disable SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'
                    return Redirect(request.ReturnUrl);
#pragma warning restore SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'
            } 
            else if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Please ensure that your username and password are correct.");

                    return View(await BuildResponseAsync(null));
                }

                if (await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError("", "Your account has been locked please try again in a few minutes");

                    return View(await BuildResponseAsync(null));
                }

                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Your account is not verified, but a new activation link has been sent to your email.");

                    await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    return View(await BuildResponseAsync(null));
                }

                var validLogin = await _userManager.CheckPasswordAsync(user, request.Password);


                if (!validLogin || user.IsArchived)
                {
                    await _userManager.AccessFailedAsync(user);
                    ModelState.AddModelError("", "Please ensure that your username and password are correct.");

                    return View(await BuildResponseAsync(null));
                }

                await _signInManager.SignInAsync(user, false);
                await _userManager.ResetAccessFailedCountAsync(user);

                if (_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl))
#pragma warning disable SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'
                    return Redirect(request.ReturnUrl);
#pragma warning restore SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'

                return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));
            }

            return View(await BuildResponseAsync(null));
        }

        private async Task<LoginResponse> BuildResponseAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            if (context?.IdP != null)
                throw new NotImplementedException();

            var allowLocal = true;
            var schemes = await _schemeProvider.GetAllSchemesAsync();
            var providers = schemes.Where(scheme => !string.IsNullOrWhiteSpace(scheme.DisplayName))
                                   .Select(scheme => new ExternalProvider(scheme.DisplayName, scheme.Name));

            if (context?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);

                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.Scheme));
                }
            }

            return new LoginResponse(context?.LoginHint, returnUrl, allowLocal, providers);
        }
    }
}
