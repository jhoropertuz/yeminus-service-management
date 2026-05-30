using FluentValidation;
using Yeminus.Application.DTOs.Technicians;

namespace Yeminus.Application.Validators.Technicians;

public class UpdateTechnicianRequestValidator : AbstractValidator<UpdateTechnicianRequest>
{
    public UpdateTechnicianRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Document number is required.")
            .MaximumLength(20).WithMessage("Document number must not exceed 20 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^\+?[0-9\s\-\(\)]{7,20}$").WithMessage("Phone format is invalid.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("Specialty is required.")
            .MaximumLength(100).WithMessage("Specialty must not exceed 100 characters.");
    }
}
