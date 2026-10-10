namespace RiderProfit.Services.Ocr;

// Reads the text in an image (OCR). Pages and importers depend on this interface,
// so the OCR provider (currently Azure AI Vision) can be swapped without changing them.
public interface IOcrService
{
    // False when the endpoint or key hasn't been set up
    bool IsConfigured { get; }

    // Returns the text lines found in the image, ordered from top to bottom
    // Refer to its docs: https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/quickstarts-sdk/image-analysis-client-library?tabs=windows%2Cvisual-studio&pivots=programming-language-csharp
    Task<List<OcrLine>> ReadLinesAsync(Stream image, CancellationToken cancellationToken = default);
}
