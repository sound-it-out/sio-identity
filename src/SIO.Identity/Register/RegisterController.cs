using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenEventSourcing.Commands;
using SIO.Domain.User.Commands;
using SIO.Identity.Register.Requests;
using SIO.Migrations;

namespace SIO.Identity.Register
{
    public class RegisterController: Controller
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IConfiguration _configuration;
        private readonly UserManager<SIOUser> _userManager;

        public RegisterController(ICommandDispatcher commandDispatcher, IConfiguration configuration, UserManager<SIOUser> userManager)
        {
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            _commandDispatcher = commandDispatcher;
            _configuration = configuration;
            _userManager = userManager;
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            return View();
        }

        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return View(request);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email is already in use");
                }
                else
                {
                    ModelState.AddModelError("", "Your account is not verified, but a new activation link has been sent to your email.");
                    await _userManager.GenerateEmailConfirmationTokenAsync(user);
                }

                return View(request);
            }

            await _commandDispatcher.DispatchAsync(new RegisterUserCommand(
                aggregateId: Guid.NewGuid(),
                correlationId: Guid.NewGuid(),
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                roles: new string[] { },
                version: 0,
                userId: ""
            ));

            return RedirectToAction(nameof(Registered));
        }

        [HttpGet("registered")]
        public IActionResult Registered()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            return View();
        }
    }
}
