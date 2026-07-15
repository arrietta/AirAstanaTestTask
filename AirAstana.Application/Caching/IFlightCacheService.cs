using Domain.Models;

namespace Application.Caching;

public interface IFlightCacheService
{
    Task<IReadOnlyList<Flight>?> GetAsync(CancellationToken cancellationToken = default);

    Task SetAsync(IEnumerable<Flight> flights, CancellationToken cancellationToken = default);
}
