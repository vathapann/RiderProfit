namespace RiderProfit.Models;

public class Trip
{
    public int Id { set; get; }
    public int ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public string Suburb { set; get; } = string.Empty;
    public decimal Earnings { set; get; } = 0;
    public decimal DistanceKm { set; get; } = 0;
    public decimal Tips { set; get; } = 0;
    public DateTime StartTime { set; get; } = DateTime.MinValue;
    public DateTime EndTime { set; get; } = DateTime.MinValue;
    public string PickupName { get; set; } = string.Empty;
}
