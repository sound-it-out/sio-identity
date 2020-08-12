using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SIO.Identity.ForgotPassword.Requests;

namespace SIO.Identity.Tests.ForgotPassword.ForgotPasswordController.ForgotPassword.Post
{
    public class WhenEmailIsEmpty : ForgotPasswordControllerSpecification<IActionResult>
    {
        private ForgotPasswordRequest _request;
        protected override Task<IActionResult> Given()
        {
            return _controller.ForgotPassword(_request);
        }

        protected override Task When()
        {
            _request = new ForgotPasswordRequest { Email = string.Empty };
            _controller.ValidateRequest(_request);
            return Task.CompletedTask;
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
        public void ThenModelStateShouldContainErrorForEmailProperty()
        {
            _controller.ModelState.ContainsKey(nameof(ForgotPasswordRequest.Email)).Should().BeTrue();
        }
    }
}
