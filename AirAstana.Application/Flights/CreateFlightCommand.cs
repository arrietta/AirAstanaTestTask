using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Flights;

public record CreateFlightCommand(
    string Origin,
    string Destination,
    DateTimeOffset Departure,
    DateTimeOffset Arrival,
    FlightStatus Status,
    string Username) : IRequest<Flight>;
