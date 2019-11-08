using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenEventSourcing.Events;
using SIO.Domain.User.Events;
using SIO.Identity.Logout.Requests;
using SIO.Migrations;

namespace SIO.Identity.Logout
{
    public class LogoutController : Controller
    {
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IPersistedGrantService _persistedGrantService;
        private readonly SignInManager<SIOUser> _signInManager;
        private readonly UserManager<SIOUser> _userManager;

        public LogoutController(IEventBusPublisher eventBusPublisher, IIdentityServerInteractionService interaction, IPersistedGrantService persistedGrantService, SignInManager<SIOUser> signInManager)
        {
            if (eventBusPublisher == null)
                throw new ArgumentNullException(nameof(eventBusPublisher));
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (persistedGrantService == null)
                throw new ArgumentNullException(nameof(persistedGrantService));
            if (signInManager == null)
                throw new ArgumentNullException(nameof(signInManager));

            _eventBusPublisher = eventBusPublisher;
            _interaction = interaction;
            _persistedGrantService = persistedGrantService;
            _signInManager = signInManager;
        }

        [HttpGet("logout")]
        public IActionResult Logout(string logoutId)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction(nameof(LoggedOut));

            return View(new LogoutRequest { LogoutId = logoutId });
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            if (!User.Identity.IsAuthenticated)
                return View(nameof(LoggedOut));

            var idp = User?.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            var subjectId = User.Identity.GetSubjectId();

            if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
            {
                if (request.LogoutId == null)
                    request.LogoutId = await _interaction.CreateLogoutContextAsync();
            }

            await _signInManager.SignOutAsync();

            var logout = await _interaction.GetLogoutContextAsync(request.LogoutId);

            await _persistedGrantService.RemoveAllGrantsAsync(subjectId, logout?.ClientId);

            var user = await _userManager.FindByIdAsync(subjectId);
            user.Version++;
            await _userManager.UpdateAsync(user);
            await _eventBusPublisher.PublishAsync(new UserLoggedOut(new Guid(subjectId), Guid.NewGuid(), user.Version, subjectId));

            return View(nameof(LoggedOut));
        }

        [HttpGet("logged-out")]
        public IActionResult LoggedOut()
        {
            return View();
        }
    }
}
