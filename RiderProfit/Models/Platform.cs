namespace RiderProfit.Models;

// A delivery platform. Built-in platforms (UserId = null) are shared by everyone;
// platforms a rider adds themselves belong only to that rider.
public class Platform
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public List<Shift> Shifts { get; set; } = new();
}
