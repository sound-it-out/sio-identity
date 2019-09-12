using System;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace SIO.Migrations
{
    public class IdentityConfig
    {
        public IEnumerable<ApiResource> ApiResources { get; set; }
        public IEnumerable<Client> Clients { get; set; }
    }
}
