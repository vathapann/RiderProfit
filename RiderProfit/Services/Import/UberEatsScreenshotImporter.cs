using RiderProfit.Models;
using RiderProfit.Services.Ocr;

namespace RiderProfit.Services.Import;

// Imports trips from an Uber Eats "Earnings activity" screenshot: OCR reads the text,
// then the parser turns it into trips.
public class UberEatsScreenshotImporter(IOcrService ocrService, UberEatsScreenshotParser parser) : ITripImporter
{
    public string PlatformName => "Uber Eats";

    public bool IsAvailable => ocrService.IsConfigured;

    public async Task<List<Trip>> ImportAsync(Stream screenshot, Shift shift, CancellationToken cancellationToken = default)
    {
        var lines = await ocrService.ReadLinesAsync(screenshot, cancellationToken);
        var trips = parser.Parse(lines, shift.StartTime, shift.EndTime);

        foreach (var trip in trips)
        {
            trip.ShiftId = shift.Id;
        }

        return trips;
    }
}
