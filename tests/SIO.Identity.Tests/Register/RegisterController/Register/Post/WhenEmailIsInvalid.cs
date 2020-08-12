using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SIO.Identity.Register.Requests;

namespace SIO.Identity.Tests.Register.RegisterController.Register.Post
{
    public class WhenEmailIsInvalid : RegisterControllerSpecification<IActionResult>
    {
        private RegisterRequest _request;

        protected override Task<IActionResult> Given()
        {
            return _controller.Register(_request);
        }

        protected override Task When()
        {
            _request = new RegisterRequest { Email = "invalid" };
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
            _controller.ModelState.ContainsKey(nameof(RegisterRequest.Email)).Should().BeTrue();
        }
    }
}
