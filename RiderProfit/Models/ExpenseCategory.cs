namespace RiderProfit.Models;

// Types of expense a rider can record.
// Fuel and charging are not listed: they are already counted through the vehicle's
// CostPerKm() for each shift, so logging them here as well would count them twice.
public enum ExpenseCategory
{
    Tolls,
    Parking,
    Maintenance,
    Insurance,
    Registration,
    Phone,
    Equipment,
    Other
}
