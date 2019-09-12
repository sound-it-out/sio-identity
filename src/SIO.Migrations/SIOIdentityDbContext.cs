using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SIO.Migrations
{
    public class SIOIdentityDbContext : IdentityDbContext<SIOUser>
    {
        public SIOIdentityDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
