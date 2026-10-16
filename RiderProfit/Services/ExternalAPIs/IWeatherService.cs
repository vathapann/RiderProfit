using RiderProfit.Models;

namespace RiderProfit.Services.ExternalAPIs;

// Answers whether it was raining in a suburb at a given local time.
public interface IWeatherService
{
    // Returns null when the suburb can't be found or the weather isn't available
    Task<bool?> WasRainingAsync(
        string suburb,
        AustralianState state,
        DateTime localTime,
        CancellationToken cancellationToken = default);
}
