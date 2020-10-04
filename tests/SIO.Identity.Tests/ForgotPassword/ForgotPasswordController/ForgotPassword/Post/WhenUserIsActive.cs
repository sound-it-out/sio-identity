using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SIO.Domain.Users.Events;
using SIO.Identity.ForgotPassword.Requests;
using SIO.Migrations;

namespace SIO.Identity.Tests.ForgotPassword.ForgotPasswordController.ForgotPassword.Post
{
    public class WhenUserIsActive : ForgotPasswordControllerSpecification<IActionResult>
    {
        private ForgotPasswordRequest _request;
        private SIOUser _user;
        protected override Task<IActionResult> Given()
        {
            return _controller.ForgotPassword(_request);
        }

        protected override async Task When()
        {
            _request = new ForgotPasswordRequest { Email = "user@mock.test" };

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

            _controller.ValidateRequest(_request);
        }

        [Then]
        public void ThenShouldReturnRedirectToActionResult()
        {
            Result.Should().BeOfType<RedirectToActionResult>();
        }

        [Then]
        public void ThenModelStateShouldBeValid()
        {
            _controller.ModelState.IsValid.Should().BeTrue();
        }

        [Then]
        public void ThenOnlyOneEventShouldBePublished()
        {
            _events.Should().HaveCount(1);
        }

        [Then]
        public void ThenUserPasswordTokenGeneratedEventShouldBePublishedWithExpectedParams()
        {
            var @event = (UserPasswordTokenGenerated)_events.FirstOrDefault(e => e.GetType() == typeof(UserPasswordTokenGenerated));

            @event.Should().NotBeNull();
            @event.AggregateId.Should().Be(Guid.Parse(_user.Id));
            @event.UserId.Should().Be(_user.Id);
            @event.Token.Should().NotBeNullOrEmpty();
        }
    }
}
