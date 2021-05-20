using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Business.Wrappers
{
    public interface IValidationHandler<TIn> : IRequestHandler<TIn, ValidationResult>
        where TIn : IValidationRequest
    { }
}