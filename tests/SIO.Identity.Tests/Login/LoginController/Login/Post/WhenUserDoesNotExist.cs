using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SIO.Identity.Login.Requests;

namespace SIO.Identity.Tests.Login.LoginController.Login.Post
{
    public class WhenUserDoesNotExist : LoginControllerSpecification<IActionResult>
    {
        private readonly string _buttonText = "login";
        private LoginRequest _request;

        protected override Task<IActionResult> Given()
        {
            return _controller.Login(_request, _buttonText);
        }

        protected override Task When()
        {
            _request = new LoginRequest { Email = "user@mock.test", Password = "password" };
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
