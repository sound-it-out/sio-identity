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
        public MockSignInManager(UserManager<SIOUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<SIOUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<SIOUser>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<SIOUser> userConfirmation) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, userConfirmation)
        {
        }

        public override Task SignInAsync(SIOUser user, bool isPersistent, string authenticationMethod = null)
        {
            return Task.CompletedTask;
        }

        public override Task SignOutAsync()
        {
            return Task.CompletedTask;
        }
    }
}
