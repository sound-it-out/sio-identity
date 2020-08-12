using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace SIO.Identity.Tests.Register.RegisterController.Register.Get
{
    public class WhenUserIsAuthenticated : RegisterControllerSpecification<IActionResult>
    {
        private readonly string _defaultAppUrl = "https://localhost/defaultappurl";
        protected override Task<IActionResult> Given()
        {
            return Task.FromResult(_controller.Register());
        }

        protected override Task When()
        {
            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s.Value).Returns(_defaultAppUrl);
            _configuration.Setup(c => c.GetSection(It.Is<string>((s) => s == "DefaultAppUrl"))).Returns(section.Object);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim("custom-claim", "example claim value"),
                    }, "mock"))
                }
            };

            return Task.CompletedTask;
        }

        [Then]
        public void ThenShouldRedirectToDefaultAppUrl()
        {
            Result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)Result).Url.Should().Be(_defaultAppUrl);
        }
    }
}
