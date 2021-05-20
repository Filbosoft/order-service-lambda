using System;
using System.Threading;
using System.Threading.Tasks;
using Business;
using MediatR;

namespace API.Infrastructure.Pipeline
 {
    public class RequestTrackingPipe<TIn, TOut> : IPipelineBehavior<TIn, TOut>
    {
        public async Task<TOut> Handle(TIn request, CancellationToken cancellationToken, RequestHandlerDelegate<TOut> next)
        {
            if (request is BusinessRequest br) 
                br.RequestedAt = DateTime.UtcNow;

            return await next();
        }
    }
}