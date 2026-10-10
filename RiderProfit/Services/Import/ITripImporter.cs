using RiderProfit.Models;

namespace RiderProfit.Services.Import;

// Reads trips from a platform's earnings screenshot. Each platform has its own layout,
// so supporting a new one (e.g. DoorDash) means adding a new implementation, not changing this one.
public interface ITripImporter
{
    string PlatformName { get; }

    // False when the OCR service isn't configured on this machine
    bool IsAvailable { get; }

    // Returns the trips found in the screenshot, dated using the shift ( not saved yet to database yet)
    Task<List<Trip>> ImportAsync(Stream screenshot, Shift shift, CancellationToken cancellationToken = default);
}
