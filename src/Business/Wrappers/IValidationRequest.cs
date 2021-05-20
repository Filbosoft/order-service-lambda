using System.ComponentModel.DataAnnotations;
using MediatR;

namespace Business.Wrappers
{
    public interface IValidationRequest : IRequest<ValidationResult>
    { }
}