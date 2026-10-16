


using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

var path = Path.Combine(AppContext.BaseDirectory, "rider_data.csv");
var config = new CsvConfiguration(CultureInfo.GetCultureInfo("en-AU"));

using var reader = new StreamReader(path);
using var csv = new CsvReader(reader, config);
var trips = csv.GetRecords<TripRecord>().ToList();

foreach(var t in trips)
{
    double hours = t.ActualDurationSec / 3600.0;
    double perHour = (double)t.TotalAmount / hours;
    Console.WriteLine($"{t.Date:d MMM}{t.AcceptedTime:hh\\:mm}{t.DropoffSuburb, -12} ${perHour:F2}/h");
}
Console.WriteLine($"Loaded{trips.Count} trips");



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


