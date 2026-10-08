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

    public async Task AddShiftAsync(Shift shift)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        db.Shifts.Add(shift);
        await db.SaveChangesAsync();
    }
}
