using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiderProfit.Components;
using RiderProfit.Components.Account;
using RiderProfit.Data;
using RiderProfit.Services;
using RiderProfit.Services.Export;
using RiderProfit.Services.ExternalAPIs;
using RiderProfit.Services.ExternalAPIs.Ocr;
using RiderProfit.Services.Import;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState(); // a built-in component that provides the current authentication state to descendant components
builder.Services.AddScoped<IdentityUserAccessor>(); // a custom service that provides the current Identity user to descendant components
builder.Services.AddScoped<IdentityRedirectManager>(); // a custom service that provides the ability to redirect to the login page when the user is not authenticated
builder.Services.AddScoped<
    AuthenticationStateProvider,
    IdentityRevalidatingAuthenticationStateProvider
>();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder
    .Services.AddIdentityCore<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = false
    )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add application services (e.g VehicleService)
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IPlatformService, PlatformService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<ITripService, TripService>();

// OCR for screenshot import. Endpoint and key come from user secrets ("AzureVision" section).
builder.Services.Configure<AzureVisionOptions>(
    builder.Configuration.GetSection(AzureVisionOptions.SectionName)
);
builder.Services.AddSingleton<IOcrService, AzureVisionOcrService>();
builder.Services.AddSingleton<UberEatsScreenshotParser>();
builder.Services.AddScoped<ITripImporter, UberEatsScreenshotImporter>();

builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddSingleton<IShiftPredictor, MlNetPredictor>();

// Trip conditions (public holidays and rain) from free APIs that need no key.
// Results are cached in memory so repeated lookups don't call the APIs again.
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IHolidayService, NagerHolidayService>(client =>
{
    client.BaseAddress = new Uri("https://date.nager.at/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<IWeatherService, OpenMeteoWeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<ITripConditionsService, TripConditionsService>();


// Exports saved trips as CSV (same columns as MlData/rider_data.csv)
builder.Services.AddScoped<ITripCsvExportService, TripCsvExportService>();

var app = builder.Build();

// Create or upgrade the database on startup so the app runs without manual setup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Command-line trip export:
//   dotnet run --project RiderProfit -- export-trips <file.csv>
// Writes every saved trip in the rider_data.csv format, then exits.
if (args.Length > 0 && args[0] == "export-trips")
{
    var outputPath = Path.GetFullPath(args.Length > 1 ? args[1] : "trips-export.csv");

    using var scope = app.Services.CreateScope();
    var exporter = scope.ServiceProvider.GetRequiredService<ITripCsvExportService>();
    await File.WriteAllTextAsync(outputPath, await exporter.ExportTripsAsync());

    Console.WriteLine($"Exported trips to {outputPath}");
    return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
