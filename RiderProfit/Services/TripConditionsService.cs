using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;
using RiderProfit.Services.ExternalAPIs;

namespace RiderProfit.Services;

// Sets IsPublicHoliday and IsRaining on trips using the holiday and weather services.
// If an API is unavailable the trip keeps false for that field and is still saved.
public class TripConditionsService(
    IDbContextFactory<ApplicationDbContext> dbFactory,
    IHolidayService holidayService,
    IWeatherService weatherService) : ITripConditionsService
{
    public async Task ApplyAsync(IEnumerable<Trip> trips, string userId, CancellationToken cancellationToken = default)
    {
        var state = await GetRiderStateAsync(userId, cancellationToken);

        foreach (var trip in trips)
        {
            trip.IsPublicHoliday = await holidayService.IsPublicHolidayAsync(
                DateOnly.FromDateTime(trip.StartTime), state, cancellationToken);

            // Weather where the trip started; fall back to the drop-off if the pickup suburb is unknown
            var suburb = string.IsNullOrWhiteSpace(trip.PickupSuburb) ? trip.DropoffSuburb : trip.PickupSuburb;
            trip.IsRaining = await weatherService.WasRainingAsync(suburb, state, trip.StartTime, cancellationToken) ?? false;
        }
    }

    // Public holidays and suburb names depend on the rider's state (set on their profile, NSW by default)
    private async Task<AustralianState> GetRiderStateAsync(string userId, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var state = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => (AustralianState?)u.State)
            .FirstOrDefaultAsync(cancellationToken);

        return state ?? AustralianState.NSW;
    }
}
