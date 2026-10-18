using System.Globalization;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;

namespace RiderProfit.Services.Export;

// Reads trips from the database and writes them as CSV with the rider_data.csv columns:
// Date,AcceptedTime,OrderType,StoreName,PickupStreet,RestaurantSuburb,DropoffStreet,DropoffSuburb,
// DropoffPostcode,ActualDurationSec,DistanceKm,TotalAmount,Tip,Notes
// plus IsRaining and IsPublicHoliday at the end.
public class TripCsvExportService(IDbContextFactory<ApplicationDbContext> dbFactory) : ITripCsvExportService
{
    public async Task<string> ExportTripsAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);

        // Trips have no UserId of their own, so a rider's trips are found through their shifts
        var trips = await db.Trips
            .AsNoTracking()
            .Where(t => userId == null || t.Shift!.UserId == userId)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);

        return ToCsv(trips);
    }

    // Public and static so the CSV format can be unit tested without a database
    public static string ToCsv(IEnumerable<Trip> trips)
    {
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(trips.Select(ToRow));
        return writer.ToString();
    }

    private static TripCsvRow ToRow(Trip trip) => new()
    {
        // Same date and time formats as rider_data.csv, e.g. 2/10/2026 and 21:21
        Date = trip.StartTime.ToString("d/MM/yyyy", CultureInfo.InvariantCulture),
        AcceptedTime = trip.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
        OrderType = "DELIVERY",
        StoreName = trip.PickupName,
        PickupStreet = string.Empty,
        RestaurantSuburb = trip.PickupSuburb,
        DropoffStreet = trip.DropoffName,
        DropoffSuburb = trip.DropoffSuburb,
        DropoffPostcode = string.Empty, // not stored in the app
        ActualDurationSec = (int)Math.Max(0, (trip.EndTime - trip.StartTime).TotalSeconds),
        DistanceKm = trip.DistanceKm,
        // rider_data.csv's TotalAmount includes tips: imported trips already hold the total in Earnings,
        // manual trips record tips separately, so both are added
        TotalAmount = trip.Earnings + trip.Tips,
        Tip = trip.Tips,
        Notes = $"shift {trip.ShiftId}, trip {trip.Id}",
        IsRaining = trip.IsRaining,
        IsPublicHoliday = trip.IsPublicHoliday,
    };

    // One CSV line. Property names become the column headers, in this order.
    private sealed class TripCsvRow
    {
        public string Date { get; init; } = string.Empty;
        public string AcceptedTime { get; init; } = string.Empty;
        public string OrderType { get; init; } = string.Empty;
        public string StoreName { get; init; } = string.Empty;
        public string PickupStreet { get; init; } = string.Empty;
        public string RestaurantSuburb { get; init; } = string.Empty;
        public string DropoffStreet { get; init; } = string.Empty;
        public string DropoffSuburb { get; init; } = string.Empty;
        public string DropoffPostcode { get; init; } = string.Empty;
        public int ActualDurationSec { get; init; }
        public decimal DistanceKm { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal Tip { get; init; }
        public string Notes { get; init; } = string.Empty;
        public bool IsRaining { get; init; }
        public bool IsPublicHoliday { get; init; }
    }
}
