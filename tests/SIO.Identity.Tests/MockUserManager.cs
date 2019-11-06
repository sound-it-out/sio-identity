using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SIO.Migrations;

namespace SIO.Identity.Tests
{
    internal class MockUserManager : UserManager<SIOUser>
    {
        internal bool CreateUserSucceeds { get; set; }
        internal bool InterceptCreateUser { get; set; }

        public MockUserManager(IUserStore<SIOUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<SIOUser> passwordHasher, IEnumerable<IUserValidator<SIOUser>> userValidators, IEnumerable<IPasswordValidator<SIOUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<SIOUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override Task<IdentityResult> CreateAsync(SIOUser user)
        {
            return InterceptCreateUser ? Task.FromResult((IdentityResult)new MockIdentityResult(CreateUserSucceeds)) : base.CreateAsync(user);
        }
    }
}
