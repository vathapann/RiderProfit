
using Microsoft.ML.Data;

namespace RiderProfit.Services;

public class TripRecord
{
    public DateTime Date {get; set; }
    public TimeSpan AcceptedTime {get; set; }
    public string OrderType {get; set;} = "";
    public string StoreName {get; set;} = "";
    public string? PickupStreet {get; set; }
    public string? RestaurantSuburb {get; set; }
    public string? DropoffStreet {get; set; }
    public string DropoffSuburb {get; set; } = "";
    public string? DropoffPostcode {get; set; }
    public int ActualDurationSec {get; set; }
    public decimal DistanceKm {get; set; }
    public decimal TotalAmount {get; set; }
    public decimal Tip {get; set; }
    public string? Notes {get; set; }
}

public class TripData
{
    public string Weekday {get; set; } = "";
    public float HourOfDay {get; set; }
    public string RestaurantSuburb {get; set;} ="";
    public float EarningsPerHour {get; set; }
}

public class TripPrediction
{
    [ColumnName("Score")]
    public float Score {get; set;}
}