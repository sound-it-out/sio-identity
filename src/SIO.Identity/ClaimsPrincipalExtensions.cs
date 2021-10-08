using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;

namespace SIO.Identity
{
    internal static class ClaimsPrincipleExtensions
    {
        public static string Subject(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var claim = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);

            if (claim == null)
                throw new InvalidOperationException("sub claim is missing");

            return claim.Value;
        }
    }
}
