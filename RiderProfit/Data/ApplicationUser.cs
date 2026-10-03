using Microsoft.AspNetCore.Identity;
using RiderProfit.Models;

namespace RiderProfit.Data
{
    // A delivery rider using RiderProfit. IdentityUser already provides the
    // login details (email, password hash, phone); these are the rider's profile settings.
    public class ApplicationUser : IdentityUser
    {
        // Name shown in the app, e.g. "Welcome, Tiwat"
        public string DisplayName { get; set; } = string.Empty;

        // Default location for weather forecasts in the shift planner
        public string HomeSuburb { get; set; } = string.Empty;

        // Used to look up state-specific public holidays
        public AustralianState State { get; set; } = AustralianState.NSW;

        // Optional weekly net profit target shown on the dashboard
        public decimal? WeeklyGoal { get; set; }
    }

}
