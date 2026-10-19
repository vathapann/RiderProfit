using RiderProfit.Models;

namespace RiderProfit.Tests;

// Tests the totals a Shift calculates from its trips. The shift page and the profit dashboard rely on these.
[TestFixture]
public class ShiftTotalsTests
{
    private Shift shift = null!;

    [SetUp]
    public void SetUp()
    {
        // A 5 pm – 9 pm shift with two trips
        shift = new Shift
        {
            StartTime = new DateTime(2026, 10, 9, 17, 0, 0),
            EndTime = new DateTime(2026, 10, 9, 21, 0, 0),
            Trips =
            [
                new Trip { Earnings = 7.67m, Tips = 0m, DistanceKm = 8.03m },
                new Trip { Earnings = 29.46m, Tips = 2.50m, DistanceKm = 19.63m },
            ],
        };
    }

    [Test]
    public void Totals_AddUpAllTrips()
    {
        Assert.Multiple(() =>
        {
            Assert.That(shift.TripCount, Is.EqualTo(2));
            Assert.That(shift.Earnings, Is.EqualTo(37.13m));
            Assert.That(shift.Tips, Is.EqualTo(2.50m));
            Assert.That(shift.TotalDistanceKm, Is.EqualTo(27.66m));
        });
    }

    [Test]
    public void Duration_IsTheWholeShiftIncludingWaitingTime()
    {
        Assert.That(shift.Duration, Is.EqualTo(TimeSpan.FromHours(4)));
    }

    [Test]
    public void Totals_ShiftWithNoTrips_AreZero()
    {
        var emptyShift = new Shift();

        Assert.Multiple(() =>
        {
            Assert.That(emptyShift.TripCount, Is.Zero);
            Assert.That(emptyShift.Earnings, Is.Zero);
            Assert.That(emptyShift.TotalDistanceKm, Is.Zero);
        });
    }
}
