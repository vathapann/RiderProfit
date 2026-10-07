namespace RiderProfit.Models;

public class Shift
{
    public int Id { set; get; }
    public string UserId { get; set; } = string.Empty;
    public int PlatformId { get; set; }

    public DateTime StartTime { set; get; } = DateTime.MinValue;
    public DateTime EndTime { set; get; } = DateTime.MinValue;

    public decimal TotalDistanceKm { set; get; } = 0;

    public Platform? Platform { set; get; } = null;
    public int VehicleId { set; get; } = 0;
    public Vehicle? Vehicle { set; get; } = null;

    public List<Expense> Expenses { set; get; } = new();

    // auto calculate earning from trips under this shift
    public decimal Earnings => Trips.Sum(t => t.Earnings);
    public decimal Tips => Trips.Sum(t => t.Tips);
    public int TripCount => Trips.Count;

    // Working time includes waiting between deliveries
    public TimeSpan Duration => EndTime - StartTime;
    public List<Trip> Trips { set; get; } = new();
}
