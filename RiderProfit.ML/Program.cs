using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;
// RiderProfit.ML: offline model evaluation (baseline, SDCA) using 5 fold cross validation.
// Run "dotnet run --project RiderProfitML" to see the latest results.
// The Shift Planner page currently uses the SDCA model only; the latest runs
// (including FastTree) have not been connected to the Shift Planner.

// joins path parts for different OS 
var path = Path.Combine(AppContext.BaseDirectory, "rider_data.csv");
var config = new CsvConfiguration(CultureInfo.GetCultureInfo("en-AU")); // read date from D/MM/YYYY to D MMM

using var reader = new StreamReader(path);
using var csv = new CsvReader(reader, config);

// Read every row into a TripRecord object
var trips = csv.GetRecords<TripRecord>().ToList();

// Loop through each trip and calculate the label: earnings per hour
foreach(var t in trips)
{
    double hours = t.ActualDurationSec / 3600.0;
    double perHour = (double)t.TotalAmount / hours;
    Console.WriteLine($"{t.Date:d MMM}{t.AcceptedTime:hh\\:mm}{t.DropoffSuburb, -12} ${perHour:F2}/h");
}
Console.WriteLine($"Loaded{trips.Count} trips");

// Turn trips into ML rows
// features: only what a rider knows before a shift (weekday, hour, suburb).
// distances and duration are excluded (data leakage)
var rows = trips.Select(t => new TripData // LINQ & Lambda expression
{
    Weekday = t.Date.DayOfWeek.ToString(),
    HourOfDay = t.AcceptedTime.Hours, 
    RestaurantSuburb = string.IsNullOrWhiteSpace(t.RestaurantSuburb) ? "Unknown" : t.RestaurantSuburb, 
    EarningsPerHour = (float)((double)t.TotalAmount / (t.ActualDurationSec / 3600.0))
}).ToList();

// entry point of ML.NET with a fixed random seed - makes results repeatable
var ml = new MLContext(0);
// load rows into IDataView for ML.NET
IDataView data = ml.Data.LoadFromEnumerable(rows);

// Baseline evaluation: always predict the mean 
var labels = rows.Select(r => r.EarningsPerHour).ToList();
double mean = labels.Average();
double baseRmse = Math.Sqrt(labels.Average(y => Math.Pow(y - mean, 2)));
double baseMae = labels.Average(y => Math.Abs(y -mean));
Console.WriteLine($"\nBaseline (always ${mean:F2}/h): RMSE={baseRmse:F2} MAE={baseMae:F2} R2=0.00");

// Pipeline: SDCA regression with 5-fold cross validation
var pipeline = ml.Transforms.Categorical.OneHotEncoding("WeekdayEncoded", nameof(TripData.Weekday))
    .Append(ml.Transforms.Categorical.OneHotEncoding("SuburbEncoded", nameof(TripData.RestaurantSuburb)))
    .Append(ml.Transforms.Concatenate("Features", "WeekdayEncoded", "SuburbEncoded", nameof(TripData.HourOfDay)))
    .Append(ml.Transforms.NormalizeMinMax("Features"))
    .Append(ml.Regression.Trainers.Sdca(
        labelColumnName: nameof(TripData.EarningsPerHour),
        featureColumnName: "Features"));

// cross validation k = 5 
var cv = ml.Regression.CrossValidate(
    data, pipeline,
    numberOfFolds: 5,
    labelColumnName: nameof(TripData.EarningsPerHour),
    seed: 0);

// FastTree Pipeline: tree based trainer, capture non-linear patterns (no normalisation needed)
var treePipeline = ml.Transforms.Categorical.OneHotEncoding("WeekdayEncoded", nameof(TripData.Weekday))
    .Append(ml.Transforms.Categorical.OneHotEncoding("SuburbEncoded", nameof(TripData.RestaurantSuburb)))
    .Append(ml.Transforms.Concatenate("Featuees", "WeekdayEncoded", "SuburbEncoded", nameof(TripData.HourOfDay)))
    .Append(ml.Regression.Trainers.FastTree(
        labelColumnName: nameof(TripData.EarningsPerHour),
        featureColumnName: "Features"));

// Tree - cross validation k =5 
var cvTree = ml.Regression.CrossValidate(
    data, pipeline,
    numberOfFolds: 5,
    labelColumnName: nameof(TripData.EarningsPerHour), 
    seed: 0);

// calculate average RMSE, MAE, and R^2 across 5 folds
Console.WriteLine($"Sdca 5-fold: RMSE={cv.Average(f => f.Metrics.RootMeanSquaredError):F2} " +
                  $"MAE={cv.Average(f => f.Metrics.MeanAbsoluteError):F2}  " +
                  $"R2={cv.Average(f => f.Metrics.RSquared):F2}" );
Console.WriteLine($"FastTree 5-fold: RMSE={cvTree.Average(f => f.Metrics.RootMeanSquaredError):F2} " +
                  $"MAE={cvTree.Average(f => f.Metrics.MeanAbsoluteError):F2} " +
                  $"R2={cvTree.Average(f => f.Metrics.RSquared):F2}" );

public class TripRecord
{
    public DateTime Date {get; set; }
    public TimeSpan AcceptedTime {get; set; }
    public string OrderType {get; set;} = "";
    public string StoreName {get; set;} = "";
    public string? PickupStreet {get; set; }
    public string? RestaurantSuburb {get; set; }
    public string? DropoffStreet {get; set; }
    public string DropoffSuburb {get; set; } = "";
    public string? DropoffPostcode {get; set; }
    public int ActualDurationSec {get; set; }
    public decimal DistanceKm {get; set; }
    public decimal TotalAmount {get; set; }
    public decimal Tip {get; set; }
    public string? Notes {get; set; }

}

public class TripData
{
    public string Weekday {get; set; } = "";
    public float HourOfDay {get; set; }
    public string RestaurantSuburb {get; set;} ="";
    public float EarningsPerHour {get; set; }
}


