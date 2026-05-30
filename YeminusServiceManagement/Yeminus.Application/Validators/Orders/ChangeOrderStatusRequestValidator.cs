using FluentValidation;
using Yeminus.Application.DTOs.Orders;
using Yeminus.Domain.Enums;

namespace Yeminus.Application.Validators.Orders;

public class ChangeOrderStatusRequestValidator : AbstractValidator<ChangeOrderStatusRequest>
{
    public ChangeOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid OrderStatus value (1=Pending, 2=InProgress, 3=Completed).");
    }
}
