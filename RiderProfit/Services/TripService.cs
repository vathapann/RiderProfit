using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;

namespace RiderProfit.Services;

public class TripService(IDbContextFactory<ApplicationDbContext> dbFactory) : ITripService
{
    // Trips have no UserId of their own, so ownership is checked through their shift
    public async Task<List<Trip>> GetTripsAsync(int shiftId, string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db
            .Trips.Where(t => t.ShiftId == shiftId && t.Shift!.UserId == userId) // LINQ + lambda
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task AddTripAsync(Trip trip)
    {
        await AddTripsAsync([trip]);
    }

    // One SaveChanges for all trips, so either every trip is saved or none are
    public async Task AddTripsAsync(IEnumerable<Trip> trips)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Trips.AddRange(trips);
        await db.SaveChangesAsync();
    }
}
