namespace RiderProfit.Models;

public abstract class Vehicle
{
    public int Id { set; get; }
    public string UserId { set; get; } = string.Empty; // who own this vehicle
    public string Make { set; get; } = string.Empty;
    public string Model { set; get; } = string.Empty;
    public int Year { set; get; } = 0;
    public abstract decimal CostPerKm();
}