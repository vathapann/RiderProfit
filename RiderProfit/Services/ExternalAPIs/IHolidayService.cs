using RiderProfit.Models;

namespace RiderProfit.Services.ExternalAPIs;

// Answers whether a date is a public holiday in a given Australian state.
public interface IHolidayService
{
    Task<bool> IsPublicHolidayAsync(DateOnly date, AustralianState state, CancellationToken cancellationToken = default);
}
