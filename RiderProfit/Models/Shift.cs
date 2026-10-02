namespace RiderProfit.Models;

public class Shift
{
    public int Id {set; get;}
    public int UserId {set; get;}
    public int PlatformId { get; set; }
    public string Suburb {set; get;} = string.Empty;
    public string StartTime {set; get;} = string.Empty;
    public string EndTime {set; get;} = string.Empty;

    public decimal Distance {set; get;} = 0;
}