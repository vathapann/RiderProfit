using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using RiderProfit.Models;

namespace RiderProfit.Services.ExternalAPIs;

// Historical weather from the free Open-Meteo APIs (https://open-meteo.com), no key needed:
//  1. Geocoding API turns a suburb name into latitude/longitude
//  2. Weather API returns hourly rainfall for that place and day, in local time
// The archive API has past weather up to about a day ago; for the most recent hours the forecast API is used instead.
public class OpenMeteoWeatherService(HttpClient http, IMemoryCache cache, ILogger<OpenMeteoWeatherService> logger)
    : IWeatherService
{
    // Rainfall in one hour at or above this many millimetres counts as raining
    public const double RainThresholdMm = 0.2;

    private const string ArchiveApi = "https://archive-api.open-meteo.com/v1/archive";
    private const string ForecastApi = "https://api.open-meteo.com/v1/forecast";

    public async Task<bool?> WasRainingAsync(
        string suburb,
        AustralianState state,
        DateTime localTime,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(suburb))
        {
            return null;
        }

        try
        {
            var location = await FindSuburbAsync(suburb.Trim(), state, cancellationToken);
            if (location is null)
            {
                return null;
            }

            var date = DateOnly.FromDateTime(localTime);

            // Archive first; it has no value yet for the last few hours, so then try the forecast API
            var archive = await GetHourlyRainAsync(ArchiveApi, location, date, cancellationToken);
            var raining = archive is null ? null : IsRainingAt(archive, localTime);
            if (raining is not null)
            {
                return raining;
            }

            var forecast = await GetHourlyRainAsync(ForecastApi, location, date, cancellationToken);
            return forecast is null ? null : IsRainingAt(forecast, localTime);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            // Without weather data the trip is still saved; IsRaining just stays false
            logger.LogWarning(ex, "Couldn't get weather for {Suburb} at {Time}", suburb, localTime);
            return null;
        }
    }

    // Finds the hour containing the given time and checks its rainfall.
    // Public and static so it can be unit tested without calling the API.
    public static bool? IsRainingAt(HourlyRain hourly, DateTime localTime)
    {
        var hourText = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, 0, 0)
            .ToString("yyyy-MM-dd'T'HH:mm", CultureInfo.InvariantCulture);

        var index = hourly.Time.IndexOf(hourText);
        if (index < 0 || index >= hourly.Precipitation.Count || hourly.Precipitation[index] is null)
        {
            return null;
        }

        return hourly.Precipitation[index] >= RainThresholdMm;
    }

    private async Task<GeoLocation?> FindSuburbAsync(
        string suburb,
        AustralianState state,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"geo-{state}-{suburb.ToLowerInvariant()}";
        if (cache.TryGetValue(cacheKey, out GeoLocation? cached))
        {
            return cached;
        }

        var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(suburb)}&count=10&country_code=AU";
        var response = await http.GetFromJsonAsync<GeocodingResponse>(url, cancellationToken);

        // Prefer an exact name match in the rider's state, e.g. "Miranda" in New South Wales
        var stateName = StateNames[state];
        var match = response?.Results?
            .Where(r => r.Admin1 == stateName)
            .OrderByDescending(r => string.Equals(r.Name, suburb, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        cache.Set(cacheKey, match, TimeSpan.FromDays(7));
        return match;
    }

    private async Task<HourlyRain?> GetHourlyRainAsync(
        string api,
        GeoLocation location,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var lon = location.Longitude.ToString(CultureInfo.InvariantCulture);
        var day = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var cacheKey = $"rain-{api}-{lat},{lon}-{day}";
        if (cache.TryGetValue(cacheKey, out HourlyRain? cached))
        {
            return cached;
        }

        var url = $"{api}?latitude={lat}&longitude={lon}&hourly=precipitation&start_date={day}&end_date={day}&timezone=auto";

        var response = await http.GetFromJsonAsync<WeatherResponse>(url, cancellationToken);
        var hourly = response?.Hourly;

        cache.Set(cacheKey, hourly, TimeSpan.FromHours(6));
        return hourly;
    }

    // Open-Meteo's geocoding reports the state's full name in "admin1"
    private static readonly Dictionary<AustralianState, string> StateNames = new()
    {
        [AustralianState.NSW] = "New South Wales",
        [AustralianState.VIC] = "Victoria",
        [AustralianState.QLD] = "Queensland",
        [AustralianState.SA] = "South Australia",
        [AustralianState.WA] = "Western Australia",
        [AustralianState.TAS] = "Tasmania",
        [AustralianState.NT] = "Northern Territory",
        [AustralianState.ACT] = "Australian Capital Territory",
    };

    private record GeocodingResponse([property: JsonPropertyName("results")] List<GeoLocation>? Results);

    private record WeatherResponse([property: JsonPropertyName("hourly")] HourlyRain? Hourly);
}

// A place found by Open-Meteo's geocoding API
public record GeoLocation(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("admin1")] string? Admin1);

// Hourly rainfall for one day: Time[i] (local, e.g. "2026-10-09T21:00") goes with Precipitation[i] in mm
public record HourlyRain(
    [property: JsonPropertyName("time")] List<string> Time,
    [property: JsonPropertyName("precipitation")] List<double?> Precipitation);
