using RiderProfit.Models;

namespace RiderProfit.Services;

// Fills in the conditions a trip happened in (rain, public holiday) before it's saved.
// These are features for the shift planner's prediction model.
public interface ITripConditionsService
{
    Task ApplyAsync(IEnumerable<Trip> trips, string userId, CancellationToken cancellationToken = default);
}
