using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SIO.Domain.Users.Commands;
using SIO.Identity.Register.Requests;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;

namespace SIO.Identity.Register
{
    public class RegisterController: Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ICommandDispatcher _commandDispatcher;

        public RegisterController(IConfiguration configuration, ICommandDispatcher commandDispatcher)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            _configuration = configuration;
            _commandDispatcher = commandDispatcher;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return Redirect(_configuration.GetValue<string>("DefaultAppUrl"));

            return View();
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return View(request);

            try
            {
                await _commandDispatcher.DispatchAsync(new RegisterUserCommand(Actor.New(), CorrelationId.New(), request.Email, request.FirstName, request.LastName));
            }
            catch(EmailInUseException)
            {
                ModelState.AddModelError("", "Email is already in use");
            }
            catch(UserNotVerifiedException)
            {
                ModelState.AddModelError("", "Your account is not verified, but a new activation link has been sent to your email.");
            }
            catch(UserCreationException)
            {
                ModelState.AddModelError("", "There was an error completing registration,  please try again");
            }

            if(!ModelState.IsValid)
                return View(request);

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
