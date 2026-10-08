using RiderProfit.Models;
namespace RiderProfit.Services;

public interface IPlatformService
{
    Task<List<Platform>> GetPlatformsAsync(string userId);

}
