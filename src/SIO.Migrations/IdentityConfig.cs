using System;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace SIO.Migrations
{
    internal class IdentityConfig
    {
        public IEnumerable<ApiResourceOption> ApiResources { get; set; }
        public IEnumerable<ClientOption> Clients { get; set; }
    }
}
