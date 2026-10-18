namespace RiderProfit.Services.Export;

// Exports trips saved in the app as CSV text, using the same columns as MlData/rider_data.csv
// so the file can be opened in Excel or used for analysis.
public interface ITripCsvExportService
{
    // All trips when userId is null; otherwise only that rider's trips. Oldest first.
    Task<string> ExportTripsAsync(string? userId = null, CancellationToken cancellationToken = default);
}
