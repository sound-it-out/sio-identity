using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace SIO.Identity.Tests.Login.LoginController.Login.Get
{
    public class WhenUserIsUnAuthenticated : LoginControllerSpecification<IActionResult>
    {
        private readonly string _returnUrl = "https://localhost/mock";
        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_returnUrl);
        }

        protected override Task When()
        {
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            };

            return Task.CompletedTask;
        }

        [Then]
        public void ThenShouldReturnViewResult()
        {
            Result.Should().BeOfType<ViewResult>();
        }
    }
}
