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
    public class WhenButtonIsNotLoginAndContextIsNotNull : LoginControllerSpecification<IActionResult>
    {
        private readonly string _buttonText = "mock";
        private LoginRequest _request;

        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_request, _buttonText);
        }

        protected override Task When()
        {
            _request = new LoginRequest { Email = "user@mock.test", Password = "Asdf@123456789asdf", ReturnUrl = "https://localhost/mock" };

            var interaction = (MockIdentityServerInteraction)_serviceProvider.GetRequiredService<IIdentityServerInteractionService>();
            interaction.HasAuthorizationContext = true;

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
        public void ThenShouldReturnRedirectToReturnUrl()
        {
            Result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)Result).Url.Should().Be(_request.ReturnUrl);
        }

        [Then]
        public void ThenModelStateShouldBeValid()
        {
            _controller.ModelState.IsValid.Should().BeTrue();
        }
    }
}
