using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenEventSourcing.Commands;
using OpenEventSourcing.Events;
using SIO.Domain.User.Commands;
using SIO.Domain.User.Events;
using SIO.Identity.Verify.Requests;
using SIO.Migrations;

namespace SIO.Identity.Verify
{
    public class VerifyController : Controller
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ICommandDispatcher _commandDispatcher;

        public VerifyController(UserManager<SIOUser> userManager, IConfiguration configuration, ICommandDispatcher commandDispatcher)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            _userManager = userManager;
            _configuration = configuration;
            _commandDispatcher = commandDispatcher;
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
            {
                ModelState.AddModelError("", "There is no account linked to the supplied email");
                return View(request);
            }

            try
            {
                await _commandDispatcher.DispatchAsync(new VerifyUserCommand(new Guid(user.Id), Guid.NewGuid(), 1, "", request.Token, request.Password));
            }
            catch(EmailConfirmationException e)
            {
                ModelState.AddModelError("", e.Message);
            }
            catch(PasswordCreationException e)
            {
                ModelState.AddModelError("", e.Message);
            }

            if (!ModelState.IsValid)
                return View(request);

            return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));
        }
    }
}
