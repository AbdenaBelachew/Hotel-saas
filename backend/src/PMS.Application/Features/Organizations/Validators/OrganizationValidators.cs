using FluentValidation;
using PMS.Application.Features.Organizations.Dtos;

namespace PMS.Application.Features.Organizations.Validators;

public class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).Matches("^[A-Za-z0-9-]+$")
            .WithMessage("Code may only contain letters, digits and hyphens.");
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Timezone).NotEmpty();
    }
}

public class UpdateOrganizationRequestValidator : AbstractValidator<UpdateOrganizationRequest>
{
    public UpdateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Timezone).NotEmpty();
    }
}
