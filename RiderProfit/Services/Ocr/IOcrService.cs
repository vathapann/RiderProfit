namespace RiderProfit.Services.Ocr;

// Reads the text in an image (OCR). Pages and importers depend on this interface,
// so the OCR provider (currently Azure AI Vision) can be swapped without changing them.
public interface IOcrService
{
    // False when the endpoint or key hasn't been set up, e.g. on a machine without user secrets
    bool IsConfigured { get; }

    // Returns the text lines found in the image, ordered from top to bottom
    Task<List<OcrLine>> ReadLinesAsync(Stream image, CancellationToken cancellationToken = default);
}
