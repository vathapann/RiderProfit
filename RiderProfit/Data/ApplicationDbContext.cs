using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RiderProfit.Models;

namespace RiderProfit.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        // One DbSet per table. Vehicles holds all vehicle types (Car, EBike, Motorbike).
        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<Platform> Platforms => Set<Platform>();

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

            // Store every vehicle type in one table, with a "VehicleType" column saying which subclass it is
            builder
                .Entity<Vehicle>()
                .HasDiscriminator<string>("VehicleType")
                .HasValue<Car>("Car")
                .HasValue<EBike>("EBike")
                .HasValue<Motorbike>("Motorbike");

            builder.Entity<Shift>(shift =>
            {
                // Each shift belongs to a rider; deleting the rider deletes their shifts
                shift
                    .HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // A platform or vehicle that has shifts can't be deleted, so history isn't lost
                shift
                    .HasOne(s => s.Platform)
                    .WithMany(p => p.Shifts)
                    .HasForeignKey(s => s.PlatformId)
                    .OnDelete(DeleteBehavior.NoAction);

                shift
                    .HasOne(s => s.Vehicle)
                    .WithMany()
                    .HasForeignKey(s => s.VehicleId)
                    .OnDelete(DeleteBehavior.NoAction);

                shift.Property(s => s.Suburb).HasMaxLength(100);

                builder.Entity<Expense>(expense =>
                {
                    expense
                        .HasOne<ApplicationUser>()
                        .WithMany()
                        .HasForeignKey(e => e.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                    // Deleting a shift keeps its expenses but unlinks them (ShiftId becomes null)
                    expense
                        .HasOne(e => e.Shift)
                        .WithMany(s => s.Expenses)
                        .HasForeignKey(e => e.ShiftId)
                        .OnDelete(DeleteBehavior.SetNull);

                    expense.Property(e => e.Category).HasConversion<string>().HasMaxLength(20);
                });

                builder
                    .Entity<Vehicle>()
                    .HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Platform>(platform =>
            {
                // Riders' own platforms are deleted with them; built-in ones have no UserId
                platform
                    .HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                platform.Property(p => p.Name).HasMaxLength(50);

                // Built-in platforms shared by all riders (UserId = null)
                platform.HasData(
                    new Platform { Id = 1, Name = "Uber Eats" },
                    new Platform { Id = 2, Name = "DoorDash" }
                );
            });
        }
    }
}
