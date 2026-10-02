namespace RiderProfit.Models;
public class Car: Vehicle 
{
    public decimal EngineSize {set; get;} = 0;
    public decimal FuelConsumption {set; get;} = 0;

    public override decimal CostPerKm()
    {
        return FuelConsumption / 100 * 2.5m;
    }
}