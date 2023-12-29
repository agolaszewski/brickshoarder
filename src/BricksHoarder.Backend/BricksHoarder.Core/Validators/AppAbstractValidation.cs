using FluentValidation;
using FluentValidation.Results;

namespace BricksHoarder.Core.Validators
{
    public abstract class AppAbstractValidation<TModel> : AbstractValidator<TModel>
    {
        public override ValidationResult Validate(ValidationContext<TModel> context)
        {
            return context.InstanceToValidate == null
                ? new ValidationResult(new[] { new ValidationFailure(nameof(TModel), "Request cannot be null") })
                : base.Validate(context);
        }

        protected override void EnsureInstanceNotNull(object instanceToValidate)
        {
        }
    }
}