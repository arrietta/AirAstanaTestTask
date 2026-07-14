using Domain.Enums;

namespace Domain.Models;

public class Flight
{
    public int Id { get; set; }

    public string Origin { get; set; } = null!;

    public string Destination { get; set; } = null!;

    public DateTimeOffset Departure { get; set; }

    public DateTimeOffset Arrival { get; set; }

    public FlightStatus Status { get; set; }
}