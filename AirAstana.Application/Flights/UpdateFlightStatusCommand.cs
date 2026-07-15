using Domain.Enums;
using MediatR;

namespace Application.Flights;

public record UpdateFlightStatusCommand(int Id, FlightStatus Status, string Username) : IRequest;
