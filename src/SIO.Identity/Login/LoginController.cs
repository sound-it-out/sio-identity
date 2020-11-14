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
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.Users.Commands;
using SIO.Domain.Users.Events;
using SIO.Identity.Login.Requests;
using SIO.Identity.Login.Responses;
using SIO.Migrations;

namespace SIO.Identity.Login
{
    public class LoginController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserManager<SIOUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IClientStore _clientStore;
        private readonly ICommandDispatcher _commandDispatcher;

        public LoginController(IIdentityServerInteractionService interaction,
            UserManager<SIOUser> userManager, 
            IConfiguration configuration,
            IAuthenticationSchemeProvider schemeProvider,
            IClientStore clientStore,
            ICommandDispatcher commandDispatcher)
        {
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (schemeProvider == null)
                throw new ArgumentNullException(nameof(schemeProvider));
            if (clientStore == null)
                throw new ArgumentNullException(nameof(clientStore));
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            _interaction = interaction;
            _userManager = userManager;
            _configuration = configuration;
            _schemeProvider = schemeProvider;
            _clientStore = clientStore;
            _commandDispatcher = commandDispatcher;
        }


        [HttpGet("login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                if (!string.IsNullOrEmpty(returnUrl) && (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl)))
                    return Redirect(returnUrl);
                else
                    return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            return View(await BuildResponseAsync(returnUrl));
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest request, string button)
        {
            if (!ModelState.IsValid)
                return View(await BuildResponseAsync(null));

            if (button != "login")
            {
                var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);

                if (context == null)
                    return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

                await _interaction.GrantConsentAsync(context, new ConsentResponse());

                if (!string.IsNullOrEmpty(request.ReturnUrl) && (_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl)))
#pragma warning disable SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'
                    return Redirect(request.ReturnUrl);
#pragma warning restore SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if(user == null)
            {
                ModelState.AddModelError("", "Please ensure that your username and password are correct.");
                return View(await BuildResponseAsync(null));
            }                

            try
            {
                await _commandDispatcher.DispatchAsync(new LoginCommand(new Guid(user.Id), Guid.NewGuid(), 0, user.Id, request.Password));
            }
            catch(UserDoesntExistException)
            {
                ModelState.AddModelError("", "Please ensure that your username and password are correct.");
            }
            catch(UserIsLockedOutException)
            {
                ModelState.AddModelError("", "Your account has been locked please try again in a few minutes");
            }
            catch(UserNotVerifiedException)
            {
                ModelState.AddModelError("", "Your account is not verified, but a new activation link has been sent to your email.");
            }
            catch(UserIsArchivedException)
            {
                ModelState.AddModelError("", "Your account is deactivated");
            }
            catch(IncorrectPasswordException)
            {
                ModelState.AddModelError("", "Please ensure that your username and password are correct.");
            }

            if(!ModelState.IsValid)
                return View(await BuildResponseAsync(null));

            if (!string.IsNullOrEmpty(request.ReturnUrl) && (_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl)))
#pragma warning disable SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'
                return Redirect(request.ReturnUrl);
#pragma warning restore SCS0027 // Open redirect: possibly unvalidated input in {1} argument passed to '{0}'

            return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));
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

            if (context?.Client?.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);

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
