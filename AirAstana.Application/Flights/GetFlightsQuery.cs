using Domain.Models;
using MediatR;

namespace Application.Flights;

public record GetFlightsQuery(string? Origin, string? Destination) : IRequest<List<Flight>>;