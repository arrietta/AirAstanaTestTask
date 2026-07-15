using FluentValidation;
using Presentation.Requests;

namespace Presentation.Validators;

public class UpdateFlightStatusRequestValidator : AbstractValidator<UpdateFlightStatusRequest>
{
    public UpdateFlightStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
