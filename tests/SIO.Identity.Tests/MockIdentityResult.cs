using Microsoft.AspNetCore.Identity;

namespace SIO.Identity.Tests
{
    internal class MockIdentityResult : IdentityResult
    {
        public MockIdentityResult(bool success)
        {
            Succeeded = success;
        }
    }
}
