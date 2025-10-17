using FluentValidation;
using Drashyam.API.DTOs;

namespace Drashyam.API.Validators;

public class CreateReferralValidator : AbstractValidator<CreateReferralDto>
{
    public CreateReferralValidator()
    {
        RuleFor(x => x.ReferredUserId)
            .NotEmpty().WithMessage("Referred user ID is required")
            .Length(1, 450).WithMessage("Referred user ID must be between 1 and 450 characters")
            .Matches(@"^[a-zA-Z0-9\-_]*$").WithMessage("Referred user ID can only contain alphanumeric characters, hyphens, and underscores");

        RuleFor(x => x.ReferralCode)
            .MaximumLength(100).WithMessage("Referral code cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9]*$").WithMessage("Referral code can only contain alphanumeric characters")
            .When(x => !string.IsNullOrEmpty(x.ReferralCode));
    }
}

public class CreateReferralCodeValidator : AbstractValidator<CreateReferralCodeDto>
{
    public CreateReferralCodeValidator()
    {
        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9]*$").WithMessage("Code can only contain alphanumeric characters")
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.MaxUsage)
            .GreaterThan(0).WithMessage("Max usage must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Max usage cannot exceed 10,000")
            .When(x => x.MaxUsage.HasValue);

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.RewardAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Reward amount cannot be negative")
            .LessThanOrEqualTo(1000).WithMessage("Reward amount cannot exceed $1,000")
            .When(x => x.RewardAmount.HasValue);

        RuleFor(x => x.RewardType)
            .MaximumLength(50).WithMessage("Reward type cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s]*$").WithMessage("Reward type can only contain letters and spaces")
            .When(x => !string.IsNullOrEmpty(x.RewardType));
    }
}

public class ClaimRewardValidator : AbstractValidator<ClaimRewardDto>
{
    public ClaimRewardValidator()
    {
        RuleFor(x => x.RewardId)
            .GreaterThan(0).WithMessage("Reward ID must be greater than 0");
    }
}
