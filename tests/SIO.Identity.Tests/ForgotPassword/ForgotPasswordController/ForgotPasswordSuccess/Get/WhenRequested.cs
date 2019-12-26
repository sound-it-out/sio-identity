using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace SIO.Identity.Tests.ForgotPassword.ForgotPasswordController.ForgotPasswordSuccess.Get
{
    public class WhenRequested : ForgotPasswordControllerSpecification<IActionResult>
    {
        protected override Task<IActionResult> Given()
        {
            return Task.FromResult(_controller.ForgotPasswordSuccess());
        }

        protected override Task When()
        {
            return Task.CompletedTask;
        }

        [Then]
        public void ThenShouldReturnViewResult()
        {
            Result.Should().BeOfType<ViewResult>();
        }
    }
}
