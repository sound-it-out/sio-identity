using System;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using OpenEventSourcing.Commands;
using SIO.Domain.Users.Commands;
using SIO.Identity.Logout.Requests;

namespace SIO.Identity.Logout
{
    public class LogoutController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ICommandDispatcher _commandDispatcher;

        public LogoutController(IIdentityServerInteractionService interaction, ICommandDispatcher commandDispatcher)
        {
            if (interaction == null)
                throw new ArgumentNullException(nameof(interaction));
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            _interaction = interaction;
            _commandDispatcher = commandDispatcher;
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

            await _commandDispatcher.DispatchAsync(new LogoutCommand(new Guid(subjectId), Guid.NewGuid(), 0, subjectId, request.LogoutId));

            return View(nameof(LoggedOut));
        }

        [HttpGet("logged-out")]
        public IActionResult LoggedOut()
        {
            return View();
        }
    }
}
