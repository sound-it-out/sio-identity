using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SIO.Identity.Tests.Register.RegisterController.Register.Get
{
    public class WhenUserIsUnAuthenticated : RegisterControllerSpecification<IActionResult>
    {
        protected override Task<IActionResult> Given()
        {
            return Task.FromResult(_controller.Register());
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
