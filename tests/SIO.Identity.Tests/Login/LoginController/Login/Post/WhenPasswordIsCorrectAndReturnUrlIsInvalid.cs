using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SIO.Domain.Users.Events;
using SIO.Identity.Login.Requests;
using SIO.Migrations;

namespace SIO.Identity.Tests.Login.LoginController.Login.Post
{
    public class WhenPasswordIsCorrectAndReturnUrlIsInvalid : LoginControllerSpecification<IActionResult>
    {
        private readonly string _buttonText = "login";
        private readonly string _defaultAppUrl = "https://localhost/defaultappurl";
        private LoginRequest _request;
        private SIOUser _user;

        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_request, _buttonText);
        }

        protected override async Task When()
        {
            _request = new LoginRequest { Email = "user@mock.test", Password = "Asdf@123456789asdf", ReturnUrl = "https://localhost/mock" };

            var section = new Mock<IConfigurationSection>();
            section.SetupGet(s => s.Value).Returns(_defaultAppUrl);
            _configuration.Setup(c => c.GetSection(It.Is<string>((s) => s == "DefaultAppUrl"))).Returns(section.Object);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(uh => uh.IsLocalUrl(It.IsAny<string>())).Returns(false);

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            _controller.Url = mockUrlHelper.Object;

            var userManager = _serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            _user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = _request.Email,
                FirstName = "firstname",
                LastName = "lastname",
                UserName = _request.Email,
            };

            await userManager.CreateAsync(_user);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(_user);
            await userManager.ConfirmEmailAsync(_user, token);
            await userManager.AddPasswordAsync(_user, _request.Password);
        }

        [Then]
        public void ThenUserLoggedInShouldBePublishedWithExpectedParams()
        {
            var @event = (UserLoggedIn)_events.FirstOrDefault(e => e.GetType() == typeof(UserLoggedIn));

            @event.Should().NotBeNull();
            @event.AggregateId.Should().Be(Guid.Parse(_user.Id));
            @event.UserId.Should().Be(_user.Id);
        }

        [Then]
        public async Task ThenUserAccessFailedCountShouldBeZero()
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = await userManager.FindByEmailAsync(_user.Email);
            user.AccessFailedCount.Should().Be(0);
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
