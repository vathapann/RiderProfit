using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;

namespace RiderProfit.Services;

public class ShiftService(IDbContextFactory<ApplicationDbContext> dbFactory) : IShiftService
{
    public async Task<List<Shift>> GetShiftsAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db
            .Shifts.Where(s => s.UserId == userId) // LINQ + lambda
            .Include(s => s.Platform) // load the related rows too, like Django's select_related
            .Include(s => s.Vehicle)
            .Include(s => s.Trips) // needed for the calculated Earnings, Tips and TripCount
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    // public async Task<Shift?> GetShiftAsync(string userId, int shiftId)
    // {
    //     await using var db = await dbFactory.CreateDbContextAsync();
    //     return await db.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId && s.UserId == userId);
    // }

    // Returns the shift with its trips only if it belongs to this rider, otherwise null.
    // Filtering by userId stops riders opening another rider's shift by changing the URL.
    public async Task<Shift?> GetShiftAsync(int id, string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db
            .Shifts.Include(s => s.Platform)
            .Include(s => s.Vehicle)
            .Include(s => s.Trips)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
    }

    public async Task AddShiftAsync(Shift shift)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Shifts.Add(shift);
        await db.SaveChangesAsync();
    }
}
