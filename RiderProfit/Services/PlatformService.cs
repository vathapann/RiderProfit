using Microsoft.EntityFrameworkCore;
using RiderProfit.Data;
using RiderProfit.Models;

namespace RiderProfit.Services;

public class PlatformService(IDbContextFactory<ApplicationDbContext> dbFactory) : IPlatformService
{
    // Built-in platforms (UserId = null) plus the platforms this rider added themselves
    public async Task<List<Platform>> GetPlatformsAsync(string userId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        return await db.Platforms
            .Where(p => p.UserId == null || p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
