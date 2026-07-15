using FluentValidation;
using Presentation.Requests;

namespace Presentation.Validators;

public class CreateFlightRequestValidator : AbstractValidator<CreateFlightRequest>
{
    public CreateFlightRequestValidator()
    {
        RuleFor(x => x.Origin).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Departure).NotEmpty();
        RuleFor(x => x.Arrival).NotEmpty();
        RuleFor(x => x.Departure).LessThan(x => x.Arrival).WithMessage("Departure must be before Arrival");
        RuleFor(x => x.Status).IsInEnum();
    }
}
