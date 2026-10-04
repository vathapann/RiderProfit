using RiderProfit.Models;

namespace RiderProfit.Services;

// Manages a rider's vehicles. Every method takes the userId so riders only see their own data.
public interface IVehicleService
{
    Task<List<Vehicle>> GetVehiclesAsync(string userId);
    Task<Vehicle?> GetVehicleAsync(int id, string userId);
    // Task AddVehicleAsync(Vehicle vehicle);
    // Task UpdateVehicleAsync(Vehicle vehicle);
    // Task<bool> DeleteVehicleAsync(int id, string userId);   // false if the vehicle has shifts
}
