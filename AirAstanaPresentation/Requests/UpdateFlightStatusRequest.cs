using Domain.Enums;

namespace Presentation.Requests;

public class UpdateFlightStatusRequest
{
    public FlightStatus Status { get; set; }
}
