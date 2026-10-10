using RiderProfit.Models;

namespace RiderProfit.Services;

public interface IShiftService
{
    Task<List<Shift>> GetShiftsAsync(string userId); //get all shifts for this rider
    Task<Shift?> GetShiftAsync(int id, string userId); // get a particular shift for this rider, or null if not found

    Task AddShiftAsync(Shift shift);
    // Task<bool> DeleteShiftAsync(int id, string userId);
}
