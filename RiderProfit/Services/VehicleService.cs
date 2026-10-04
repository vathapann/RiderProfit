using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;

namespace RiderProfit.Services;

public class VehicleService(IDbContextFactory<ApplicationDbContext> dbFactory) : IVehicleService
{
    public async Task<List<Vehicle>> GetVehiclesAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Vehicles
            .Where(v => v.UserId == userId)      // LINQ + lambda
            .OrderBy(v => v.Make)
            .ToListAsync();
    }

    // Returns the vehicle only if it belongs to this rider, otherwise null
    public async Task<Vehicle?> GetVehicleAsync(int id, string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
    }
    // TODO (Sovatha): implement these. Placeholders so the project builds; they throw if called.
    public Task AddVehicleAsync(Vehicle vehicle) => throw new NotImplementedException();

    public Task UpdateVehicleAsync(Vehicle vehicle) => throw new NotImplementedException();

    public Task<bool> DeleteVehicleAsync(int id, string userId) => throw new NotImplementedException();
}
