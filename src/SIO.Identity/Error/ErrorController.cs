using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;

namespace SIO.Identity.Error
{
    public class ErrorController : Controller
    {
        private readonly IIdentityServerInteractionService _identityServerInteractionService;

        public ErrorController(IIdentityServerInteractionService identityServerInteractionService)
        {
            if (identityServerInteractionService == null)
                throw new ArgumentNullException(nameof(identityServerInteractionService));

            _identityServerInteractionService = identityServerInteractionService;
        }

        [HttpGet("error")]
        public async Task<IActionResult> Error(string errorId)
        {
            var errorContext = await _identityServerInteractionService.GetErrorContextAsync(errorId);
            return View(errorContext);
        }
    }
}
