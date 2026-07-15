using System.Security.Claims;
using Application.Flights;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Requests;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FlightsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get flights.</summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get([FromQuery] string? origin, [FromQuery] string? destination)
    {
        var list = await _mediator.Send(new GetFlightsQuery(origin, destination));
        return Ok(list);
    }

    /// <summary>Create new flight. Moderator only.</summary>
    [HttpPost]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> Create([FromBody] CreateFlightRequest req)
    {
        var flight = await _mediator.Send(new CreateFlightCommand(
            req.Origin,
            req.Destination,
            req.Departure,
            req.Arrival,
            req.Status,
            CurrentUsername()));
        return CreatedAtAction(nameof(Get), new { id = flight.Id }, flight);
    }

    /// <summary>Update flight status. Moderator only.</summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Moderator")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateFlightStatusRequest req)
    {
        await _mediator.Send(new UpdateFlightStatusCommand(id, req.Status, CurrentUsername()));
        return NoContent();
    }

    private string CurrentUsername() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.Identity?.Name
        ?? "anonymous";
}
