using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RiderProfit.Components;
using RiderProfit.Components.Account;
using RiderProfit.Data;
using RiderProfit.Services;
using RiderProfit.Services.Import;
using RiderProfit.Services.Ocr;

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

// Add application services (VehicleService)
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

var app = builder.Build();

// Create or upgrade the database on startup so the app runs without manual setup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
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
