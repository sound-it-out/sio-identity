using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SIO.Identity.ForgotPassword;
using SIO.Identity.ForgotPassword.Requests;
using SIO.Migrations;
using Xunit;

namespace SIO.Identity.Tests.ForgotPassword
{
    public class ForgotPasswordControllerTests : ControllerTest<ForgotPasswordController>
    {
        [Theory]
        [InlineData("userManager")]
        public void Initialising_Controller_With_Null_Param_Should_Error(string param)
        {
            Action createdController = () => new ForgotPasswordController(null, null);
            createdController.Should().Throw<ArgumentNullException>()
                .WithMessage($@"Value cannot be null.
Parameter name: {param}");
        }

        [Fact]
        public void ForgotPassword_GET_Should_Return_View()
        {
            var result = BuildController(out var serviceProvider).ForgotPassword();
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void ForgotPasswordSuccess_GET_Should_Return_View()
        {
            var result = BuildController(out var serviceProvider).ForgotPasswordSuccess();
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task ForgotPassword_POST_Should_Error_If_Email_Is_Null()
        {
            var request = new ForgotPasswordRequest { Email = null };
            var controller = BuildController(out var serviceProvider);

            controller.ValidateRequest(request);

            var result = await controller.ForgotPassword(request);

            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ForgotPassword_POST_Should_Error_If_Email_Is_Empty()
        {
            var request = new ForgotPasswordRequest { Email = "" };
            var controller = BuildController(out var serviceProvider);

            controller.ValidateRequest(request);

            var result = await controller.ForgotPassword(request);

            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ForgotPassword_POST_Should_Error_If_Email_Is_Invalid()
        {
            var request = new ForgotPasswordRequest { Email = "invalid" };
            var controller = BuildController(out var serviceProvider);

            controller.ValidateRequest(request);

            var result = await controller.ForgotPassword(request);

            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ForgotPassword_POST_Should_Error_If_User_Doesnt_Exist()
        {
            var request = new ForgotPasswordRequest { Email = "test@test.com" };
            var controller = BuildController(out var serviceProvider);

            controller.ValidateRequest(request);

            var result = await controller.ForgotPassword(request);

            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ForgotPassword_POST_Should_Error_If_User_Exists_But_Needs_To_Verify()
        {
            var request = new ForgotPasswordRequest { Email = "test@test.com" };
            var controller = BuildController(out var serviceProvider);
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

            var result = await controller.ForgotPassword(request);

            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ForgotPassword_POST_Should_Error_If_User_Exists_But_Is_Archived()
        {
            var request = new ForgotPasswordRequest { Email = "test@test.com" };
            var controller = BuildController(out var serviceProvider);
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
            user.IsArchived = true;
            await userManager.UpdateAsync(user);

            controller.ValidateRequest(request);

            var result = await controller.ForgotPassword(request);

            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task ForgotPassword_POST_Should_Succeed_If_User_Is_Valid()
        {
            var request = new ForgotPasswordRequest { Email = "test@test.com" };
            var controller = BuildController(out var serviceProvider);
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

            var result = await controller.ForgotPassword(request);

            result.Should().NotBeNull();
            result.Should().BeOfType<RedirectToActionResult>();
            controller.ModelState.IsValid.Should().BeTrue();
        }
    }
}
