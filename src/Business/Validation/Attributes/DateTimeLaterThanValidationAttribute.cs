using System;
using System.ComponentModel.DataAnnotations;

namespace Business.Validation.Attributes
{
    public class DateTimeLaterThanUTCNowValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var dateTimeValue = Convert.ToDateTime(value);
            if (dateTimeValue <= DateTime.UtcNow)
                return new ValidationResult($"{validationContext.DisplayName} must be later than now");
            
            return ValidationResult.Success;
        }
    }
}