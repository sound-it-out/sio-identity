using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;

namespace SIO.Identity
{
    internal static class ClaimsPrincipleExtensions
    {
        public static Guid? UserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var claim = principal.Claims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);

            if (claim != null && claim.Value != null)
                return new Guid(claim.Value);

            return null;
        }
    }
}
