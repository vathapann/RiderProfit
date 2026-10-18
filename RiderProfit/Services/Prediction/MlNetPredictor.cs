using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;

namespace RiderProfit.Services;

public class MlNetPredictor : IShiftPredictor
{
    private readonly MLContext _ml = new(seed: 0); // fixed seed = repeatable results
    private readonly ITransformer _model; // trained model 
    public IReadOnlyList<string> KnownSuburbs {get; }
    public MlNetPredictor()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "MlData", "rider_data.csv");
        var rows = ToTripData(LoadTrips(path));

        KnownSuburbs = rows.Select(r => r.RestaurantSuburb).Distinct().OrderBy(s => s).ToList(); 
        _model = Train(rows);
    }

    public float Predict(DayOfWeek day, int hour, string suburb)
    {
        // prediction engine is not thread-safe, instantiate a new one per call
        var engine = _ml.Model.CreatePredictionEngine<TripData, TripPrediction>(_model);
        var input = new TripData
        {
            Weekday = day.ToString(),
            HourOfDay = hour,
            RestaurantSuburb = suburb
        };
        return engine.Predict(input).Score;
    }

    // Read the csv file into TripRecord objects
    private static List<TripRecord> LoadTrips(string path)
    {
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("en-AU"));
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<TripRecord>().ToList();
    }
    // Turn raw records into training rows (label is earnings per delivery hour)
    private static List<TripData> ToTripData(IEnumerable<TripRecord> trips)
    {
        return trips
            .Where(t => t.ActualDurationSec > 0) //avoid dviding by 0
            .Select(t => new TripData
            {
                Weekday = t.Date.DayOfWeek.ToString(),
                HourOfDay = t.AcceptedTime.Hours,
                RestaurantSuburb = string.IsNullOrWhiteSpace(t.RestaurantSuburb) ? "Unknown" : t.RestaurantSuburb,
                EarningsPerHour = (float)((double)t.TotalAmount / (t.ActualDurationSec/3600.0)),
            })
            .ToList();
    }

    // Build the same pipeline as the console project, then fit on all the data    
    private ITransformer Train(IEnumerable<TripData> rows)
    {
        IDataView data = _ml.Data.LoadFromEnumerable(rows);
        var pipeline = _ml.Transforms.Categorical.OneHotEncoding("WeekdayEncoded",nameof(TripData.Weekday))
            .Append(_ml.Transforms.Categorical.OneHotEncoding("SuburbEncoded", nameof(TripData.RestaurantSuburb)))
            .Append(_ml.Transforms.Concatenate("Features", "WeekdayEncoded", "SuburbEncoded", nameof(TripData.HourOfDay)))
            .Append(_ml.Transforms.NormalizeMinMax("Features"))
            .Append(_ml.Regression.Trainers.Sdca(
                labelColumnName: nameof(TripData.EarningsPerHour),
                featureColumnName: "Features"));
        return pipeline.Fit(data); // fit model here (not in console)
    }

}
