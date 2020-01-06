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
    public class WhenUserIsNotVerified : LoginControllerSpecification<IActionResult>
    {
        private readonly string _buttonText = "login";
        private LoginRequest _request;

        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_request, _buttonText);
        }

        protected override async Task When()
        {
            _request = new LoginRequest { Email = "user@mock.test", Password = "password" };
            _controller.ValidateRequest(_request);

            var userManager = _serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = _request.Email,
                FirstName = "firstname",
                LastName = "lastname",
                UserName = _request.Email,
            };

            await userManager.CreateAsync(user);
            await userManager.GenerateEmailConfirmationTokenAsync(user);
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
