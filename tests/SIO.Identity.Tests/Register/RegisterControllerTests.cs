using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SIO.Identity.Register.Requests;
using SIO.Migrations;
using Xunit;

namespace SIO.Identity.Tests.Register
{
    public class RegisterControllerTests : ControllerTests<Identity.Register.RegisterController>
    {
        [Fact]
        public async Task Register_POST_Errors_When_FirstName_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = null,
                LastName = "LastName"
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_FirstName_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = "",
                LastName = "LastName"
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_LastName_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = "FirstName",
                LastName = null
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Errors_When_lastName_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "Invalid",
                FirstName = "FirstName",
                LastName = ""
            };
            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Returns_View_When_Request_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var result = await controller.Register(null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Register_POST_Should_Error_When_User_Is_Verified()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = "firstname",
                LastName = "lastname",
                UserName = request.Email,
            };

            await userManager.CreateAsync(user);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await userManager.ConfirmEmailAsync(user, token);

            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Should_Error_When_User_Is_Not_Verified()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            var userManager = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            var user = new SIOUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                FirstName = "firstname",
                LastName = "lastname",
                UserName = request.Email,
            };

            await userManager.CreateAsync(user);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Register_POST_Should_Redirect_To_Registered_If_Registration_Successful()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(Identity.Register.RegisterController.Registered));
            controller.ModelState.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Register_POST_Should_Return_Error_When_User_Creation_Fails()
        {
            var controller = BuildController(out var serviceProvider);
            var userManger = serviceProvider.GetRequiredService<UserManager<SIOUser>>();
            ((MockUserManager)userManger).InterceptCreateUser = true;

            var request = new RegisterRequest
            {
                Email = "mock@sound-it-out.com",
                FirstName = "FirstName",
                LastName = "LastName"
            };

            controller.ValidateRequest(request);

            var result = await controller.Register(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }
    }
}
