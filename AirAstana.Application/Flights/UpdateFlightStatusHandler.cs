using Application.Caching;
using Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Flights;

public class UpdateFlightStatusHandler : IRequestHandler<UpdateFlightStatusCommand>
{
    private readonly AirAstanaDbContext _db;
    private readonly IFlightCacheService _cache;
    private readonly ILogger<UpdateFlightStatusHandler> _logger;

    public UpdateFlightStatusHandler(
        AirAstanaDbContext db,
        IFlightCacheService cache,
        ILogger<UpdateFlightStatusHandler> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateFlightStatusCommand request, CancellationToken cancellationToken)
    {
        var flight = await _db.Flights.FindAsync(new object[] { request.Id }, cancellationToken)
                     ?? throw new KeyNotFoundException($"Flight {request.Id} not found");

        var previous = flight.Status;
        flight.Status = request.Status;
        await _db.SaveChangesAsync(cancellationToken);

        var list = await _db.Flights.AsNoTracking().ToListAsync(cancellationToken);
        await _cache.SetAsync(list.OrderBy(f => f.Arrival), cancellationToken);

        _logger.LogInformation(
            "{User} at {Time}: updated flight {FlightId} status {Previous} to {Status}",
            request.Username,
            DateTimeOffset.UtcNow,
            flight.Id,
            previous,
            flight.Status);

        return Unit.Value;
    }
}
