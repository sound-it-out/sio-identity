using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SIO.Identity.Verify;
using SIO.Identity.Verify.Requests;
using SIO.Migrations;
using Xunit;

namespace SIO.Identity.Tests.Verify
{
    public class VerifyControllerTests : ControllerTest<VerifyController>
    {
        [Fact]
        public void Verify_GET_Should_Error_When_Email_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var result = controller.Verify(null, "dG9rZW4=");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Verify_GET_Should_Error_When_Email_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);

            var result = controller.Verify("", "dG9rZW4=");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Verify_GET_Should_Error_When_Token_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);

            var result = controller.Verify("mock@sound-it-out.com", null);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Verify_GET_Should_Error_When_Token_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);

            var result = controller.Verify("mock@sound-it-out.com", "");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Verify_GET_Should_Return_View_When_Email_And_Token_Are_Valid()
        {
            var controller = BuildController(out var serviceProvider);

            var result = controller.Verify("mock@sound-it-out.com", "dG9rZW4=");
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeTrue();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Email_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = null,
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Email_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "",
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Email_Is_Invalid()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "invalid",
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Token_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = null,
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Token_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Password_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = null,
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Password_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = "",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_RePassword_Is_Null()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = null
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_RePassword_Is_Empty()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = ""
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Password_And_RePassword_Do_Not_Match()
        {
            var controller = BuildController(out var serviceProvider);
            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = "password",
                RePassword = "repassword"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Redirect_To_Default_App_Url_When_User_Is_Authenticated()
        {
            var controller = BuildController(out var serviceProvider);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, "example name"),
                        new Claim(ClaimTypes.NameIdentifier, "1"),
                        new Claim("custom-claim", "example claim value"),
                    }, "mock"))
                }
            };

            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Be("DefaultAppUrl");
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_User_Does_Not_Exist()
        {
            var controller = BuildController(out var serviceProvider);

            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.SetupGet(mu => mu.Identity.IsAuthenticated).Returns(false);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = mockUser.Object
                }
            };

            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
            };
            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Unable_To_Confirm_User_Email()
        {
            var controller = BuildController(out var serviceProvider);

            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.SetupGet(mu => mu.Identity.IsAuthenticated).Returns(false);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = mockUser.Object
                }
            };

            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Token = "token",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
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

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Error_When_Unable_To_Add_Password_For_User()
        {
            var controller = BuildController(out var serviceProvider);

            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.SetupGet(mu => mu.Identity.IsAuthenticated).Returns(false);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = mockUser.Object
                }
            };

            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Password = "asdf",
                RePassword = "asdf"
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
            request.Token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Verify_POST_Should_Redirect_To_Default_App_Url_When_Verify_Succeeds()
        {
            var controller = BuildController(out var serviceProvider);

            var mockUser = new Mock<ClaimsPrincipal>();
            mockUser.SetupGet(mu => mu.Identity.IsAuthenticated).Returns(false);

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = mockUser.Object
                }
            };

            var request = new VerifyRequest
            {
                Email = "mock@sound-it-out.com",
                Password = "Asdf@123456789asdf",
                RePassword = "Asdf@123456789asdf"
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
            request.Token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            controller.ValidateRequest(request);

            var result = await controller.Verify(request);
            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Be("DefaultAppUrl");
            controller.ModelState.IsValid.Should().BeTrue();
        }
    }
}
