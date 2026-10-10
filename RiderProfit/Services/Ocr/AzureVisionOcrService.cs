using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.Extensions.Options;

namespace RiderProfit.Services.Ocr;

// Reads text from images using the Azure AI Vision "Read" (OCR) feature.
public class AzureVisionOcrService : IOcrService
{
    private readonly ImageAnalysisClient? client;

    public AzureVisionOcrService(IOptions<AzureVisionOptions> options)
    {
        var settings = options.Value;

        // Only create the client when both settings exist, so the app still starts without them
        if (!string.IsNullOrWhiteSpace(settings.Endpoint) && !string.IsNullOrWhiteSpace(settings.Key))
        {
            client = new ImageAnalysisClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.Key));
        }
    }

    public bool IsConfigured => client is not null;

    public async Task<List<OcrLine>> ReadLinesAsync(Stream image, CancellationToken cancellationToken = default)
    {
        if (client is null)
        {
            throw new InvalidOperationException("Azure AI Vision is not configured. Set the AzureVision:Endpoint and AzureVision:Key user secrets.");
        }

        var imageData = await BinaryData.FromStreamAsync(image, cancellationToken);
        var result = await client.AnalyzeAsync(imageData, VisualFeatures.Read, cancellationToken: cancellationToken);

        // Azure groups text into blocks of lines. Flatten them and keep each line's top-left corner.
        return result.Value.Read.Blocks
            .SelectMany(block => block.Lines)
            .Select(line => new OcrLine(
                line.Text,
                Top: line.BoundingPolygon.Min(point => point.Y),
                Left: line.BoundingPolygon.Min(point => point.X)))
            .OrderBy(line => line.Top)
            .ThenBy(line => line.Left)
            .ToList();
    }
}
