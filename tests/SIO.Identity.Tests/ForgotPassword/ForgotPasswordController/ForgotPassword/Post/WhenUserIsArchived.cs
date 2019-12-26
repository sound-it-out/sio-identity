using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SIO.Identity.ForgotPassword.Requests;
using SIO.Migrations;

namespace SIO.Identity.Tests.ForgotPassword.ForgotPasswordController.ForgotPassword.Post
{
    public class WhenUserIsArchived : ForgotPasswordControllerSpecification<IActionResult>
    {
        private ForgotPasswordRequest _request;
        protected override Task<IActionResult> Given()
        {
            return _controller.ForgotPassword(_request);
        }

        protected override async Task When()
        {
            _request = new ForgotPasswordRequest { Email = "user@mock.test" };

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
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);
            user.IsArchived = true;
            await userManager.UpdateAsync(user);

            _controller.ValidateRequest(_request);
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
        public void ThenModelStateShouldNotContainErrorForEmailProperty()
        {
            _controller.ModelState.ContainsKey(nameof(ForgotPasswordRequest.Email)).Should().BeFalse();
        }
    }
}
