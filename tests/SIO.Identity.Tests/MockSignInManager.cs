using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIO.Migrations;

namespace SIO.Identity.Tests
{
    public class MockSignInManager : SignInManager<SIOUser>
    {
        public MockSignInManager(UserManager<SIOUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<SIOUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<SIOUser>> logger, IAuthenticationSchemeProvider schemes) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
        {
        }

        public override Task SignInAsync(SIOUser user, bool isPersistent, string authenticationMethod = null)
        {
            return Task.CompletedTask;
        }
    }
}
