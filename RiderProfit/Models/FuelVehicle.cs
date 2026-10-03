namespace RiderProfit.Models;

// Base class for petrol vehicles (cars and motorbikes)
public abstract class FuelVehicle : Vehicle
{
    // Engine size in litres, e.g. 1.5
    public decimal EngineSize { get; set; }

    // Fuel used in litres per 100 km
    public decimal FuelConsumption { get; set; }

    // Price paid per litre of fuel, in dollars
    public decimal FuelPricePerLitre { get; set; } = 2.00m;

    // Fuel cost per km = litres used per km × price per litre
    public override decimal CostPerKm()
    {
        return FuelConsumption / 100 * FuelPricePerLitre;
    }
}
