namespace RiderProfit.Models;
public class EBike : Vehicle
{
    public decimal BatteryCapacity {set; get;} = 0;
    public decimal EnergyConsumption {set; get;} = 0;

    public override decimal CostPerKm()
    {
        return EnergyConsumption / 100 * 0.3m;
    }
}