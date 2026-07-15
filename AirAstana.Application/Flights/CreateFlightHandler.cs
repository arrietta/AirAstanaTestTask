using Domain.Models;
using Application.Caching;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Flights;

public class CreateFlightHandler : IRequestHandler<CreateFlightCommand, Flight>
{
    private readonly AirAstanaDbContext _db;
    private readonly IFlightCacheService _cache;
    private readonly ILogger<CreateFlightHandler> _logger;

    public CreateFlightHandler(
        AirAstanaDbContext db,
        IFlightCacheService cache,
        ILogger<CreateFlightHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Flight> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
    {
        var flight = new Flight
        {
            Origin = request.Origin,
            Destination = request.Destination,
            Departure = request.Departure,
            Arrival = request.Arrival,
            Status = request.Status
        };

        _db.Flights.Add(flight);
        await _db.SaveChangesAsync(cancellationToken);

        await RefreshCacheAsync(cancellationToken);

        _logger.LogInformation(
            "{User} at {Time}: created flight {FlightId} ({Origin} to {Destination}, {Status})",
            request.Username,
            DateTimeOffset.UtcNow,
            flight.Id,
            flight.Origin,
            flight.Destination,
            flight.Status);

        return flight;
    }

    private async Task RefreshCacheAsync(CancellationToken cancellationToken)
    {
        var list = await _db.Flights.AsNoTracking().ToListAsync(cancellationToken);
        await _cache.SetAsync(list.OrderBy(f => f.Arrival), cancellationToken);
    }
}
