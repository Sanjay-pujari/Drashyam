using FluentValidation;
using Drashyam.API.DTOs;

namespace Drashyam.API.Validators;

public class CreateInviteValidator : AbstractValidator<CreateInviteDto>
{
    public CreateInviteValidator()
    {
        RuleFor(x => x.InviteeEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.InviteeFirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s]*$").WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.InviteeLastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s]*$").WithMessage("Last name can only contain letters and spaces");

        RuleFor(x => x.PersonalMessage)
            .MaximumLength(500).WithMessage("Personal message cannot exceed 500 characters");

        RuleFor(x => x.ExpirationDays)
            .InclusiveBetween(1, 30).WithMessage("Expiration days must be between 1 and 30");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid invite type");
    }
}

public class AcceptInviteValidator : AbstractValidator<AcceptInviteDto>
{
    public AcceptInviteValidator()
    {
        RuleFor(x => x.InviteToken)
            .NotEmpty().WithMessage("Invite token is required")
            .Length(16, 16).WithMessage("Invite token must be exactly 16 characters")
            .Matches(@"^[a-zA-Z0-9]*$").WithMessage("Invite token can only contain alphanumeric characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(2, 100).WithMessage("First name must be between 2 and 100 characters")
            .Matches(@"^[a-zA-Z\s]*$").WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(2, 100).WithMessage("Last name must be between 2 and 100 characters")
            .Matches(@"^[a-zA-Z\s]*$").WithMessage("Last name can only contain letters and spaces");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]").WithMessage("Password must contain at least one lowercase letter, one uppercase letter, one digit, and one special character");
    }
}

public class BulkInviteValidator : AbstractValidator<BulkInviteDto>
{
    public BulkInviteValidator()
    {
        RuleFor(x => x.Invites)
            .NotEmpty().WithMessage("At least one invite is required")
            .Must(invites => invites.Count <= 50).WithMessage("Cannot send more than 50 invites at once");

        RuleForEach(x => x.Invites)
            .SetValidator(new CreateInviteValidator());
    }
}
