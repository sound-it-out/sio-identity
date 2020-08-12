using System;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SIO.Identity.Login.Requests;
using SIO.Migrations;

namespace SIO.Identity.Tests.Login.LoginController.Login.Post
{
    public class WhenButtonIsNotLoginAndContextIsNull : LoginControllerSpecification<IActionResult>
    {
        private readonly string _buttonText = "mock";
        private readonly string _defaultAppUrl = "https://localhost/defaultappurl";
        private LoginRequest _request;

        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_request, _buttonText);
        }

        protected override Task When()
        {
            _request = new LoginRequest { Email = "user@mock.test", Password = "Asdf@123456789asdf", ReturnUrl = "https://localhost/mock" };

            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s.Value).Returns(_defaultAppUrl);
            _configuration.Setup(c => c.GetSection(It.Is<string>((s) => s == "DefaultAppUrl"))).Returns(section.Object);

            var interaction = (MockIdentityServerInteraction)_serviceProvider.GetRequiredService<IIdentityServerInteractionService>();
            interaction.HasAuthorizationContext = false;

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(uh => uh.IsLocalUrl(It.IsAny<string>())).Returns(true);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.Url = mockUrlHelper.Object;

            return Task.CompletedTask;
        }

        [Then]
        public void ThenShouldReturnRedirectToDefaultAppUrl()
        {
            Result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)Result).Url.Should().Be(_defaultAppUrl);
        }

        [Then]
        public void ThenModelStateShouldBeValid()
        {
            _controller.ModelState.IsValid.Should().BeTrue();
        }
    }
}
