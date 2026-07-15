using Domain.Models;
using Application.Caching;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Flights;

public class GetFlightsHandler : IRequestHandler<GetFlightsQuery, List<Flight>>
{
    private readonly AirAstanaDbContext _db;
    private readonly IFlightCacheService _cache;
    private readonly ILogger<GetFlightsHandler> _logger;

    public GetFlightsHandler(AirAstanaDbContext db, IFlightCacheService cache, ILogger<GetFlightsHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<Flight>> Handle(GetFlightsQuery request, CancellationToken cancellationToken)
    {
        var flights = await _cache.GetAsync(cancellationToken);

        if (flights is null)
        {
            flights = await LoadFlightsOrderedAsync(cancellationToken);
            await _cache.SetAsync(flights, cancellationToken);
        }

        IEnumerable<Flight> query = flights;
        if (!string.IsNullOrWhiteSpace(request.Origin))
            query = query.Where(f => f.Origin == request.Origin);
        if (!string.IsNullOrWhiteSpace(request.Destination))
            query = query.Where(f => f.Destination == request.Destination);

        return query.OrderBy(f => f.Arrival).ToList();
    }

    private async Task<List<Flight>> LoadFlightsOrderedAsync(CancellationToken cancellationToken)
    {
        var list = await _db.Flights.AsNoTracking().ToListAsync(cancellationToken);
        return list.OrderBy(f => f.Arrival).ToList();
    }
}
