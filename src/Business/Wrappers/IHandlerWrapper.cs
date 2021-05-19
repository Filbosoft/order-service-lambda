using MediatR;

namespace Business.Wrappers
{
    public interface IHandlerWrapper<TIn,TOut> : IRequestHandler<TIn, BusinessResponse<TOut>>
        where TIn : IRequestWrapper<TOut>
    {}
}