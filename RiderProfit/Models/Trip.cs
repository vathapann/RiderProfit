namespace RiderProfit.Models;

// One delivery within a shift. Created manually or imported from an Uber Eats screenshot.

public class Trip
{
    public int Id { set; get; }
    public int ShiftId { get; set; }
    public Shift? Shift { get; set; }

    public decimal Earnings { set; get; } = 0;
    public decimal DistanceKm { set; get; } = 0;
    public decimal Tips { set; get; } = 0;
    public DateTime StartTime { set; get; } = DateTime.MinValue;
    public DateTime EndTime { set; get; } = DateTime.MinValue;
    public string PickupName { get; set; } = string.Empty;
    public string DropoffName { set; get; } = string.Empty;
    public string PickupSuburb { set; get; } = string.Empty; // check with 3rd parties API
    public string DropoffSuburb { set; get; } = string.Empty; // check with 3rd parties API
    public bool IsRaining { get; set; } = false; // check with 3rd parties API (Open-Meteo's historical API (archive-api.open-meteo.com)
    public bool IsPublicHoliday { get; set; } = false; // check with 3rd parties API
    public TimeSpan Duration => EndTime - StartTime; // may useful for dashboard
}
