using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIO.Identity.Login.Responses
{
    public class LoginResponse
    {
        public string Username { get; }
        public string ReturnUrl { get; }
        public bool EnableLocalLogin { get; }
        public IEnumerable<ExternalProvider> ExternalProviders { get; }

        public LoginResponse(string username, string returnUrl, bool enableLocalLogin, IEnumerable<ExternalProvider> externalProviders)
        {
            Username = username;
            ReturnUrl = returnUrl;
            EnableLocalLogin = enableLocalLogin;
            ExternalProviders = externalProviders;
        }
    }
}
