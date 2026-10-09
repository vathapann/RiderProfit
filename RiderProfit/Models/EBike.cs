namespace RiderProfit.Models;

// An electric bike. Its running cost comes from the electricity used to charge it.
public class EBike : Vehicle
{
    // Battery size in kWh
    public decimal BatteryCapacity { get; set; }

    // Electricity used in kWh per 100 km
    public decimal EnergyConsumption { get; set; }

    // Price paid per kWh of electricity, in dollars
    public decimal ElectricityPricePerKwh { get; set; } = 0.30m;

    // Charging cost per km = kWh used per km × price per kWh
    public override decimal CostPerKm()
    {
        return EnergyConsumption / 100 * ElectricityPricePerKwh;
    }
}
