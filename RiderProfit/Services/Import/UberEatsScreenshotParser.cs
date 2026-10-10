using System.Globalization;
using System.Text.RegularExpressions;
using RiderProfit.Models;
using RiderProfit.Services.Ocr;

namespace RiderProfit.Services.Import;

// Turns the OCR text of an Uber Eats "Earnings activity" screenshot into trips.
//
// Each trip card on the screen reads, top to bottom:
//   $7.67                                   9:21 pm      <- amount and time on one row
//   DELIVERY · 15 min 57 sec · 8.03 km                   <- duration and distance
//   (map labels such as "Sydney" or "M31")               <- noise, ignored
//   Black Label Cafe And Desserts (Rocky Point Rd)       <- pickup
//   Willarong Road, Caringbah NSW 2229, Australia        <- drop-off
//
// This class only works with text, not Azure or the database, so it can be unit tested.
public partial class UberEatsScreenshotParser
{
    // Lines whose tops are this close (in pixels) are treated as one row of the screen
    private const int SameRowTolerance = 12;

    public List<Trip> Parse(List<OcrLine> lines, DateTime shiftStart, DateTime shiftEnd)
    {
        var rows = GroupIntoRows(lines);

        // Every trip card starts with a row containing a dollar amount.
        // Rows above the first amount belong to a card cut off at the top of the screenshot.
        var cardStarts = rows
            .Select((row, index) => new { row, index })
            .Where(x => x.row.Any(line => AmountPattern().IsMatch(line.Text)))
            .Select(x => x.index)
            .ToList();

        var trips = new List<Trip>();

        for (var i = 0; i < cardStarts.Count; i++)
        {
            var end = i + 1 < cardStarts.Count ? cardStarts[i + 1] : rows.Count;
            var cardLines = rows.Skip(cardStarts[i]).Take(end - cardStarts[i]).SelectMany(row => row).ToList();

            var trip = ParseCard(cardLines, shiftStart, shiftEnd);
            if (trip is not null)
            {
                trips.Add(trip);
            }
        }

        return trips;
    }

    // Returns null when the card is not a complete delivery (e.g. cut off at the bottom of the screenshot)
    private static Trip? ParseCard(List<OcrLine> cardLines, DateTime shiftStart, DateTime shiftEnd)
    {
        var amountMatch = cardLines.Select(l => AmountPattern().Match(l.Text)).First(m => m.Success);
        var timeMatch = cardLines.Select(l => TimePattern().Match(l.Text)).FirstOrDefault(m => m.Success);

        var deliveryIndex = cardLines.FindIndex(l => DeliveryPattern().IsMatch(l.Text));
        if (timeMatch is null || deliveryIndex < 0)
        {
            return null;
        }

        var delivery = DeliveryPattern().Match(cardLines[deliveryIndex].Text);
        var startTime = ToShiftDateTime(timeMatch.Value, shiftStart, shiftEnd);

        var trip = new Trip
        {
            Earnings = decimal.Parse(amountMatch.Groups["amount"].Value, CultureInfo.InvariantCulture),
            DistanceKm = decimal.Parse(delivery.Groups["km"].Value, CultureInfo.InvariantCulture),
            StartTime = startTime,
            EndTime = startTime + ParseDuration(delivery.Groups["duration"].Value),
        };

        // The drop-off is the last address on the card and the pickup is the line just above it.
        // Looking only below the DELIVERY line skips the amount and time.
        var afterDelivery = cardLines.Skip(deliveryIndex + 1).ToList();
        var dropoffIndex = afterDelivery.FindLastIndex(l => AddressPattern().IsMatch(l.Text));

        if (dropoffIndex >= 0)
        {
            var dropoff = AddressPattern().Match(afterDelivery[dropoffIndex].Text);
            trip.DropoffName = dropoff.Groups["street"].Value.Trim();
            trip.DropoffSuburb = dropoff.Groups["suburb"].Value.Trim();

            if (dropoffIndex > 0)
            {
                var pickupText = afterDelivery[dropoffIndex - 1].Text.Trim();
                var pickupAddress = AddressPattern().Match(pickupText);

                // A pickup is usually a restaurant name; if it's an address, keep just the street and suburb
                trip.PickupName = pickupAddress.Success ? pickupAddress.Groups["street"].Value.Trim() : pickupText;
                trip.PickupSuburb = pickupAddress.Success ? pickupAddress.Groups["suburb"].Value.Trim() : string.Empty;
            }
        }

        return trip;
    }

    // Groups OCR lines into screen rows, because a card's time can sit a pixel above its amount
    private static List<List<OcrLine>> GroupIntoRows(List<OcrLine> lines)
    {
        var rows = new List<List<OcrLine>>();

        foreach (var line in lines.OrderBy(l => l.Top).ThenBy(l => l.Left))
        {
            var currentRow = rows.LastOrDefault();
            if (currentRow is not null && line.Top - currentRow[0].Top <= SameRowTolerance)
            {
                currentRow.Add(line);
            }
            else
            {
                rows.Add([line]);
            }
        }

        return rows;
    }

    // Screenshots only show the time, so the date comes from the shift.
    // For a shift that runs past midnight, times earlier than its start belong to the next day.
    private static DateTime ToShiftDateTime(string timeText, DateTime shiftStart, DateTime shiftEnd)
    {
        var time = DateTime.ParseExact(
            timeText.Replace(" ", "").ToUpperInvariant(),
            "h:mmtt",
            CultureInfo.InvariantCulture);

        var date = shiftStart.Date;
        if (shiftEnd.Date > shiftStart.Date && time.TimeOfDay < shiftStart.TimeOfDay)
        {
            date = date.AddDays(1);
        }

        return date + time.TimeOfDay;
    }

    // Reads durations such as "15 min 57 sec" or "1 hr 12 min"
    private static TimeSpan ParseDuration(string text)
    {
        int Part(string unit)
        {
            var match = Regex.Match(text, $@"(\d+)\s*{unit}");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        return new TimeSpan(Part("hr"), Part("min"), Part("sec"));
    }

    // "$7.67" or "$29.46" on its own
    [GeneratedRegex(@"^\$(?<amount>\d+(?:\.\d{2})?)$")]
    private static partial Regex AmountPattern();

    // "9:21 pm" or "12:05 AM"
    [GeneratedRegex(@"^\d{1,2}:\d{2}\s*[ap]m$", RegexOptions.IgnoreCase)]
    private static partial Regex TimePattern();

    // "DELIVERY · 15 min 57 sec · 8.03 km". OCR sometimes reads the "·" separator as "." or "•".
    [GeneratedRegex(@"DELIVERY\s*[·.•]\s*(?<duration>.+?)\s*[·.•]\s*(?<km>\d+(?:\.\d+)?)\s*km", RegexOptions.IgnoreCase)]
    private static partial Regex DeliveryPattern();

    // "Willarong Road, Caringbah NSW 2229, Australia" → street "Willarong Road", suburb "Caringbah"
    [GeneratedRegex(@"^(?<street>[^,]+),\s*(?<suburb>[A-Za-z' .-]+?)\s+(?:NSW|VIC|QLD|SA|WA|TAS|NT|ACT)\s+\d{4}")]
    private static partial Regex AddressPattern();
}
