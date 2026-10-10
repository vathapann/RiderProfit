namespace RiderProfit.Services.Ocr;

// Settings for Azure AI Vision, read from the "AzureVision" configuration section.
// Store them with user secrets, never in appsettings.json:
//   dotnet user-secrets set "AzureVision:Endpoint" "<endpoint>" --project RiderProfit
//   dotnet user-secrets set "AzureVision:Key" "<key>" --project RiderProfit
public class AzureVisionOptions
{
    public const string SectionName = "AzureVision";

    public string Endpoint { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
}
