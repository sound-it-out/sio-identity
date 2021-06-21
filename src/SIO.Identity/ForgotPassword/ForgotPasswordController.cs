using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SIO.Domain.Users.Commands;
using SIO.Identity.ForgotPassword.Requests;
using SIO.Infrastructure;
using SIO.Infrastructure.Commands;
using SIO.Migrations;

namespace SIO.Identity.ForgotPassword
{
    public class ForgotPasswordController : Controller
    {
        private readonly UserManager<SIOUser> _userManager;
        private readonly ICommandDispatcher _commandDispatcher;

        public ForgotPasswordController(UserManager<SIOUser> userManager, ICommandDispatcher commandDispatcher)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));
            if (commandDispatcher == null)
                throw new ArgumentNullException(nameof(commandDispatcher));

            _userManager = userManager;
            _commandDispatcher = commandDispatcher;
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
                try
                {
                    await _commandDispatcher.DispatchAsync(new ForgotPasswordCommand(user.Id, CorrelationId.New()));
                }
                catch(UserDoesntExistException)
                {
                    ModelState.AddModelError("", "There is no account associated with the specified email");
                }
                catch(UserNotVerifiedException)
                {
                    ModelState.AddModelError("", "The account associated with this email has not been activated yet");
                }
                catch(UserIsArchivedException)
                {
                    ModelState.AddModelError("", "Your account is deactivated");
                }
            }
            else
            {
                ModelState.AddModelError("", "There is no account associated with the specified email");
            }

            if(!ModelState.IsValid)
                return View(request);

            return RedirectToAction(nameof(ForgotPasswordSuccess));
        }

        [HttpGet("forgot-password/success")]
        public IActionResult ForgotPasswordSuccess()
        {
            return View();
        }
    }
}
