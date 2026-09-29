using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RiderProfit.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity tables must be configured first
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(user =>
            {
                user.Property(u => u.DisplayName).HasMaxLength(100);
                user.Property(u => u.HomeSuburb).HasMaxLength(100);

                // Store the state as text ("NSW") rather than a number so the database is readable
                user.Property(u => u.State).HasConversion<string>().HasMaxLength(3);
            });
        }
    }
}
