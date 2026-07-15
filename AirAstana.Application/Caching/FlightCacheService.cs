using System.Text.Json;
using Domain.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Application.Caching;

public class FlightCacheService : IFlightCacheService
{
    private const string CacheKey = "flights_all";
    private readonly IDistributedCache _cache;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FlightCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<IReadOnlyList<Flight>?> GetAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetStringAsync(CacheKey, cancellationToken);
        if (cached is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<Flight>>(cached, JsonOptions) ?? new List<Flight>();
    }

    public async Task SetAsync(IEnumerable<Flight> flights, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(flights.ToList(), JsonOptions);
        await _cache.SetStringAsync(CacheKey, payload, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        }, cancellationToken);
    }
}
