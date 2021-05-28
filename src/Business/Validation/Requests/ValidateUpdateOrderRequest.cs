using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Business.Wrappers;

namespace Business.Validation.Requests
{
    public class ValidateUpdateOrderRequest : IValidationRequest
    {
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
    }

    public class ValidateUpdateOrderRequestHandler : IValidationHandler<ValidateUpdateOrderRequest>
    {
        public async Task<ValidationResult> Handle(ValidateUpdateOrderRequest request, CancellationToken cancellationToken)
        {
            if (request.Quantity != null && request.Quantity <= 0)
                return new ValidationResult("Quantity must be more than 0");

            if (request.Price != null && request.Price <= 0)
                return new ValidationResult("Price must be more than 0");

            return ValidationResult.Success;
        }
    }
}