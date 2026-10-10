using RiderProfit.Models;

namespace RiderProfit.Services;

public interface ITripService
{
    Task<List<Trip>> GetTripsAsync(int shiftId, string userId); // get all trips for this rider's shift
    // Task<Trip?> GetTripAsync(int id, string userId); // get a particular trip for this rider, or null if not found

    Task AddTripAsync(Trip trip);
    Task AddTripsAsync(IEnumerable<Trip> trips); // saves several trips at once, e.g. from a screenshot
    // Task<bool> DeleteTripAsync(int id, string userId);
}
