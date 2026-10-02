namespace RiderProfit.Models;
public abstract class Vehicle
{
    public int Id {set; get;}
    public string UserId {set; get;} = string.Empty;
    public string Make {set; get;} = string.Empty;

    public abstract decimal CostPerKm();
}