namespace RiderProfit.Models;

public class Shift
{
    public int Id {set; get;}
    public string UserId { get; set; } = string.Empty;
    public int PlatformId { get; set; }
    public string Suburb {set; get;} = string.Empty;
    public DateTime StartTime {set; get;} = DateTime.MinValue;
    public DateTime EndTime {set; get;} = DateTime.MinValue;

    public decimal Earnings {set; get;} = 0;
    public decimal Tips {set; get;} = 0;

    public Platform? Platform {set; get;} = null;
    public int VehicleId {set; get;} = 0;
    public Vehicle? Vehicle {set; get;} = null;

    public List<Expense> Expenses {set; get;} = new();

    public decimal DistanceKm {set; get;} = 0;
}