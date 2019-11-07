using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIO.Migrations;

namespace SIO.Identity.Tests
{
    internal class MockUserManager : UserManager<SIOUser>
    {
        internal bool CreateUserSucceeds { get; set; }
        internal bool InterceptCreateUser { get; set; }

        internal bool AddPasswordSucceeds { get; set; }
        internal bool InterceptAddPassword { get; set; }

        internal bool ConfirmEmailSucceeds { get; set; }
        internal bool InterceptConfirmEmail { get; set; }

        public MockUserManager(IUserStore<SIOUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<SIOUser> passwordHasher, IEnumerable<IUserValidator<SIOUser>> userValidators, IEnumerable<IPasswordValidator<SIOUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<SIOUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<IdentityResult> CreateAsync(SIOUser user)
        {
            return InterceptCreateUser ? Task.FromResult((IdentityResult)new MockIdentityResult(CreateUserSucceeds)) : base.CreateAsync(user);
        }

        public override Task<IdentityResult> AddPasswordAsync(SIOUser user, string password)
        {
            return InterceptAddPassword ? Task.FromResult((IdentityResult)new MockIdentityResult(AddPasswordSucceeds)) : base.AddPasswordAsync(user, password);
        }

        public override Task<IdentityResult> ConfirmEmailAsync(SIOUser user, string token)
        {
            return InterceptConfirmEmail ? Task.FromResult((IdentityResult)new MockIdentityResult(ConfirmEmailSucceeds)) : base.ConfirmEmailAsync(user, token);
        }
    }
}
