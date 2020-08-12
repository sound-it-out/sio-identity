using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SIO.Identity.Login.Requests;
using SIO.Migrations;

namespace SIO.Identity.Tests.Login.LoginController.Login.Post
{
    public class WhenPasswordIsIncorrect : LoginControllerSpecification<IActionResult>
    {
        private readonly string _buttonText = "login";
        private LoginRequest _request;
        private SIOUser _user;

        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_request, _buttonText);
        }

        protected override async Task When()
        {
            _request = new LoginRequest { Email = "user@mock.test", Password = "password" };
            _controller.ValidateRequest(_request);

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
            await userManager.AddPasswordAsync(_user, "correct_password");
        }

        [Then]
        public async Task ThenUserAccessFailedCountShouldBeOne()
        {
            var userManager = _serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = await userManager.FindByEmailAsync(_user.Email);
            user.AccessFailedCount.Should().Be(1);
        }

        [Then]
        public void ThenShouldReturnViewResult()
        {
            Result.Should().BeOfType<ViewResult>();
        }

        [Then]
        public void ThenModelStateShouldBeInvalid()
        {
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Then]
        public void ThenModelStateShouldNotContainErrorForPasswordProperty()
        {
            _controller.ModelState.ContainsKey(nameof(LoginRequest.Password)).Should().BeFalse();
        }

        [Then]
        public void ThenModelStateShouldNotContainErrorForEmailProperty()
        {
            _controller.ModelState.ContainsKey(nameof(LoginRequest.Email)).Should().BeFalse();
        }
    }
}
