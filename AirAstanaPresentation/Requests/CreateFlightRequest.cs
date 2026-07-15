using Domain.Enums;

namespace Presentation.Requests;

public class CreateFlightRequest
{
    public string Origin { get; set; } = null!;
    public string Destination { get; set; } = null!;
    public DateTimeOffset Departure { get; set; }
    public DateTimeOffset Arrival { get; set; }
    public FlightStatus Status { get; set; }
}
