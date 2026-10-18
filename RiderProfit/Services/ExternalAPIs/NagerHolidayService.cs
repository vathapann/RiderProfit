using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using RiderProfit.Models;

namespace RiderProfit.Services.ExternalAPIs;

// Public holidays from the free Nager.Date API (https://date.nager.at), no key needed.
// Each year's list is fetched once and cached, since holidays don't change.
public class NagerHolidayService(HttpClient http, IMemoryCache cache, ILogger<NagerHolidayService> logger)
    : IHolidayService
{
    public async Task<bool> IsPublicHolidayAsync(
        DateOnly date,
        AustralianState state,
        CancellationToken cancellationToken = default)
    {
        var holidays = await GetHolidaysAsync(date.Year, cancellationToken);
        return holidays.Any(h => h.Date == date && AppliesToState(h, state));
    }

    // A holiday applies when it's national (Global) or lists the state, e.g. "AU-NSW"
    public static bool AppliesToState(NagerHoliday holiday, AustralianState state) =>
        holiday.Global || (holiday.Counties?.Contains($"AU-{state}") ?? false);

    private async Task<List<NagerHoliday>> GetHolidaysAsync(int year, CancellationToken cancellationToken)
    {
        var cacheKey = $"holidays-AU-{year}";
        if (cache.TryGetValue(cacheKey, out List<NagerHoliday>? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var holidays = await http.GetFromJsonAsync<List<NagerHoliday>>(
                $"api/v3/PublicHolidays/{year}/AU", cancellationToken) ?? [];

            cache.Set(cacheKey, holidays, TimeSpan.FromDays(1));
            return holidays;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            // Without holiday data, trips are saved as "not a holiday" rather than failing
            logger.LogWarning(ex, "Couldn't load public holidays for {Year}", year);
            return [];
        }
    }
}

// One holiday as returned by Nager.Date
public record NagerHoliday(
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("localName")] string LocalName,
    [property: JsonPropertyName("global")] bool Global,
    [property: JsonPropertyName("counties")] List<string>? Counties);
