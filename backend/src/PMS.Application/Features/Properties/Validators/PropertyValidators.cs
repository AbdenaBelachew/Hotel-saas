using FluentValidation;
using PMS.Application.Features.Properties.Dtos;

namespace PMS.Application.Features.Properties.Validators;

public class CreatePropertyRequestValidator : AbstractValidator<CreatePropertyRequest>
{
    public CreatePropertyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9-]+$")
            .WithMessage("Code may only contain letters, digits and hyphens.");
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Timezone).NotEmpty();
        RuleFor(x => x.CheckOutTime).NotEqual(default(TimeOnly));
    }
}

public class UpdatePropertyRequestValidator : AbstractValidator<UpdatePropertyRequest>
{
    public UpdatePropertyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PropertyType).IsInEnum();
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Timezone).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
