using MediatR;

namespace Business.Wrappers
{
    public interface IRequestWrapper<T> : IRequest<BusinessResponse<T>>
    { }
}