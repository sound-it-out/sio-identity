using System.Collections.Generic;
using IdentityServer4;

namespace SIO.Migrations
{
    internal class ClientOption
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public bool RequirePkce { get; set; }
        public bool RequireClientSecret { get; set; }
        public bool RequiresConsent { get; set; }
        public ICollection<string> AllowedGrantTypes { get; set; }
        public bool AllowAccessTokensViaBrowser { get; set; } = true;
        public bool AllowOfflineAccess { get; set; } = false;
        public ICollection<string> ClientSecrets { get; set; }
        public ICollection<string> RedirectUris { get; set; }
        public ICollection<string> PostLogoutRedirectUris { get; set; }
        public ICollection<string> AllowedCorsOrigins { get; set; }
        public ICollection<string> AllowedScopes { get; set; } = new[]
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            "api"
        };

        public ClientOption()
        {
            AllowedGrantTypes = new List<string>();
            ClientSecrets = new List<string>();
            RedirectUris = new List<string>();
            PostLogoutRedirectUris = new List<string>();
            AllowedCorsOrigins = new List<string>();
        }
    }
}
